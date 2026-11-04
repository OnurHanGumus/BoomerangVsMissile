using DG.Tweening;
using Enums;
using Signals;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers.UI
{
    public class WeeklyTaskButtonController : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables
        [SerializeField] private Button button;
        [SerializeField] private GameObject notificationBadge;
        [SerializeField] private bool enableAvailablePulse = true;
        #endregion

        #region Private Variables
        private Tween _pulseTween;
        private Vector3 _originalScale = Vector3.one;
        #endregion

        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }

        private void Start()
        {
            UpdateBadgeVisuals();
        }

        private void Init()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
                if (button == null)
                {
                    button = GetComponentInChildren<Button>();
                }
            }

            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClicked);
                button.onClick.AddListener(OnButtonClicked);
            }

            _originalScale = transform.localScale;
        }

        #region Event Subscription
        private void SubscribeEvents()
        {
            if (WeeklyTaskSignals.Instance != null)
            {
                WeeklyTaskSignals.Instance.onWeeklyTasksRefreshed += UpdateBadgeVisuals;
                WeeklyTaskSignals.Instance.onTaskCompleted += OnTaskCompleted;
                WeeklyTaskSignals.Instance.onTaskProgressChanged += OnTaskProgressChanged;
            }

            if (CoreGameSignals.Instance != null)
            {
                CoreGameSignals.Instance.onReset += UpdateBadgeVisuals;
            }
        }

        private void UnsubscribeEvents()
        {
            if (WeeklyTaskSignals.Instance != null)
            {
                WeeklyTaskSignals.Instance.onWeeklyTasksRefreshed -= UpdateBadgeVisuals;
                WeeklyTaskSignals.Instance.onTaskCompleted -= OnTaskCompleted;
                WeeklyTaskSignals.Instance.onTaskProgressChanged -= OnTaskProgressChanged;
            }

            if (CoreGameSignals.Instance != null)
            {
                CoreGameSignals.Instance.onReset -= UpdateBadgeVisuals;
            }
        }

        private void OnDestroy()
        {
            _pulseTween?.Kill();
            UnsubscribeEvents();
        }
        #endregion

        private void OnEnable()
        {
            UpdateBadgeVisuals();
        }

        private void OnTaskCompleted(int slotIndex)
        {
            UpdateBadgeVisuals();
        }

        private void OnTaskProgressChanged(int slotIndex)
        {
            UpdateBadgeVisuals();
        }

        public void OnButtonClicked()
        {
            AudioSignals.Instance.onPlaySound?.Invoke(AudioSoundEnums.Click);
            PitchSignals.Instance.onPlayPitch?.Invoke(PitchEnums.Click);

            UISignals.Instance.onOpenPanel?.Invoke(UIPanels.WeeklyTaskPanel);
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.StartPanel);
        }

        private void UpdateBadgeVisuals()
        {
            bool hasClaimable = false;
            if (WeeklyTaskSignals.Instance != null)
            {
                hasClaimable = WeeklyTaskSignals.Instance.onHasClaimable();
            }

            if (notificationBadge != null)
            {
                notificationBadge.SetActive(hasClaimable);
            }

            if (enableAvailablePulse)
            {
                _pulseTween?.Kill();
                if (hasClaimable)
                {
                    transform.localScale = _originalScale;
                    _pulseTween = transform.DOScale(_originalScale * 1.08f, 0.8f)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine)
                        .SetUpdate(true);
                }
                else
                {
                    transform.localScale = _originalScale;
                }
            }
        }
    }
}
