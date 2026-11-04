using DG.Tweening;
using Enums;
using Signals;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers.UI
{
    public class FortuneWheelButtonController : MonoBehaviour
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
            UpdateAvailabilityVisuals();
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
            if (FortuneWheelSignals.Instance != null)
            {
                FortuneWheelSignals.Instance.onDailyWheelCooldownChanged += UpdateAvailabilityVisuals;
                FortuneWheelSignals.Instance.onWheelSpinCompleted += OnWheelSpinCompleted;
            }

            if (CoreGameSignals.Instance != null)
            {
                CoreGameSignals.Instance.onReset += UpdateAvailabilityVisuals;
            }
        }

        private void UnsubscribeEvents()
        {
            if (FortuneWheelSignals.Instance != null)
            {
                FortuneWheelSignals.Instance.onDailyWheelCooldownChanged -= UpdateAvailabilityVisuals;
                FortuneWheelSignals.Instance.onWheelSpinCompleted -= OnWheelSpinCompleted;
            }

            if (CoreGameSignals.Instance != null)
            {
                CoreGameSignals.Instance.onReset -= UpdateAvailabilityVisuals;
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
            UpdateAvailabilityVisuals();
        }

        private void OnWheelSpinCompleted(int index)
        {
            UpdateAvailabilityVisuals();
        }

        public void OnButtonClicked()
        {
            AudioSignals.Instance.onPlaySound?.Invoke(AudioSoundEnums.Click);
            PitchSignals.Instance.onPlayPitch?.Invoke(PitchEnums.Click);

            UISignals.Instance.onOpenPanel?.Invoke(UIPanels.FortuneWheelPanel);
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.StartPanel);
        }

        private void UpdateAvailabilityVisuals()
        {
            bool isAvailable = true;
            if (FortuneWheelSignals.Instance != null)
            {
                isAvailable = FortuneWheelSignals.Instance.onGetIsDailyWheelAvailable();
            }

            if (notificationBadge != null)
            {
                notificationBadge.SetActive(isAvailable);
            }

            if (enableAvailablePulse)
            {
                _pulseTween?.Kill();
                if (isAvailable)
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

