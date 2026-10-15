using System;
using Data.ValueObject;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers.UI
{
    public class BoomerangGridTileUI : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables
        [Header("Weapon Identity")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Button tileButton;

        [Header("State Indicators")]
        [SerializeField] private GameObject selectedBorder;
        [SerializeField] private GameObject equippedBadge;
        [SerializeField] private GameObject lockOverlay;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private Image tileBackground;

        [Header("Visual Theme")]
        [SerializeField] private Color normalBorderColor = new Color(0.15f, 0.22f, 0.32f, 1f);
        [SerializeField] private Color selectedBorderColor = new Color(0.2f, 0.85f, 1f, 1f);
        [SerializeField] private Color unownedTileColor = new Color(0.08f, 0.1f, 0.15f, 0.85f);
        [SerializeField] private Color ownedTileColor = new Color(0.12f, 0.16f, 0.24f, 0.95f);
        #endregion

        #region Private Variables
        private BoomerangItemData _data;
        private Action<int> _onSelectedCallback;
        private Sequence _punchSequence;
        #endregion

        #endregion

        public int BoomerangId => _data != null ? _data.Id : -1;

        public void Setup(BoomerangItemData data, Action<int> onSelectedCallback)
        {
            _data = data;
            _onSelectedCallback = onSelectedCallback;

            if (iconImage != null && _data.Icon != null)
            {
                iconImage.sprite = _data.Icon;
                iconImage.preserveAspect = true;
            }

            if (nameText != null)
            {
                nameText.text = _data.Title;
            }

            if (tileButton != null)
            {
                tileButton.onClick.RemoveAllListeners();
                tileButton.onClick.AddListener(OnTileClicked);
            }
        }

        public void UpdateState(bool isSelected, bool isEquipped, bool isOwned)
        {
            if (selectedBorder != null)
            {
                selectedBorder.SetActive(isSelected);
            }

            if (equippedBadge != null)
            {
                equippedBadge.SetActive(isEquipped);
            }

            if (lockOverlay != null)
            {
                lockOverlay.SetActive(!isOwned);
            }

            if (priceText != null && _data != null)
            {
                priceText.text = _data.Price > 0 ? _data.Price.ToString() : "FREE";
            }

            if (tileBackground != null)
            {
                tileBackground.color = isOwned ? ownedTileColor : unownedTileColor;
            }
        }

        private void OnTileClicked()
        {
            if (_data == null)
            {
                return;
            }

            // Punch scale feedback
            _punchSequence?.Kill();
            _punchSequence = DOTween.Sequence();
            _punchSequence.Append(transform.DOPunchScale(Vector3.one * 0.08f, 0.15f, 6, 1f));
            _punchSequence.SetUpdate(true);

            _onSelectedCallback?.Invoke(_data.Id);
        }

        private void OnDestroy()
        {
            _punchSequence?.Kill();
            if (tileButton != null)
            {
                tileButton.onClick.RemoveAllListeners();
            }
        }
    }
}
