using Data.ValueObject;
using DG.Tweening;
using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers.UI
{
    public class WeeklyTaskCardUI : MonoBehaviour
    {
        #region Self Variables

        #region Public Variables
        public int SlotIndex => _slotIndex;
        #endregion

        #region Serialized Variables
        [SerializeField] private Image cardBackground;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TextMeshProUGUI progressCountText;
        [SerializeField] private TextMeshProUGUI rewardAmountText;
        [SerializeField] private Button claimButton;
        [SerializeField] private Image claimButtonBackground;
        [SerializeField] private TextMeshProUGUI claimButtonText;

        [Header("Semantic Ribbon Sprites")]
        [SerializeField] private Sprite defaultCardSprite;
        [SerializeField] private Sprite claimedCardSprite;
        [SerializeField] private Sprite claimableButtonSprite;
        [SerializeField] private Sprite slateButtonSprite;
        #endregion

        #region Private Variables
        private int _slotIndex = -1;
        private Tween _progressTween;
        private bool _hasProgressTween;
        private static readonly Color DefaultCardColor = new Color(1f, 1f, 1f, 1f);
        private static readonly Color ClaimableCardColor = new Color(1f, 1f, 1f, 1f);
        private static readonly Color ClaimedCardColor = new Color(0.86f, 0.89f, 0.94f, 0.82f);

        private static readonly Color LockedButtonColor = new Color(0.84f, 0.88f, 0.92f, 0.90f);
        private static readonly Color ClaimableButtonColor = new Color(1f, 1f, 1f, 1f);
        private static readonly Color ClaimedButtonColor = new Color(0.78f, 0.82f, 0.86f, 0.80f);
        #endregion

        #endregion

        private void Awake()
        {
            BindButton();
        }

        private void BindButton()
        {
            if (claimButton != null)
            {
                claimButton.onClick.RemoveListener(OnClaimClicked);
                claimButton.onClick.AddListener(OnClaimClicked);
            }
        }

        public void ConfigureReferences(
            Image bg,
            TextMeshProUGUI descTxt,
            Slider slider,
            TextMeshProUGUI countTxt,
            TextMeshProUGUI rewardTxt,
            Button btn,
            Image btnBg,
            TextMeshProUGUI btnTxt)
        {
            cardBackground = bg;
            descriptionText = descTxt;
            progressSlider = slider;
            progressCountText = countTxt;
            rewardAmountText = rewardTxt;
            claimButton = btn;
            claimButtonBackground = btnBg;
            claimButtonText = btnTxt;
            BindButton();
        }

        public void BindData(int slotIndex, WeeklyTaskProgressData task, string description)
        {
            _slotIndex = slotIndex;
            if (task == null)
            {
                return;
            }

            if (descriptionText != null)
            {
                descriptionText.text = description;
            }

            if (progressSlider != null)
            {
                if (_hasProgressTween)
                {
                    _progressTween.Kill();
                    _hasProgressTween = false;
                }

                _progressTween = progressSlider
                    .DOValue(task.NormalizedProgress, 0.25f)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true);
                _hasProgressTween = true;
            }

            if (progressCountText != null)
            {
                progressCountText.text = $"{task.Progress}/{task.Target}";
            }

            if (rewardAmountText != null)
            {
                rewardAmountText.text = $"+{task.Reward}";
            }

            if (task.IsClaimed)
            {
                if (cardBackground != null)
                {
                    cardBackground.sprite = claimedCardSprite;
                    cardBackground.color = ClaimedCardColor;
                }
                if (claimButton != null) claimButton.interactable = false;
                if (claimButtonBackground != null)
                {
                    claimButtonBackground.sprite = slateButtonSprite;
                    claimButtonBackground.color = ClaimedButtonColor;
                }
                if (claimButtonText != null) claimButtonText.text = "TAKEN";
            }
            else if (task.IsClaimable)
            {
                if (cardBackground != null)
                {
                    cardBackground.sprite = defaultCardSprite;
                    cardBackground.color = ClaimableCardColor;
                }
                if (claimButton != null) claimButton.interactable = true;
                if (claimButtonBackground != null)
                {
                    claimButtonBackground.sprite = claimableButtonSprite;
                    claimButtonBackground.color = ClaimableButtonColor;
                }
                if (claimButtonText != null) claimButtonText.text = "CLAIM";
            }
            else
            {
                if (cardBackground != null)
                {
                    cardBackground.sprite = defaultCardSprite;
                    cardBackground.color = DefaultCardColor;
                }
                if (claimButton != null) claimButton.interactable = false;
                if (claimButtonBackground != null)
                {
                    claimButtonBackground.sprite = slateButtonSprite;
                    claimButtonBackground.color = LockedButtonColor;
                }
                if (claimButtonText != null) claimButtonText.text = "CLAIM";
            }
        }

        private void OnClaimClicked()
        {
            if (_slotIndex < 0)
            {
                return;
            }

            transform.DOKill();
            transform.localScale = Vector3.one;
            transform.DOPunchScale(Vector3.one * 0.04f, 0.2f, 6, 0.5f).SetUpdate(true);

            WeeklyTaskSignals.Instance.onClaimTask?.Invoke(_slotIndex);
        }

        private void OnDestroy()
        {
            if (_hasProgressTween)
            {
                _progressTween.Kill();
                _hasProgressTween = false;
            }
            transform.DOKill();
        }
    }
}
