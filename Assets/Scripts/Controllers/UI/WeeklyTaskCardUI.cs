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
        #endregion

        #region Private Variables
        private int _slotIndex = -1;
        private static readonly Color DefaultCardColor = new Color(0.11f, 0.14f, 0.22f, 0.96f);
        private static readonly Color ClaimableCardColor = new Color(0.14f, 0.25f, 0.22f, 0.98f);
        private static readonly Color ClaimedCardColor = new Color(0.08f, 0.10f, 0.15f, 0.72f);

        private static readonly Color LockedButtonColor = new Color(0.24f, 0.28f, 0.36f, 1f);
        private static readonly Color ClaimableButtonColor = new Color(0.16f, 0.78f, 0.38f, 1f);
        private static readonly Color ClaimedButtonColor = new Color(0.18f, 0.22f, 0.28f, 0.85f);
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
                progressSlider.value = task.NormalizedProgress;
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
                if (cardBackground != null) cardBackground.color = ClaimedCardColor;
                if (claimButton != null) claimButton.interactable = false;
                if (claimButtonBackground != null) claimButtonBackground.color = ClaimedButtonColor;
                if (claimButtonText != null) claimButtonText.text = "CLAIMED";
            }
            else if (task.IsClaimable)
            {
                if (cardBackground != null) cardBackground.color = ClaimableCardColor;
                if (claimButton != null) claimButton.interactable = true;
                if (claimButtonBackground != null) claimButtonBackground.color = ClaimableButtonColor;
                if (claimButtonText != null) claimButtonText.text = "CLAIM";
            }
            else
            {
                if (cardBackground != null) cardBackground.color = DefaultCardColor;
                if (claimButton != null) claimButton.interactable = false;
                if (claimButtonBackground != null) claimButtonBackground.color = LockedButtonColor;
                if (claimButtonText != null) claimButtonText.text = "IN PROGRESS";
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
            transform.DOKill();
        }
    }
}
