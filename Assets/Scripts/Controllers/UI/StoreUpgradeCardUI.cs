using Data.ValueObject;
using Enums;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers.UI
{
    public class StoreUpgradeCardUI : MonoBehaviour
    {
        #region Serialized Variables
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI bonusText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Button buyButton;
        [SerializeField] private Image buyButtonBackground;
        [SerializeField] private Slider levelProgressBar;

        [Header("Button Visuals")]
        [SerializeField] private Color normalColor = new Color(0.2f, 0.75f, 0.2f, 1f);
        [SerializeField] private Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        [SerializeField] private Color maxLevelColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);
        #endregion

        #region Private Variables
        private UpgradeDefinition _definition;
        #endregion

        public UpgradeDefinition Definition => _definition;

        public void Setup(UpgradeDefinition definition)
        {
            _definition = definition;
            if (buyButton != null)
            {
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(OnBuyClicked);
            }
            UpdateUI();
        }

        public void UpdateUI()
        {
            if (_definition == null)
            {
                return;
            }

            int currentLevel = BoomerangUpgradeManager.Instance != null
                ? BoomerangUpgradeManager.Instance.GetLevel(_definition.Type)
                : 0;
            int maxLevel = _definition.MaxLevel;
            bool isMax = currentLevel >= maxLevel;
            int cost = _definition.CalculateCost(currentLevel);
            bool canAfford = BoomerangUpgradeManager.Instance != null && BoomerangUpgradeManager.Instance.CanAfford(_definition.Type);

            // Title
            if (titleText != null)
            {
                titleText.text = _definition.Title;
            }

            // Level text
            if (levelText != null)
            {
                levelText.text = isMax ? "MAX LEVEL" : $"Lv. {currentLevel} / {maxLevel}";
            }

            // Bonus text
            if (bonusText != null)
            {
                string bonusStr = _definition.FormatBonus(currentLevel);
                if (isMax)
                {
                    bonusText.text = $"{bonusStr} (MAX)";
                }
                else
                {
                    bonusText.text = $"{bonusStr} (+{_definition.BonusPerLevel:0.###}/lv)";
                }
            }

            // Icon
            if (iconImage != null)
            {
                if (_definition.Icon != null)
                {
                    iconImage.sprite = _definition.Icon;
                    iconImage.gameObject.SetActive(true);
                }
                else
                {
                    iconImage.gameObject.SetActive(false);
                }
            }

            // Progress bar
            if (levelProgressBar != null)
            {
                levelProgressBar.minValue = 0;
                levelProgressBar.maxValue = maxLevel;
                levelProgressBar.value = currentLevel;
            }

            // Price / Button State
            if (priceText != null)
            {
                priceText.text = isMax ? "MAX" : $"{cost} 💎";
            }

            if (buyButton != null)
            {
                buyButton.interactable = !isMax && canAfford;
            }

            if (buyButtonBackground != null)
            {
                if (isMax)
                {
                    buyButtonBackground.color = maxLevelColor;
                }
                else if (canAfford)
                {
                    buyButtonBackground.color = normalColor;
                }
                else
                {
                    buyButtonBackground.color = disabledColor;
                }
            }
        }

        private void OnBuyClicked()
        {
            if (_definition == null || BoomerangUpgradeManager.Instance == null)
            {
                return;
            }

            bool success = BoomerangUpgradeManager.Instance.TryPurchaseUpgrade(_definition.Type);
            if (success)
            {
                UpdateUI();
            }
        }
    }
}
