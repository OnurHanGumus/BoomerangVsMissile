using System;
using System.Collections.Generic;
using Data.UnityObject;
using Data.ValueObject;
using DG.Tweening;
using Enums;
using Managers;
using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers.UI
{
    public class BoomerangStorePanelController : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables
        [Header("Top Inspector - Visuals")]
        [SerializeField] private Image inspectorIcon;
        [SerializeField] private TextMeshProUGUI inspectorTitleText;
        [SerializeField] private TextMeshProUGUI inspectorDescText;
        [SerializeField] private TextMeshProUGUI inspectorTierText;

        [Header("Top Inspector - Stat Sliders")]
        [SerializeField] private Slider speedSlider;
        [SerializeField] private TextMeshProUGUI speedValueText;

        [SerializeField] private Slider returnSpeedSlider;
        [SerializeField] private TextMeshProUGUI returnSpeedValueText;

        [SerializeField] private Slider damageSlider;
        [SerializeField] private TextMeshProUGUI damageValueText;

        [SerializeField] private Slider sizeSlider;
        [SerializeField] private TextMeshProUGUI sizeValueText;

        [Header("Top Inspector - Action Button")]
        [SerializeField] private Button actionButton;
        [SerializeField] private TextMeshProUGUI actionButtonText;
        [SerializeField] private Image actionButtonBackground;

        [Header("Button Colors")]
        [SerializeField] private Color buyColor = new Color(0.18f, 0.72f, 0.35f, 1f);
        [SerializeField] private Color equipColor = new Color(0.15f, 0.65f, 0.95f, 1f);
        [SerializeField] private Color equippedColor = new Color(0.45f, 0.42f, 0.30f, 0.6f);
        [SerializeField] private Color disabledColor = new Color(0.35f, 0.35f, 0.4f, 0.5f);

        [Header("Bottom Grid Section")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private Transform tilesContainer;

        [Header("Global Header Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI moneyText;
        [SerializeField] private Button closeButton;
        #endregion

        #region Private Variables
        private readonly List<BoomerangGridTileUI> _spawnedTiles = new List<BoomerangGridTileUI>();
        private int _inspectedId = 0;
        private Sequence _inspectorTweenSequence;
        private Sequence _actionPunchSequence;

        // Stat normalization limits
        private const float MinSpeed = 3f;
        private const float MaxSpeed = 15f;
        private const float MinReturnSpeed = 0f;
        private const float MaxReturnSpeed = 3.5f;
        private const float MinDamage = 0f;
        private const float MaxDamage = 5f;
        private const float MinSize = 0f;
        private const float MaxSize = 2f;
        #endregion

        #endregion

        private void Awake()
        {
            SubscribeEvents();
        }

        private void Start()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(OnCloseClicked);
            }

            if (actionButton != null)
            {
                actionButton.onClick.RemoveAllListeners();
                actionButton.onClick.AddListener(OnActionClicked);
            }

            InitPanel();
        }

        #region Event Subscription
        private void SubscribeEvents()
        {
            BoomerangSignals.Instance.onSelectBoomerang += OnBoomerangSelected;
            ScoreSignals.Instance.onScoreDecrease += OnMoneyChanged;
            ScoreSignals.Instance.onScoreIncrease += OnMoneyIncreased;
            UISignals.Instance.onOpenPanel += OnOpenPanel;
        }

        private void UnsubscribeEvents()
        {
            if (BoomerangSignals.Instance != null)
            {
                BoomerangSignals.Instance.onSelectBoomerang -= OnBoomerangSelected;
            }

            if (ScoreSignals.Instance != null)
            {
                ScoreSignals.Instance.onScoreDecrease -= OnMoneyChanged;
                ScoreSignals.Instance.onScoreIncrease -= OnMoneyIncreased;
            }

            if (UISignals.Instance != null)
            {
                UISignals.Instance.onOpenPanel -= OnOpenPanel;
            }
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
            _inspectorTweenSequence?.Kill();
            _actionPunchSequence?.Kill();

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
            }

            if (actionButton != null)
            {
                actionButton.onClick.RemoveAllListeners();
            }
        }
        #endregion

        private void OnOpenPanel(UIPanels panel)
        {
            if (panel == UIPanels.StorePanel)
            {
                int currentEquipped = BoomerangStoreManager.Instance != null
                    ? BoomerangStoreManager.Instance.SelectedItemId
                    : 0;

                InspectBoomerang(currentEquipped, false);
                RefreshAllTiles();
            }
        }

        private void OnBoomerangSelected(int id)
        {
            RefreshAllTiles();
            if (_inspectedId == id)
            {
                InspectBoomerang(id, false);
            }
        }

        private void OnMoneyChanged(ScoreTypeEnums type, int amount)
        {
            if (type == ScoreTypeEnums.Money)
            {
                RefreshInspectorActionButton();
            }
        }

        private void OnMoneyIncreased(ScoreTypeEnums type, int amount)
        {
            if (type == ScoreTypeEnums.Money)
            {
                RefreshInspectorActionButton();
            }
        }

        public void InitPanel()
        {
            PopulateGrid();

            int initialId = BoomerangStoreManager.Instance != null
                ? BoomerangStoreManager.Instance.SelectedItemId
                : 0;

            InspectBoomerang(initialId, false);
        }

        public void PopulateGrid()
        {
            if (tilesContainer == null)
            {
                return;
            }

            // Clear previous tiles
            foreach (Transform child in tilesContainer)
            {
                Destroy(child.gameObject);
            }
            _spawnedTiles.Clear();

            CD_Boomerangs config = BoomerangStoreManager.Instance != null
                ? BoomerangStoreManager.Instance.Config
                : Resources.Load<CD_Boomerangs>("Data/CD_Boomerangs");

            if (config == null || config.Boomerangs == null || tilePrefab == null)
            {
                return;
            }

            foreach (BoomerangItemData data in config.Boomerangs)
            {
                GameObject tileObj = Instantiate(tilePrefab, tilesContainer);
                var tileUI = tileObj.GetComponent<BoomerangGridTileUI>();
                if (tileUI != null)
                {
                    tileUI.Setup(data, OnTileClicked);
                    _spawnedTiles.Add(tileUI);
                }
            }

            RefreshAllTiles();
        }

        private void OnTileClicked(int id)
        {
            AudioSignals.Instance.onPlaySound?.Invoke(AudioSoundEnums.Click);
            PitchSignals.Instance.onPlayPitch?.Invoke(PitchEnums.Click);

            InspectBoomerang(id, true);
        }

        public void InspectBoomerang(int id, bool animate = true)
        {
            _inspectedId = id;

            CD_Boomerangs config = BoomerangStoreManager.Instance != null
                ? BoomerangStoreManager.Instance.Config
                : Resources.Load<CD_Boomerangs>("Data/CD_Boomerangs");

            BoomerangItemData data = config != null ? config.GetBoomerang(id) : null;
            if (data == null)
            {
                return;
            }

            // 1. Update Weapon Visuals
            if (inspectorIcon != null && data.Icon != null)
            {
                inspectorIcon.sprite = data.Icon;
                inspectorIcon.preserveAspect = true;

                if (animate)
                {
                    inspectorIcon.transform.DOKill();
                    inspectorIcon.transform.localScale = Vector3.one * 0.85f;
                    inspectorIcon.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);
                }
            }

            if (inspectorTitleText != null)
            {
                inspectorTitleText.text = data.Title;
            }

            if (inspectorDescText != null)
            {
                inspectorDescText.text = data.Description;
            }

            if (inspectorTierText != null)
            {
                inspectorTierText.text = $"TIER {data.Id + 1}";
            }

            // 2. Animate Stat Sliders
            float speedNormalized = Mathf.Clamp01((data.Speed - MinSpeed) / (MaxSpeed - MinSpeed));
            float returnNormalized = Mathf.Clamp01((data.ReturnSpeedMultiplier - MinReturnSpeed) / (MaxReturnSpeed - MinReturnSpeed));
            float damageNormalized = Mathf.Clamp01((data.Damage - MinDamage) / (MaxDamage - MinDamage));
            float sizeNormalized = Mathf.Clamp01((data.SizeMultiplier - MinSize) / (MaxSize - MinSize));

            float tweenDuration = animate ? 0.3f : 0f;

            if (speedSlider != null)
            {
                speedSlider.DOKill();
                if (animate) speedSlider.DOValue(speedNormalized, tweenDuration).SetEase(Ease.OutQuad).SetUpdate(true);
                else speedSlider.value = speedNormalized;
            }
            if (speedValueText != null) speedValueText.text = $"{data.Speed:0.0}";

            if (returnSpeedSlider != null)
            {
                returnSpeedSlider.DOKill();
                if (animate) returnSpeedSlider.DOValue(returnNormalized, tweenDuration).SetEase(Ease.OutQuad).SetUpdate(true);
                else returnSpeedSlider.value = returnNormalized;
            }
            if (returnSpeedValueText != null) returnSpeedValueText.text = $"{data.ReturnSpeedMultiplier:0.0}x";

            if (damageSlider != null)
            {
                damageSlider.DOKill();
                if (animate) damageSlider.DOValue(damageNormalized, tweenDuration).SetEase(Ease.OutQuad).SetUpdate(true);
                else damageSlider.value = damageNormalized;
            }
            if (damageValueText != null) damageValueText.text = $"{data.Damage} DMG";

            if (sizeSlider != null)
            {
                sizeSlider.DOKill();
                if (animate) sizeSlider.DOValue(sizeNormalized, tweenDuration).SetEase(Ease.OutQuad).SetUpdate(true);
                else sizeSlider.value = sizeNormalized;
            }
            if (sizeValueText != null) sizeValueText.text = $"{data.SizeMultiplier:0.0}x";

            // 3. Update Action Button
            RefreshInspectorActionButton();

            // 4. Update Grid Tiles State
            RefreshAllTiles();
        }

        private void RefreshInspectorActionButton()
        {
            if (actionButton == null || BoomerangStoreManager.Instance == null)
            {
                return;
            }

            bool isOwned = BoomerangStoreManager.Instance.IsOwned(_inspectedId);
            bool isEquipped = BoomerangStoreManager.Instance.IsEquipped(_inspectedId);
            bool canAfford = BoomerangStoreManager.Instance.CanAfford(_inspectedId);

            CD_Boomerangs config = BoomerangStoreManager.Instance.Config;
            BoomerangItemData data = config != null ? config.GetBoomerang(_inspectedId) : null;
            int price = data != null ? data.Price : 0;

            if (!isOwned)
            {
                if (actionButtonText != null)
                {
                    actionButtonText.text = price > 0 ? $"BUY  {price}" : "CLAIM FREE";
                }

                if (actionButtonBackground != null)
                {
                    actionButtonBackground.color = canAfford ? buyColor : disabledColor;
                }

                actionButton.interactable = canAfford;
            }
            else if (isEquipped)
            {
                if (actionButtonText != null)
                {
                    actionButtonText.text = "EQUIPPED  ✓";
                }

                if (actionButtonBackground != null)
                {
                    actionButtonBackground.color = equippedColor;
                }

                actionButton.interactable = false;
            }
            else
            {
                if (actionButtonText != null)
                {
                    actionButtonText.text = "EQUIP";
                }

                if (actionButtonBackground != null)
                {
                    actionButtonBackground.color = equipColor;
                }

                actionButton.interactable = true;
            }
        }

        private void OnActionClicked()
        {
            if (BoomerangStoreManager.Instance == null)
            {
                return;
            }

            // Micro-punch animation
            if (actionButton != null)
            {
                _actionPunchSequence?.Kill();
                _actionPunchSequence = DOTween.Sequence();
                _actionPunchSequence.Append(actionButton.transform.DOPunchScale(Vector3.one * 0.12f, 0.18f, 7, 1f));
                _actionPunchSequence.SetUpdate(true);
            }

            bool isOwned = BoomerangStoreManager.Instance.IsOwned(_inspectedId);

            if (!isOwned)
            {
                bool bought = BoomerangStoreManager.Instance.TryBuyBoomerang(_inspectedId);
                if (bought)
                {
                    AudioSignals.Instance.onPlaySound?.Invoke(AudioSoundEnums.Click);
                    PitchSignals.Instance.onPlayPitch?.Invoke(PitchEnums.Click);
                    InspectBoomerang(_inspectedId, false);
                }
            }
            else
            {
                BoomerangStoreManager.Instance.SelectBoomerang(_inspectedId);
                AudioSignals.Instance.onPlaySound?.Invoke(AudioSoundEnums.Click);
                PitchSignals.Instance.onPlayPitch?.Invoke(PitchEnums.Click);
                InspectBoomerang(_inspectedId, false);
            }
        }

        public void RefreshAllTiles()
        {
            if (BoomerangStoreManager.Instance == null)
            {
                return;
            }

            foreach (var tile in _spawnedTiles)
            {
                if (tile != null)
                {
                    bool isSelected = tile.BoomerangId == _inspectedId;
                    bool isEquipped = BoomerangStoreManager.Instance.IsEquipped(tile.BoomerangId);
                    bool isOwned = BoomerangStoreManager.Instance.IsOwned(tile.BoomerangId);

                    tile.UpdateState(isSelected, isEquipped, isOwned);
                }
            }
        }

        private void OnCloseClicked()
        {
            AudioSignals.Instance.onPlaySound?.Invoke(AudioSoundEnums.Click);
            PitchSignals.Instance.onPlayPitch?.Invoke(PitchEnums.Click);

            UISignals.Instance.onClosePanel?.Invoke(UIPanels.StorePanel);
            UISignals.Instance.onOpenPanel?.Invoke(UIPanels.StartPanel);
        }
    }
}
