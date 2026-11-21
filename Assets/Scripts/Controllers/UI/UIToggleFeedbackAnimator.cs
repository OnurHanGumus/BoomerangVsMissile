using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers.UI
{
    [RequireComponent(typeof(Toggle))]
    public class UIToggleFeedbackAnimator : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables

        [SerializeField] private RectTransform backgroundTransform;
        [SerializeField] private RectTransform checkmarkTransform;
        [SerializeField] private float punchStrength = 0.16f;
        [SerializeField] private float punchDuration = 0.22f;

        #endregion

        #region Private Variables

        private Toggle _toggle;
        private Vector3 _initialBgScale;
        private Vector3 _initialCheckmarkScale;
        private Tween _bgTween;
        private Tween _checkTween;
        private bool _hasBgTween;
        private bool _hasCheckTween;

        #endregion

        #endregion

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _toggle = GetComponent<Toggle>();
            _initialBgScale = backgroundTransform.localScale;
            _initialCheckmarkScale = checkmarkTransform.localScale;
            _toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void OnToggleValueChanged(bool isOn)
        {
            if (Time.frameCount <= 1)
            {
                return;
            }

            KillTweens();

            backgroundTransform.localScale = _initialBgScale;
            _bgTween = backgroundTransform
                .DOPunchScale(_initialBgScale * punchStrength, punchDuration, 6, 0.8f)
                .SetUpdate(true);
            _hasBgTween = true;

            if (isOn)
            {
                checkmarkTransform.localScale = _initialCheckmarkScale * 0.55f;
                _checkTween = checkmarkTransform
                    .DOScale(_initialCheckmarkScale, punchDuration)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true);
                _hasCheckTween = true;
            }
        }

        private void KillTweens()
        {
            if (_hasBgTween)
            {
                _bgTween.Kill();
                _hasBgTween = false;
                backgroundTransform.localScale = _initialBgScale;
            }

            if (_hasCheckTween)
            {
                _checkTween.Kill();
                _hasCheckTween = false;
                checkmarkTransform.localScale = _initialCheckmarkScale;
            }
        }

        private void OnDestroy()
        {
            _toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            KillTweens();
        }
    }
}
