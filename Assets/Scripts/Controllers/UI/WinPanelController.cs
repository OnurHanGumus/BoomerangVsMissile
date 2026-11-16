using DG.Tweening;
using Enums;
using Signals;
using TMPro;
using UnityEngine;

namespace Controllers.UI
{
    public class WinPanelController : MonoBehaviour
    {
        #region Self Variables

        #region Public Variables
        public TextMeshProUGUI RewardText => rewardText;
        #endregion

        #region Serialized Variables
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private string rewardFormat = "+{0}";
        #endregion

        #region Private Variables
        private int _lastRewardAmount;
        #endregion

        #endregion

        private void Awake()
        {
            Init();
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void Init()
        {
        }

        #region Event Subscription
        private void SubscribeEvents()
        {
            ScoreSignals.Instance.onLevelRewardGranted += OnLevelRewardGranted;
            UISignals.Instance.onOpenPanel += OnOpenPanel;
        }

        private void UnsubscribeEvents()
        {
            ScoreSignals.Instance.onLevelRewardGranted -= OnLevelRewardGranted;
            UISignals.Instance.onOpenPanel -= OnOpenPanel;
        }

        #endregion

        private void OnLevelRewardGranted(int rewardAmount)
        {
            _lastRewardAmount = rewardAmount;
            UpdateRewardDisplay(_lastRewardAmount);
        }

        private void OnOpenPanel(UIPanels panel)
        {
            if (panel == UIPanels.WinPanel)
            {
                _lastRewardAmount = ScoreSignals.Instance.onGetLastLevelReward();
                UpdateRewardDisplay(_lastRewardAmount);
            }
        }

        public void UpdateRewardDisplay(int rewardAmount)
        {
            rewardText.text = string.Format(rewardFormat, rewardAmount);
            rewardText.transform.DOKill();
            rewardText.transform.localScale = Vector3.one;
            rewardText.transform.DOPunchScale(Vector3.one * 0.2f, 0.35f, 6, 0.5f).SetUpdate(true);
        }
    }
}
