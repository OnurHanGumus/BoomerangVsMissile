using Data.ValueObject;
using Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers.UI
{
    public class FortuneWheelSectorUI : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI amountText;
        #endregion

        #region Private Variables
        private FortuneWheelSectorData _data;
        #endregion

        #endregion

        public void Setup(FortuneWheelSectorData data, Sprite defaultCoinSprite = null, Sprite defaultLifeSprite = null, Color? textColor = null)
        {
            _data = data;

            if (iconImage != null)
            {
                if (data.CustomIcon != null)
                {
                    iconImage.sprite = data.CustomIcon;
                }
                else if (iconImage.sprite == null)
                {
                    if (data.PrizeType == FortuneWheelPrizeType.Coin && defaultCoinSprite != null)
                    {
                        iconImage.sprite = defaultCoinSprite;
                    }
                    else if (data.PrizeType == FortuneWheelPrizeType.Life && defaultLifeSprite != null)
                    {
                        iconImage.sprite = defaultLifeSprite;
                    }
                }

                iconImage.enabled = iconImage.sprite != null;
            }

            if (amountText != null)
            {
                if (textColor.HasValue)
                {
                    amountText.color = textColor.Value;
                }

                if (!string.IsNullOrEmpty(data.CustomLabel))
                {
                    amountText.text = data.CustomLabel;
                }
                else
                {
                    amountText.text = data.PrizeType == FortuneWheelPrizeType.Life
                        ? $"+{data.Amount}"
                        : $"{data.Amount}";
                }
            }
        }

        public void SetTextColor(Color color)
        {
            if (amountText != null)
            {
                amountText.color = color;
            }
        }
    }
}
