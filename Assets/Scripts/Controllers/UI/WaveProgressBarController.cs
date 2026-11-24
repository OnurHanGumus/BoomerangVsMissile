using DG.Tweening;
using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers.UI
{
    public class WaveProgressBarController : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables
        [SerializeField] private CanvasGroup barCanvasGroup;
        [SerializeField] private RectTransform barRectTransform;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI levelLabelText;
        [SerializeField] private TextMeshProUGUI countText;

        [Header("Fill Color Ramp")]
        [SerializeField] private Color startFillColor = new Color(0.17f, 0.85f, 1.0f, 1f);
        [SerializeField] private Color endFillColor = new Color(1.0f, 0.85f, 0.28f, 1f);
        #endregion

        #region Private Variables
        private Vector3 _initialCountScale;
        private Vector3 _initialBarScale;
        private Tween _fillTween;
        private Tween _punchTween;
        private bool _hasFillTween;
        private bool _hasPunchTween;
        private int _lastDestroyed = -1;
        #endregion

        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }

        private void Init()
        {
            _initialCountScale = countText.rectTransform.localScale;
            _initialBarScale = barRectTransform.localScale;
            barCanvasGroup.alpha = 0f;
            progressSlider.value = 0f;
            fillImage.color = startFillColor;
        }

        #region Event Subscription
        private void SubscribeEvents()
        {
            MissileSignals.Instance.onWaveProgressChanged += OnWaveProgressChanged;
            CoreGameSignals.Instance.onPlay += OnPlay;
            CoreGameSignals.Instance.onLevelInitialize += OnLevelInitialize;
            CoreGameSignals.Instance.onRestartLevel += OnRestartLevel;
        }

        private void UnsubscribeEvents()
        {
            MissileSignals.Instance.onWaveProgressChanged -= OnWaveProgressChanged;
            CoreGameSignals.Instance.onPlay -= OnPlay;
            CoreGameSignals.Instance.onLevelInitialize -= OnLevelInitialize;
            CoreGameSignals.Instance.onRestartLevel -= OnRestartLevel;
        }

        private void OnDestroy()
        {
            KillTweens();
            UnsubscribeEvents();
        }
        #endregion

        private void OnPlay()
        {
            RefreshLevelLabel();
        }

        private void OnLevelInitialize()
        {
            RefreshLevelLabel();
        }

        private void OnRestartLevel()
        {
            _lastDestroyed = -1;
            KillTweens();
            progressSlider.value = 0f;
            fillImage.color = startFillColor;
            RefreshLevelLabel();
        }

        private void RefreshLevelLabel()
        {
            int levelId = LevelSignals.Instance.onGetLevelId();
            levelLabelText.text = $"LVL {levelId + 1}";
        }

        private void OnWaveProgressChanged(int destroyedCount, int totalCount)
        {
            if (totalCount <= 0)
            {
                barCanvasGroup.alpha = 0f;
                return;
            }

            barCanvasGroup.alpha = 1f;
            RefreshLevelLabel();

            int clampedDestroyed = Mathf.Clamp(destroyedCount, 0, totalCount);
            float normalized = Mathf.Clamp01((float)clampedDestroyed / totalCount);

            countText.text = $"{clampedDestroyed}/{totalCount}";
            fillImage.color = Color.Lerp(startFillColor, endFillColor, normalized);

            if (_hasFillTween)
            {
                _fillTween.Kill();
                _hasFillTween = false;
            }

            if (clampedDestroyed == 0)
            {
                progressSlider.value = 0f;
            }
            else
            {
                _fillTween = progressSlider
                    .DOValue(normalized, 0.22f)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true);
                _hasFillTween = true;
            }

            if (clampedDestroyed > _lastDestroyed && clampedDestroyed > 0)
            {
                if (_hasPunchTween)
                {
                    _punchTween.Kill();
                    _hasPunchTween = false;
                }

                countText.rectTransform.localScale = _initialCountScale;
                _punchTween = countText.rectTransform
                    .DOPunchScale(_initialCountScale * 0.22f, 0.22f, 6, 0.8f)
                    .SetUpdate(true);
                _hasPunchTween = true;
            }

            _lastDestroyed = clampedDestroyed;
        }

        private void KillTweens()
        {
            if (_hasFillTween)
            {
                _fillTween.Kill();
                _hasFillTween = false;
            }

            if (_hasPunchTween)
            {
                _punchTween.Kill();
                _hasPunchTween = false;
                countText.rectTransform.localScale = _initialCountScale;
            }

            barRectTransform.localScale = _initialBarScale;
        }
    }
}
