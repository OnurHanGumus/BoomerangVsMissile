using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class UIAmbientMotionController : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables

        [Header("Pulse Scale")]
        [SerializeField] private bool enablePulseScale;
        [SerializeField] private float pulseScaleMultiplier = 1.06f;
        [SerializeField] private float pulseDuration = 1.1f;

        [Header("Breathing Alpha")]
        [SerializeField] private bool enableBreathingAlpha;
        [SerializeField] private float minAlpha = 0.80f;
        [SerializeField] private float alphaDuration = 1.1f;

        [Header("Float Vertical")]
        [SerializeField] private bool enableFloatVertical;
        [SerializeField] private float floatAmplitude = 6f;
        [SerializeField] private float floatDuration = 1.8f;

        [Header("Slow Rotate")]
        [SerializeField] private bool enableSlowRotate;
        [SerializeField] private float rotateDegreesPerCycle = -360f;
        [SerializeField] private float rotateDuration = 14f;

        #endregion

        #region Private Variables

        private RectTransform _rectTransform;
        private Graphic _graphic;
        private Vector3 _initialScale;
        private Vector2 _initialAnchoredPosition;
        private Vector3 _initialEulerAngles;
        private float _initialAlpha;

        private Tween _pulseTween;
        private Tween _alphaTween;
        private Tween _floatTween;
        private Tween _rotateTween;
        private bool _hasPulseTween;
        private bool _hasAlphaTween;
        private bool _hasFloatTween;
        private bool _hasRotateTween;

        #endregion

        #endregion

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _rectTransform = GetComponent<RectTransform>();
            _initialScale = _rectTransform.localScale;
            _initialAnchoredPosition = _rectTransform.anchoredPosition;
            _initialEulerAngles = _rectTransform.localEulerAngles;

            if (enableBreathingAlpha)
            {
                _graphic = GetComponent<Graphic>();
                _initialAlpha = _graphic.color.a;
            }
        }

        private void OnEnable()
        {
            StartMotions();
        }

        private void OnDisable()
        {
            StopMotions();
        }

        private void OnDestroy()
        {
            StopMotions();
        }

        private void StartMotions()
        {
            StopMotions();

            if (enablePulseScale)
            {
                _rectTransform.localScale = _initialScale;
                _pulseTween = _rectTransform
                    .DOScale(_initialScale * pulseScaleMultiplier, pulseDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(true);
                _hasPulseTween = true;
            }

            if (enableBreathingAlpha)
            {
                Color c = _graphic.color;
                c.a = _initialAlpha;
                _graphic.color = c;

                _alphaTween = _graphic
                    .DOFade(minAlpha, alphaDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(true);
                _hasAlphaTween = true;
            }

            if (enableFloatVertical)
            {
                _rectTransform.anchoredPosition = _initialAnchoredPosition;
                _floatTween = _rectTransform
                    .DOAnchorPosY(_initialAnchoredPosition.y + floatAmplitude, floatDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(true);
                _hasFloatTween = true;
            }

            if (enableSlowRotate)
            {
                _rectTransform.localEulerAngles = _initialEulerAngles;
                _rotateTween = _rectTransform
                    .DOLocalRotate(
                        new Vector3(_initialEulerAngles.x, _initialEulerAngles.y, _initialEulerAngles.z + rotateDegreesPerCycle),
                        rotateDuration,
                        RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Restart)
                    .SetUpdate(true);
                _hasRotateTween = true;
            }
        }

        private void StopMotions()
        {
            if (_hasPulseTween)
            {
                _pulseTween.Kill();
                _hasPulseTween = false;
                _rectTransform.localScale = _initialScale;
            }

            if (_hasAlphaTween)
            {
                _alphaTween.Kill();
                _hasAlphaTween = false;
                Color c = _graphic.color;
                c.a = _initialAlpha;
                _graphic.color = c;
            }

            if (_hasFloatTween)
            {
                _floatTween.Kill();
                _hasFloatTween = false;
                _rectTransform.anchoredPosition = _initialAnchoredPosition;
            }

            if (_hasRotateTween)
            {
                _rotateTween.Kill();
                _hasRotateTween = false;
                _rectTransform.localEulerAngles = _initialEulerAngles;
            }
        }
    }
}
