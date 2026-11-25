using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers.UI
{
    public class LevelSelectionTileUI : MonoBehaviour
    {
        #region Self Variables

        #region Public Variables
        public RectTransform RectTransform => rectTransform;
        public int LevelIndex => _levelIndex;
        #endregion

        #region Serialized Variables
        [Header("UI References")]
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Button button;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image selectionBorderImage;
        [SerializeField] private TextMeshProUGUI levelNumberText;

        [Header("State Sprites")]
        [SerializeField] private Sprite unlockedSprite;
        [SerializeField] private Sprite selectedSprite;
        [SerializeField] private Sprite maxReachedSprite;
        [SerializeField] private Sprite lockedSprite;

        [Header("State Colors")]
        [SerializeField] private Color unlockedBgColor = Color.white;
        [SerializeField] private Color selectedBgColor = Color.white;
        [SerializeField] private Color maxReachedBgColor = Color.white;
        [SerializeField] private Color lockedBgColor = new Color(0.75f, 0.80f, 0.86f, 0.72f);

        [SerializeField] private Color unlockedTextColor = Color.white;
        [SerializeField] private Color selectedTextColor = Color.white;
        [SerializeField] private Color maxReachedTextColor = Color.white;
        [SerializeField] private Color lockedTextColor = new Color(0.68f, 0.74f, 0.82f, 0.65f);

        [SerializeField] private Color selectedBorderColor = new Color(0.45f, 1.0f, 0.65f, 1f);
        [SerializeField] private Color maxReachedBorderColor = new Color(1.0f, 0.82f, 0.25f, 1f);
        #endregion

        #region Private Variables
        private int _levelIndex;
        private Action<int> _onClicked = delegate { };
        #endregion

        #endregion

        public void Initialize(Action<int> onClicked)
        {
            _onClicked = onClicked;
            button.onClick.RemoveListener(HandleClick);
            button.onClick.AddListener(HandleClick);
        }

        public void Bind(int levelIndex, bool isUnlocked, bool isSelected, bool isMaxReached)
        {
            _levelIndex = levelIndex;
            levelNumberText.text = (levelIndex + 1).ToString();
            button.interactable = isUnlocked;

            if (isSelected)
            {
                backgroundImage.sprite = selectedSprite;
                backgroundImage.color = selectedBgColor;
                levelNumberText.color = selectedTextColor;
                selectionBorderImage.enabled = true;
                selectionBorderImage.color = selectedBorderColor;
            }
            else if (isMaxReached)
            {
                backgroundImage.sprite = maxReachedSprite;
                backgroundImage.color = maxReachedBgColor;
                levelNumberText.color = maxReachedTextColor;
                selectionBorderImage.enabled = true;
                selectionBorderImage.color = maxReachedBorderColor;
            }
            else if (isUnlocked)
            {
                backgroundImage.sprite = unlockedSprite;
                backgroundImage.color = unlockedBgColor;
                levelNumberText.color = unlockedTextColor;
                selectionBorderImage.enabled = false;
            }
            else
            {
                backgroundImage.sprite = lockedSprite;
                backgroundImage.color = lockedBgColor;
                levelNumberText.color = lockedTextColor;
                selectionBorderImage.enabled = false;
            }
        }

        private void HandleClick()
        {
            _onClicked(_levelIndex);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(HandleClick);
        }
    }
}
