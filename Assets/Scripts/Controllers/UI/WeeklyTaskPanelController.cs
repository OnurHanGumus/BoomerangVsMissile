using System;
using System.Collections.Generic;
using Data.ValueObject;
using DG.Tweening;
using Enums;
using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers.UI
{
    public class WeeklyTaskPanelController : MonoBehaviour
    {
        #region Self Variables

        #region Public Variables
        public CanvasGroup PanelCanvasGroup => canvasGroup;
        #endregion

        #region Serialized Variables
        [Header("Canvas & Container")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform dialogContainer;
        [SerializeField] private Transform cardsContainer;
        [SerializeField] private WeeklyTaskCardUI cardPrefab;

        [Header("Header & Countdown")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private Button closeButton;

        [Header("Grand Prize Section")]
        [SerializeField] private Slider grandPrizeProgressSlider;
        [SerializeField] private TextMeshProUGUI grandPrizeProgressText;
        [SerializeField] private TextMeshProUGUI grandPrizeRewardText;
        [SerializeField] private Button grandPrizeClaimButton;
        [SerializeField] private Image grandPrizeClaimButtonBackground;
        [SerializeField] private TextMeshProUGUI grandPrizeClaimButtonText;

        [Header("Optional Visual Overrides")]
        [SerializeField] private Sprite coinSprite;
        [SerializeField] private TMP_FontAsset fontAsset;
        #endregion

        #region Private Variables
        private readonly List<WeeklyTaskCardUI> _cards = new List<WeeklyTaskCardUI>();
        private float _countdownTimer = 0f;

        private static readonly Color LockedButtonColor = new Color(0.24f, 0.28f, 0.36f, 1f);
        private static readonly Color ClaimableGrandButtonColor = new Color(0.95f, 0.68f, 0.12f, 1f);
        private static readonly Color ClaimedButtonColor = new Color(0.18f, 0.22f, 0.28f, 0.85f);
        #endregion

        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }

        private void Start()
        {
            RegisterToUIPanelController();
            RefreshAllUI();
        }

        private void Init()
        {
            LoadDefaultAssetsIfMissing();
            EnsureProceduralLayoutIfNeeded();
            BindButtons();
        }

        private void LoadDefaultAssetsIfMissing()
        {
            if (coinSprite == null)
            {
                coinSprite = Resources.Load<Sprite>("Art/Sprites/Icons/Icon_Money_Coin");
            }

            if (fontAsset == null)
            {
                fontAsset = Resources.Load<TMP_FontAsset>("Art/Orbitron/Orbitron-VariableFont_wght SDF");
            }

            if (cardPrefab == null)
            {
                cardPrefab = Resources.Load<WeeklyTaskCardUI>("Prefabs/WeeklyTaskCard");
            }
        }

        private void BindButtons()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseButtonClicked);
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }

            if (grandPrizeClaimButton != null)
            {
                grandPrizeClaimButton.onClick.RemoveListener(OnGrandPrizeClaimClicked);
                grandPrizeClaimButton.onClick.AddListener(OnGrandPrizeClaimClicked);
            }
        }

        private void RegisterToUIPanelController()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            UIPanelActivenessController panelController = FindFirstObjectByType<UIPanelActivenessController>();
            if (panelController != null && canvasGroup != null)
            {
                panelController.RegisterPanel(UIPanels.WeeklyTaskPanel, canvasGroup);
            }
        }

        #region Event Subscription
        private void SubscribeEvents()
        {
            if (WeeklyTaskSignals.Instance != null)
            {
                WeeklyTaskSignals.Instance.onWeeklyTasksRefreshed += RefreshAllUI;
                WeeklyTaskSignals.Instance.onTaskProgressChanged += OnTaskProgressChanged;
                WeeklyTaskSignals.Instance.onTaskCompleted += OnTaskCompleted;
            }
        }

        private void UnsubscribeEvents()
        {
            if (WeeklyTaskSignals.Instance != null)
            {
                WeeklyTaskSignals.Instance.onWeeklyTasksRefreshed -= RefreshAllUI;
                WeeklyTaskSignals.Instance.onTaskProgressChanged -= OnTaskProgressChanged;
                WeeklyTaskSignals.Instance.onTaskCompleted -= OnTaskCompleted;
            }
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }
        #endregion

        private void Update()
        {
            if (canvasGroup != null && canvasGroup.alpha <= 0.01f)
            {
                return;
            }

            _countdownTimer += Time.unscaledDeltaTime;
            if (_countdownTimer >= 1f)
            {
                _countdownTimer = 0f;
                UpdateCountdownText();
            }
        }

        private void OnTaskProgressChanged(int slotIndex)
        {
            RefreshAllUI();
        }

        private void OnTaskCompleted(int slotIndex)
        {
            RefreshAllUI();
        }

        public void RefreshAllUI()
        {
            if (WeeklyTaskSignals.Instance == null)
            {
                return;
            }

            List<WeeklyTaskProgressData> tasks = WeeklyTaskSignals.Instance.onGetTasks();
            if (tasks == null)
            {
                return;
            }

            EnsureCardsCount(tasks.Count);

            int completedCount = 0;
            for (int i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                if (task != null && task.IsCompleted)
                {
                    completedCount++;
                }

                string desc = WeeklyTaskSignals.Instance.onGetTaskDescription(i);
                if (i < _cards.Count && _cards[i] != null)
                {
                    _cards[i].BindData(i, task, desc);
                }
            }

            UpdateGrandPrizeUI(completedCount, tasks.Count);
            UpdateCountdownText();
        }

        private void UpdateGrandPrizeUI(int completedCount, int totalCount)
        {
            float normalized = totalCount > 0 ? Mathf.Clamp01((float)completedCount / totalCount) : 0f;
            if (grandPrizeProgressSlider != null)
            {
                grandPrizeProgressSlider.value = normalized;
            }

            if (grandPrizeProgressText != null)
            {
                grandPrizeProgressText.text = $"GRAND PRIZE PROGRESS: {completedCount}/{totalCount}";
            }

            int grandReward = WeeklyTaskSignals.Instance != null ? WeeklyTaskSignals.Instance.onGetGrandPrizeReward() : 1500;
            if (grandPrizeRewardText != null)
            {
                grandPrizeRewardText.text = $"+{grandReward} COINS";
            }

            bool isClaimed = WeeklyTaskSignals.Instance != null && WeeklyTaskSignals.Instance.onIsGrandPrizeClaimed();
            bool isClaimable = WeeklyTaskSignals.Instance != null && WeeklyTaskSignals.Instance.onIsGrandPrizeClaimable();

            if (isClaimed)
            {
                if (grandPrizeClaimButton != null) grandPrizeClaimButton.interactable = false;
                if (grandPrizeClaimButtonBackground != null) grandPrizeClaimButtonBackground.color = ClaimedButtonColor;
                if (grandPrizeClaimButtonText != null) grandPrizeClaimButtonText.text = "CLAIMED";
            }
            else if (isClaimable)
            {
                if (grandPrizeClaimButton != null) grandPrizeClaimButton.interactable = true;
                if (grandPrizeClaimButtonBackground != null) grandPrizeClaimButtonBackground.color = ClaimableGrandButtonColor;
                if (grandPrizeClaimButtonText != null) grandPrizeClaimButtonText.text = "CLAIM PRIZE";
            }
            else
            {
                if (grandPrizeClaimButton != null) grandPrizeClaimButton.interactable = false;
                if (grandPrizeClaimButtonBackground != null) grandPrizeClaimButtonBackground.color = LockedButtonColor;
                if (grandPrizeClaimButtonText != null) grandPrizeClaimButtonText.text = $"{completedCount}/{totalCount}";
            }
        }

        private void UpdateCountdownText()
        {
            if (countdownText == null || WeeklyTaskSignals.Instance == null)
            {
                return;
            }

            long remainingSeconds = WeeklyTaskSignals.Instance.onGetRemainingSeconds();
            TimeSpan span = TimeSpan.FromSeconds(Mathf.Max(0, remainingSeconds));
            countdownText.text = $"RESETS IN: {span.Days}d {span.Hours:D2}h {span.Minutes:D2}m";
        }

        private void OnGrandPrizeClaimClicked()
        {
            if (grandPrizeClaimButton != null)
            {
                grandPrizeClaimButton.transform.DOKill();
                grandPrizeClaimButton.transform.localScale = Vector3.one;
                grandPrizeClaimButton.transform.DOPunchScale(Vector3.one * 0.06f, 0.25f, 6, 0.5f).SetUpdate(true);
            }

            WeeklyTaskSignals.Instance.onClaimGrandPrize?.Invoke();
        }

        private void OnCloseButtonClicked()
        {
            AudioSignals.Instance.onPlaySound?.Invoke(AudioSoundEnums.Click);
            PitchSignals.Instance.onPlayPitch?.Invoke(PitchEnums.Click);

            UISignals.Instance.onClosePanel?.Invoke(UIPanels.WeeklyTaskPanel);
            UISignals.Instance.onOpenPanel?.Invoke(UIPanels.StartPanel);
        }

        #region Card Pool & Procedural Fallback Builder
        private void EnsureCardsCount(int targetCount)
        {
            if (cardsContainer == null)
            {
                return;
            }

            if (_cards.Count == 0)
            {
                var existing = cardsContainer.GetComponentsInChildren<WeeklyTaskCardUI>(true);
                if (existing != null && existing.Length > 0)
                {
                    _cards.AddRange(existing);
                }
            }

            while (_cards.Count < targetCount)
            {
                WeeklyTaskCardUI cardInstance;
                if (cardPrefab != null)
                {
                    cardInstance = Instantiate(cardPrefab, cardsContainer);
                }
                else
                {
                    cardInstance = CreateProceduralCard(cardsContainer, _cards.Count);
                }
                _cards.Add(cardInstance);
            }

            for (int i = 0; i < _cards.Count; i++)
            {
                if (_cards[i] != null)
                {
                    _cards[i].gameObject.SetActive(i < targetCount);
                }
            }
        }

        private void EnsureProceduralLayoutIfNeeded()
        {
            if (cardsContainer != null && closeButton != null && grandPrizeClaimButton != null)
            {
                return;
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            // Build self-contained hierarchy if not wired in scene
            RectTransform rootRect = GetComponent<RectTransform>();
            if (rootRect != null)
            {
                rootRect.anchorMin = Vector2.zero;
                rootRect.anchorMax = Vector2.one;
                rootRect.offsetMin = Vector2.zero;
                rootRect.offsetMax = Vector2.zero;
            }

            Image backdrop = GetComponent<Image>();
            if (backdrop == null)
            {
                backdrop = gameObject.AddComponent<Image>();
                backdrop.color = new Color(0.02f, 0.04f, 0.09f, 0.88f);
            }

            GameObject dialogObj = new GameObject("DialogContainer", typeof(RectTransform), typeof(Image));
            dialogObj.transform.SetParent(transform, false);
            dialogContainer = dialogObj.GetComponent<RectTransform>();
            dialogContainer.anchorMin = new Vector2(0.05f, 0.07f);
            dialogContainer.anchorMax = new Vector2(0.95f, 0.93f);
            dialogContainer.offsetMin = Vector2.zero;
            dialogContainer.offsetMax = Vector2.zero;
            Image dialogBg = dialogObj.GetComponent<Image>();
            dialogBg.color = new Color(0.07f, 0.09f, 0.15f, 0.98f);

            // Header
            GameObject headerObj = new GameObject("Header", typeof(RectTransform));
            headerObj.transform.SetParent(dialogContainer, false);
            RectTransform headerRect = headerObj.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 0.89f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.offsetMin = new Vector2(24f, 8f);
            headerRect.offsetMax = new Vector2(-24f, -12f);

            titleText = CreateTMP(headerRect, "TitleText", "WEEKLY TASKS", 40, TextAlignmentOptions.TopLeft, Color.white);
            titleText.rectTransform.anchorMin = new Vector2(0f, 0.4f);
            titleText.rectTransform.anchorMax = new Vector2(0.8f, 1f);

            countdownText = CreateTMP(headerRect, "CountdownText", "RESETS IN: 6d 23h", 24, TextAlignmentOptions.BottomLeft, new Color(0.55f, 0.82f, 1f, 1f));
            countdownText.rectTransform.anchorMin = new Vector2(0f, 0f);
            countdownText.rectTransform.anchorMax = new Vector2(0.8f, 0.45f);

            // Close Button
            GameObject closeObj = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            closeObj.transform.SetParent(headerRect, false);
            RectTransform closeRect = closeObj.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 0.5f);
            closeRect.anchorMax = new Vector2(1f, 0.5f);
            closeRect.pivot = new Vector2(1f, 0.5f);
            closeRect.sizeDelta = new Vector2(84f, 84f);
            closeRect.anchoredPosition = Vector2.zero;
            closeObj.GetComponent<Image>().color = new Color(0.85f, 0.22f, 0.28f, 1f);
            closeButton = closeObj.GetComponent<Button>();
            CreateTMP(closeRect, "X", "X", 38, TextAlignmentOptions.Center, Color.white);

            // Grand Prize Banner
            GameObject grandObj = new GameObject("GrandPrizeBanner", typeof(RectTransform), typeof(Image));
            grandObj.transform.SetParent(dialogContainer, false);
            RectTransform grandRect = grandObj.GetComponent<RectTransform>();
            grandRect.anchorMin = new Vector2(0.03f, 0.75f);
            grandRect.anchorMax = new Vector2(0.97f, 0.88f);
            grandRect.offsetMin = Vector2.zero;
            grandRect.offsetMax = Vector2.zero;
            grandObj.GetComponent<Image>().color = new Color(0.14f, 0.18f, 0.28f, 1f);

            grandPrizeProgressText = CreateTMP(grandRect, "GrandProgressText", "GRAND PRIZE PROGRESS: 0/6", 24, TextAlignmentOptions.Left, Color.white);
            grandPrizeProgressText.rectTransform.anchorMin = new Vector2(0.03f, 0.55f);
            grandPrizeProgressText.rectTransform.anchorMax = new Vector2(0.68f, 0.95f);

            grandPrizeRewardText = CreateTMP(grandRect, "GrandRewardText", "+1500 COINS", 26, TextAlignmentOptions.Right, new Color(1f, 0.86f, 0.2f, 1f));
            grandPrizeRewardText.rectTransform.anchorMin = new Vector2(0.45f, 0.55f);
            grandPrizeRewardText.rectTransform.anchorMax = new Vector2(0.70f, 0.95f);

            // Progress Slider (matches SPEED_Row Slider structure)
            grandPrizeProgressSlider = CreateStatStyleSlider(
                grandRect,
                "Slider",
                new Vector2(0.03f, 0.15f),
                new Vector2(0.70f, 0.45f),
                new Color(0.10f, 0.14f, 0.22f, 1f),
                new Color(1f, 0.75f, 0.20f, 1f));

            GameObject grandBtnObj = new GameObject("GrandClaimButton", typeof(RectTransform), typeof(Image), typeof(Button));
            grandBtnObj.transform.SetParent(grandRect, false);
            RectTransform grandBtnRect = grandBtnObj.GetComponent<RectTransform>();
            grandBtnRect.anchorMin = new Vector2(0.73f, 0.15f);
            grandBtnRect.anchorMax = new Vector2(0.97f, 0.85f);
            grandBtnRect.offsetMin = Vector2.zero;
            grandBtnRect.offsetMax = Vector2.zero;
            grandPrizeClaimButtonBackground = grandBtnObj.GetComponent<Image>();
            grandPrizeClaimButton = grandBtnObj.GetComponent<Button>();
            grandPrizeClaimButtonText = CreateTMP(grandBtnRect, "BtnText", "0/6", 24, TextAlignmentOptions.Center, Color.white);

            // Cards Container
            GameObject listObj = new GameObject("CardsContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
            listObj.transform.SetParent(dialogContainer, false);
            RectTransform listRect = listObj.GetComponent<RectTransform>();
            listRect.anchorMin = new Vector2(0.03f, 0.03f);
            listRect.anchorMax = new Vector2(0.97f, 0.73f);
            listRect.offsetMin = Vector2.zero;
            listRect.offsetMax = Vector2.zero;
            VerticalLayoutGroup vlg = listObj.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 14f;
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = true;
            cardsContainer = listRect;
        }

        private WeeklyTaskCardUI CreateProceduralCard(Transform parent, int index)
        {
            GameObject cardObj = new GameObject($"WeeklyTaskCard_{index + 1}", typeof(RectTransform), typeof(Image), typeof(WeeklyTaskCardUI));
            cardObj.transform.SetParent(parent, false);
            Image cardBg = cardObj.GetComponent<Image>();
            cardBg.color = new Color(0.11f, 0.14f, 0.22f, 0.96f);

            RectTransform cardRect = cardObj.GetComponent<RectTransform>();

            TextMeshProUGUI descTxt = CreateTMP(cardRect, "DescriptionText", "Task Description", 25, TextAlignmentOptions.Left, Color.white);
            descTxt.rectTransform.anchorMin = new Vector2(0.03f, 0.50f);
            descTxt.rectTransform.anchorMax = new Vector2(0.70f, 0.92f);

            TextMeshProUGUI rewardTxt = CreateTMP(cardRect, "RewardAmountText", "+100", 26, TextAlignmentOptions.Right, new Color(1f, 0.86f, 0.2f, 1f));
            rewardTxt.rectTransform.anchorMin = new Vector2(0.55f, 0.50f);
            rewardTxt.rectTransform.anchorMax = new Vector2(0.71f, 0.92f);

            Slider slider = CreateStatStyleSlider(
                cardRect,
                "Slider",
                new Vector2(0.03f, 0.14f),
                new Vector2(0.71f, 0.42f),
                new Color(0.10f, 0.14f, 0.22f, 1f),
                new Color(0.20f, 0.90f, 0.85f, 1f));

            TextMeshProUGUI countTxt = CreateTMP(slider.GetComponent<RectTransform>(), "ProgressCountText", "0/5", 20, TextAlignmentOptions.Center, Color.white);

            GameObject btnObj = new GameObject("ClaimButton", typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(cardRect, false);
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.74f, 0.16f);
            btnRect.anchorMax = new Vector2(0.97f, 0.84f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            Image btnBg = btnObj.GetComponent<Image>();
            Button btn = btnObj.GetComponent<Button>();
            TextMeshProUGUI btnTxt = CreateTMP(btnRect, "ClaimButtonText", "CLAIM", 22, TextAlignmentOptions.Center, Color.white);

            var cardUI = cardObj.GetComponent<WeeklyTaskCardUI>();
            cardUI.ConfigureReferences(cardBg, descTxt, slider, countTxt, rewardTxt, btn, btnBg, btnTxt);
            return cardUI;
        }

        private Slider CreateStatStyleSlider(
            RectTransform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color backgroundColor,
            Color fillColor)
        {
            GameObject sliderObj = new GameObject(name, typeof(RectTransform), typeof(Slider));
            sliderObj.transform.SetParent(parent, false);
            RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
            sliderRect.anchorMin = anchorMin;
            sliderRect.anchorMax = anchorMax;
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;

            GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(sliderRect, false);
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImg = bgObj.GetComponent<Image>();
            bgImg.sprite = CreateWhiteSprite();
            bgImg.type = Image.Type.Sliced;
            bgImg.color = backgroundColor;

            GameObject fillAreaObj = new GameObject("Fill Area", typeof(RectTransform));
            fillAreaObj.transform.SetParent(sliderRect, false);
            RectTransform fillAreaRect = fillAreaObj.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;

            GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillObj.transform.SetParent(fillAreaRect, false);
            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            Image fillImg = fillObj.GetComponent<Image>();
            fillImg.sprite = CreateWhiteSprite();
            fillImg.type = Image.Type.Sliced;
            fillImg.color = fillColor;

            Slider slider = sliderObj.GetComponent<Slider>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;
            slider.fillRect = fillRect;
            slider.targetGraphic = null;
            slider.value = 0f;
            return slider;
        }

        private TextMeshProUGUI CreateTMP(RectTransform parent, string name, string text, float fontSize, TextAlignmentOptions align, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            if (fontAsset != null)
            {
                tmp.font = fontAsset;
            }
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = color;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 14f;
            tmp.fontSizeMax = fontSize;
            return tmp;
        }

        private static Sprite _cachedWhiteSprite;
        private static Sprite CreateWhiteSprite()
        {
            if (_cachedWhiteSprite == null)
            {
                Texture2D tex = Texture2D.whiteTexture;
                _cachedWhiteSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            return _cachedWhiteSprite;
        }
        #endregion
    }
}
