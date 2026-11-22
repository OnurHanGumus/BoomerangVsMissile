using Data.UnityObject;
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
        private float _rollDuration;
        private Vector3 _initialRewardScale;
        private Tween _rollTween;
        private Tween _punchTween;
        private bool _hasRollTween;
        private bool _hasPunchTween;
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
            KillTweens();
            UnsubscribeEvents();
        }

        private void Init()
        {
            _initialRewardScale = rewardText.rectTransform.localScale;
            _rollDuration = Resources.Load<CD_UI>("Data/CD_UI").Data.CoinRollDuration;
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
            KillTweens();

            int displayedValue = 0;
            rewardText.text = string.Format(rewardFormat, 0);
            rewardText.rectTransform.localScale = _initialRewardScale;

            float duration = Mathf.Max(0.25f, _rollDuration * 1.4f);
            _rollTween = DOTween.To(
                    () => displayedValue,
                    value =>
                    {
                        displayedValue = value;
                        rewardText.text = string.Format(rewardFormat, value);
                    },
                    rewardAmount,
                    duration)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    _hasRollTween = false;
                    rewardText.rectTransform.localScale = _initialRewardScale;
                    _punchTween = rewardText.rectTransform
                        .DOPunchScale(_initialRewardScale * 0.24f, 0.32f, 6, 0.6f)
                        .SetUpdate(true);
                    _hasPunchTween = true;
                });
            _hasRollTween = true;
        }

        private void KillTweens()
        {
            if (_hasRollTween)
            {
                _rollTween.Kill();
                _hasRollTween = false;
            }

            if (_hasPunchTween)
            {
                _punchTween.Kill();
                _hasPunchTween = false;
                rewardText.rectTransform.localScale = _initialRewardScale;
            }
        }
    }
}
