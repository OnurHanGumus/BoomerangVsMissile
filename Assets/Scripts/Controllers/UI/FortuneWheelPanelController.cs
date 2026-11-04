using System;
using System.Collections;
using System.Collections.Generic;
using Data.UnityObject;
using Data.ValueObject;
using DG.Tweening;
using Enums;
using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers.UI
{
    public class FortuneWheelPanelController : MonoBehaviour
    {
        #region Self Variables

        #region Public Variables
        public CanvasGroup PanelCanvasGroup => canvasGroup;
        public bool IsSpinning => _isSpinning;
        public bool IsAvailable => GetRemainingCooldownSeconds() <= 0L;
        #endregion

        #region Serialized Variables
        [Header("Canvas & Container")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform dialogContainer;

        [Header("Wheel & Pointer")]
        [SerializeField] private RectTransform wheelTransform;
        [SerializeField] private Image wheelImage;
        [SerializeField] private Image pointerImage;
        [SerializeField] private float sectorRadius = 170f;

        [Header("Sector Prefabs Setup")]
        [SerializeField] private FortuneWheelSectorUI coinSectorPrefab;
        [SerializeField] private FortuneWheelSectorUI lifeSectorPrefab;
        [SerializeField] private Transform sectorContainer;
        [SerializeField] private Sprite coinSprite;
        [SerializeField] private Sprite lifeSprite;
        [SerializeField] private TMP_FontAsset fontAsset;

        [Header("Slice Text Colors")]
        [Tooltip("Text color on blue slices (Even indices: 0, 2, 4, 6, 8).")]
        [SerializeField] private Color blueSliceTextColor = Color.white;
        [Tooltip("Text color on white slices (Odd indices: 1, 3, 5, 7, 9) - hex #6FC1F8.")]
        [SerializeField] private Color whiteSliceTextColor = new Color(0.4352941f, 0.7568628f, 0.972549f, 1f);

        [Header("Buttons & Texts")]
        [SerializeField] private Button spinButton;
        [SerializeField] private TextMeshProUGUI spinButtonText;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;

        [Header("Data (Optional Override)")]
        [SerializeField] private CD_FortuneWheel fortuneWheelConfig;
        #endregion

        #region Private Variables
        private FortuneWheelData _data;
        private bool _isSpinning;
        private bool _isRewardPending;
        private int _pendingPrizeSectorIndex = -1;
        private long _lastSpinUtcTicks;
        private float _currentWheelAngle;
        private List<FortuneWheelSectorUI> _spawnedSectors = new List<FortuneWheelSectorUI>();
        private Coroutine _spinCoroutine;
        private float _cooldownUpdateTimer;
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
            EnsureSectorsBuilt();
            UpdateUIState();
        }

        private void Init()
        {
            _data = GetData();
            LoadLastSpinTime();
            BindButtons();
            LoadDefaultSpritesIfMissing();
        }

        private FortuneWheelData GetData()
        {
            if (fortuneWheelConfig != null && fortuneWheelConfig.Data != null)
            {
                return fortuneWheelConfig.Data;
            }

            CD_FortuneWheel loaded = Resources.Load<CD_FortuneWheel>("Data/CD_FortuneWheel");
            if (loaded != null && loaded.Data != null)
            {
                fortuneWheelConfig = loaded;
                return loaded.Data;
            }

            return new FortuneWheelData();
        }

        private void LoadDefaultSpritesIfMissing()
        {
            if (coinSprite == null)
            {
                coinSprite = Resources.Load<Sprite>("Art/Sprites/Icons/Icon_Money_Coin");
            }

            if (lifeSprite == null)
            {
                lifeSprite = Resources.Load<Sprite>("Art/Sprites/Icons/Hearth_icon");
            }

            if (wheelImage != null && wheelImage.sprite == null)
            {
                wheelImage.sprite = Resources.Load<Sprite>("Art/Sprites/Fortune Wheel");
            }

            if (pointerImage != null && pointerImage.sprite == null)
            {
                pointerImage.sprite = Resources.Load<Sprite>("Art/Sprites/Icons/Fortunewheel_pointer");
            }

            if (fontAsset == null)
            {
                fontAsset = Resources.Load<TMP_FontAsset>("Art/Orbitron/Orbitron-VariableFont_wght SDF");
            }

            if (coinSectorPrefab == null)
            {
                coinSectorPrefab = Resources.Load<FortuneWheelSectorUI>("Prefabs/FortuneWheelSector_Coin");
            }

            if (lifeSectorPrefab == null)
            {
                lifeSectorPrefab = Resources.Load<FortuneWheelSectorUI>("Prefabs/FortuneWheelSector_Life");
            }
        }

        private void BindButtons()
        {
            if (spinButton != null)
            {
                spinButton.onClick.RemoveListener(OnSpinButtonClicked);
                spinButton.onClick.AddListener(OnSpinButtonClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseButtonClicked);
                closeButton.onClick.AddListener(OnCloseButtonClicked);
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
                panelController.RegisterPanel(UIPanels.FortuneWheelPanel, canvasGroup);
            }
        }

        #region Event Subscription
        private void SubscribeEvents()
        {
            if (FortuneWheelSignals.Instance != null)
            {
                FortuneWheelSignals.Instance.onOpenFortuneWheel += OpenPanel;
                FortuneWheelSignals.Instance.onCloseFortuneWheel += ClosePanel;
                FortuneWheelSignals.Instance.onGetIsDailyWheelAvailable += OnGetIsDailyWheelAvailable;
                FortuneWheelSignals.Instance.onGetRemainingCooldownSeconds += GetRemainingCooldownSeconds;
            }
        }

        private void UnsubscribeEvents()
        {
            if (FortuneWheelSignals.Instance != null)
            {
                FortuneWheelSignals.Instance.onOpenFortuneWheel -= OpenPanel;
                FortuneWheelSignals.Instance.onCloseFortuneWheel -= ClosePanel;
                FortuneWheelSignals.Instance.onGetIsDailyWheelAvailable -= OnGetIsDailyWheelAvailable;
                FortuneWheelSignals.Instance.onGetRemainingCooldownSeconds -= GetRemainingCooldownSeconds;
            }
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }
        #endregion

        private void Update()
        {
            if (_isSpinning)
            {
                return;
            }

            _cooldownUpdateTimer += Time.unscaledDeltaTime;
            if (_cooldownUpdateTimer >= 1f)
            {
                _cooldownUpdateTimer = 0f;
                UpdateUIState();
            }
        }

        #region UI Layout & Procedural Sectors
        private void EnsureSectorsBuilt()
        {
            if (_data == null || _data.Sectors == null || _data.Sectors.Count == 0)
            {
                return;
            }

            Transform container = sectorContainer != null ? sectorContainer : (wheelTransform != null ? wheelTransform : transform);

            if (_spawnedSectors.Count == _data.Sectors.Count)
            {
                return;
            }

            foreach (var item in _spawnedSectors)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }
            _spawnedSectors.Clear();

            int count = _data.Sectors.Count;
            float degreesPerSector = 360f / count;

            for (int i = 0; i < count; i++)
            {
                FortuneWheelSectorData sectorData = _data.Sectors[i];
                float angleClockwise = i * degreesPerSector;
                float angleRad = angleClockwise * Mathf.Deg2Rad;

                Vector2 sectorPosition = new Vector2(
                    Mathf.Sin(angleRad) * sectorRadius,
                    Mathf.Cos(angleRad) * sectorRadius
                );

                // Alternating slices: Even indices (0, 2, 4, 6, 8) are Blue; Odd indices (1, 3, 5, 7, 9) are White
                bool isWhiteSlice = (i % 2) != 0;
                Color sliceTextColor = isWhiteSlice ? whiteSliceTextColor : blueSliceTextColor;

                FortuneWheelSectorUI prefabToUse = sectorData.PrizeType == FortuneWheelPrizeType.Coin
                    ? coinSectorPrefab
                    : lifeSectorPrefab;

                if (prefabToUse == null)
                {
                    prefabToUse = coinSectorPrefab != null ? coinSectorPrefab : lifeSectorPrefab;
                }

                GameObject sectorObj;
                if (prefabToUse != null)
                {
                    sectorObj = Instantiate(prefabToUse.gameObject, container);
                }
                else
                {
                    sectorObj = CreateDefaultSectorGameObject(container, i, sliceTextColor);
                }

                RectTransform rect = sectorObj.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = sectorPosition;
                    // Rotate sector text/icon outward along its radial angle
                    rect.localEulerAngles = new Vector3(0f, 0f, -angleClockwise);
                }

                FortuneWheelSectorUI sectorUI = sectorObj.GetComponent<FortuneWheelSectorUI>();
                if (sectorUI != null)
                {
                    sectorUI.Setup(sectorData, coinSprite, lifeSprite, sliceTextColor);
                    _spawnedSectors.Add(sectorUI);
                }
            }
        }

        private GameObject CreateDefaultSectorGameObject(Transform parent, int index, Color textColor)
        {
            GameObject obj = new GameObject($"Sector_{index}", typeof(RectTransform), typeof(FortuneWheelSectorUI));
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(75, 75);

            // Icon child
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(obj.transform, false);
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchoredPosition = new Vector2(0, 12);
            iconRect.sizeDelta = new Vector2(34, 34);

            // Amount child
            GameObject textObj = new GameObject("Amount", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(obj.transform, false);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchoredPosition = new Vector2(0, -18);
            textRect.sizeDelta = new Vector2(70, 24);

            TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
            if (fontAsset != null)
            {
                tmp.font = fontAsset;
            }
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 17;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = textColor;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.outlineColor = new Color32(10, 25, 55, 240);
            tmp.outlineWidth = 0.22f;

            FortuneWheelSectorUI sectorUI = obj.GetComponent<FortuneWheelSectorUI>();
            var iconField = typeof(FortuneWheelSectorUI).GetField("iconImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (iconField != null) iconField.SetValue(sectorUI, iconObj.GetComponent<Image>());

            var textField = typeof(FortuneWheelSectorUI).GetField("amountText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (textField != null) textField.SetValue(sectorUI, tmp);

            return obj;
        }
        #endregion

        #region Open & Close Panel
        public void OpenPanel()
        {
            EnsureSectorsBuilt();
            UpdateUIState();

            if (dialogContainer != null)
            {
                dialogContainer.DOKill();
                dialogContainer.localScale = Vector3.one * 0.85f;
                dialogContainer.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);
            }

            UISignals.Instance.onOpenPanel?.Invoke(UIPanels.FortuneWheelPanel);
        }

        public void ClosePanel()
        {
            if (_isSpinning)
            {
                return;
            }

            if (_isRewardPending)
            {
                ClaimReward();
            }

            UISignals.Instance.onClosePanel?.Invoke(UIPanels.FortuneWheelPanel);
            UISignals.Instance.onOpenPanel?.Invoke(UIPanels.StartPanel);
        }

        private void OnCloseButtonClicked()
        {
            ClosePanel();
        }
        #endregion

        #region Spin & Reward Logic
        public void OnSpinButtonClicked()
        {
            if (_isSpinning)
            {
                return;
            }

            if (_isRewardPending)
            {
                ClaimReward();
                UpdateUIState();
                return;
            }

            if (!IsAvailable)
            {
                return;
            }

            StartSpin();
        }

        public void StartSpin()
        {
            if (_isSpinning || !IsAvailable)
            {
                return;
            }

            int targetSector = PickWeightedSector();
            _pendingPrizeSectorIndex = targetSector;
            _isSpinning = true;

            if (closeButton != null)
            {
                closeButton.interactable = false;
                closeButton.gameObject.SetActive(false);
            }

            if (spinButton != null)
            {
                spinButton.interactable = false;
            }

            if (spinButtonText != null)
            {
                spinButtonText.text = "SPINNING...";
            }

            if (resultText != null)
            {
                resultText.text = "";
            }

            FortuneWheelSignals.Instance.onWheelSpinStarted?.Invoke();

            if (_spinCoroutine != null)
            {
                StopCoroutine(_spinCoroutine);
            }
            _spinCoroutine = StartCoroutine(SpinRoutine(targetSector));
        }

        private IEnumerator SpinRoutine(int targetSector)
        {
            int sectorCount = _data.Sectors.Count;
            float degreesPerSector = 360f / sectorCount;

            // Angular jitter to land naturally inside the wedge without touching edge lines
            float jitter = UnityEngine.Random.Range(-Mathf.Abs(_data.JitterDegrees), Mathf.Abs(_data.JitterDegrees));
            int fullSpins = UnityEngine.Random.Range(Mathf.Max(3, _data.MinFullSpins), Mathf.Max(_data.MinFullSpins, _data.MaxFullSpins) + 1);

            // Clockwise rotation target matching AdamAsmaca formula
            float baseTargetClockwise = ((360f - (targetSector * degreesPerSector)) % 360f) + jitter;

            // In Unity RectTransform, clockwise rotation decreases Z angle (negative)
            float startAngleZ = wheelTransform != null ? wheelTransform.localEulerAngles.z : 0f;
            float normalizedCurrent = (360f - (startAngleZ % 360f)) % 360f;

            float deltaClockwise = baseTargetClockwise - normalizedCurrent;
            if (deltaClockwise < 0f)
            {
                deltaClockwise += 360f;
            }

            float totalClockwiseRotation = (fullSpins * 360f) + deltaClockwise;
            float duration = Mathf.Max(2f, _data.SpinDuration);

            float elapsed = 0f;
            int lastSectorTick = -1;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Decelerate cubic ease curve (same response as Android DecelerateInterpolator)
                float easeOut = 1f - Mathf.Pow(1f - t, 2.5f);
                float currentClockwise = easeOut * totalClockwiseRotation;

                float currentZ = startAngleZ - currentClockwise;
                if (wheelTransform != null)
                {
                    wheelTransform.localEulerAngles = new Vector3(0f, 0f, currentZ);
                }

                // Sector boundary tick audio and haptics
                int currentSectorUnderPointer = Mathf.FloorToInt(((currentClockwise + (degreesPerSector * 0.5f)) / degreesPerSector)) % sectorCount;
                if (currentSectorUnderPointer != lastSectorTick)
                {
                    lastSectorTick = currentSectorUnderPointer;
                    AudioSignals.Instance.onPlaySound?.Invoke(AudioSoundEnums.Click);
                    PitchSignals.Instance.onPlayPitch?.Invoke(PitchEnums.Click);
                }

                yield return null;
            }

            // Snap accurately to final target angle
            float finalZ = startAngleZ - totalClockwiseRotation;
            if (wheelTransform != null)
            {
                wheelTransform.localEulerAngles = new Vector3(0f, 0f, finalZ);
            }

            OnSpinEnded(targetSector);
        }

        private void OnSpinEnded(int wonSectorIndex)
        {
            _isSpinning = false;
            _isRewardPending = true;

            // Victory SFX & Haptics
            AudioSignals.Instance.onPlaySound?.Invoke(AudioSoundEnums.Win);
            PitchSignals.Instance.onPlayPitch?.Invoke(PitchEnums.Win);

            // Record cooldown UTC timestamp
            RecordDailySpin();

            // Re-enable close button
            if (closeButton != null)
            {
                closeButton.gameObject.SetActive(true);
                closeButton.interactable = true;
            }

            // Display congratulatory prize result
            FortuneWheelSectorData prize = _data.Sectors[wonSectorIndex];
            string rewardMessage = prize.PrizeType == FortuneWheelPrizeType.Coin
                ? $"🎉 You won {prize.Amount} Coins!"
                : $"🎉 You won +{prize.Amount} Life!";

            if (resultText != null)
            {
                resultText.text = rewardMessage;
                resultText.transform.DOKill();
                resultText.transform.localScale = Vector3.one * 0.7f;
                resultText.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
            }

            if (spinButtonText != null)
            {
                spinButtonText.text = "CLAIM";
            }

            if (spinButton != null)
            {
                spinButton.interactable = true;
            }

            FortuneWheelSignals.Instance.onWheelSpinCompleted?.Invoke(wonSectorIndex);
        }

        private void ClaimReward()
        {
            if (!_isRewardPending || _pendingPrizeSectorIndex < 0 || _pendingPrizeSectorIndex >= _data.Sectors.Count)
            {
                return;
            }

            FortuneWheelSectorData prize = _data.Sectors[_pendingPrizeSectorIndex];
            _isRewardPending = false;
            _pendingPrizeSectorIndex = -1;

            if (prize.PrizeType == FortuneWheelPrizeType.Coin)
            {
                ScoreSignals.Instance.onScoreIncrease?.Invoke(ScoreTypeEnums.Money, prize.Amount);
            }
            else if (prize.PrizeType == FortuneWheelPrizeType.Life)
            {
                EnergySignals.Instance.onAddEnergy?.Invoke(prize.Amount);
            }

            AudioSignals.Instance.onPlaySound?.Invoke(AudioSoundEnums.Click);
        }

        public int PickWeightedSector()
        {
            if (_data == null || _data.Sectors == null || _data.Sectors.Count == 0)
            {
                return 0;
            }

            int totalWeight = 0;
            for (int i = 0; i < _data.Sectors.Count; i++)
            {
                totalWeight += Mathf.Max(1, _data.Sectors[i].Weight);
            }

            int roll = UnityEngine.Random.Range(0, totalWeight);
            int cumulative = 0;

            for (int i = 0; i < _data.Sectors.Count; i++)
            {
                cumulative += Mathf.Max(1, _data.Sectors[i].Weight);
                if (roll < cumulative)
                {
                    return i;
                }
            }

            return 0;
        }
        #endregion

        #region Daily Cooldown & Persistence
        private bool OnGetIsDailyWheelAvailable()
        {
            return IsAvailable;
        }

        public long GetRemainingCooldownSeconds()
        {
            if (_lastSpinUtcTicks <= 0L)
            {
                return 0L;
            }

            long nowTicks = DateTime.UtcNow.Ticks;
            long elapsedTicks = nowTicks - _lastSpinUtcTicks;

            // Clock tamper protection (moved backward)
            if (elapsedTicks < 0)
            {
                _lastSpinUtcTicks = nowTicks;
                SaveSignals.Instance.onSaveLong?.Invoke(_lastSpinUtcTicks, SaveLoadStates.DailyWheelLastSpinTicks, SaveFiles.SaveFile);
                return (long)TimeSpan.FromHours(_data.CooldownHours).TotalSeconds;
            }

            long cooldownTicks = TimeSpan.FromHours(_data.CooldownHours).Ticks;
            if (elapsedTicks >= cooldownTicks)
            {
                return 0L;
            }

            return (cooldownTicks - elapsedTicks) / TimeSpan.TicksPerSecond;
        }

        private void RecordDailySpin()
        {
            _lastSpinUtcTicks = DateTime.UtcNow.Ticks;
            SaveSignals.Instance.onSaveLong?.Invoke(_lastSpinUtcTicks, SaveLoadStates.DailyWheelLastSpinTicks, SaveFiles.SaveFile);
            FortuneWheelSignals.Instance.onDailyWheelCooldownChanged?.Invoke();
        }

        private void LoadLastSpinTime()
        {
            if (SaveSignals.Instance != null)
            {
                _lastSpinUtcTicks = SaveSignals.Instance.onGetLong(SaveLoadStates.DailyWheelLastSpinTicks, SaveFiles.SaveFile);
            }
        }

        private void UpdateUIState()
        {
            if (_isSpinning)
            {
                return;
            }

            if (_isRewardPending)
            {
                if (spinButtonText != null) spinButtonText.text = "CLAIM";
                if (spinButton != null) spinButton.interactable = true;
                return;
            }

            long remainingSec = GetRemainingCooldownSeconds();
            if (remainingSec <= 0L)
            {
                if (spinButtonText != null) spinButtonText.text = "FREE SPIN";
                if (spinButton != null) spinButton.interactable = true;
                if (resultText != null && string.IsNullOrEmpty(resultText.text))
                {
                    resultText.text = "Spin to win Coins & Lives!";
                }
            }
            else
            {
                TimeSpan span = TimeSpan.FromSeconds(remainingSec);
                string formatted = string.Format("{0:D2}:{1:D2}:{2:D2}", span.Hours, span.Minutes, span.Seconds);
                if (spinButtonText != null) spinButtonText.text = $"NEXT IN {formatted}";
                if (spinButton != null) spinButton.interactable = false;
            }
        }
        #endregion
    }
}
