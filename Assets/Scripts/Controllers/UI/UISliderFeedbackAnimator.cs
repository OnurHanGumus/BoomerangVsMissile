using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Controllers.UI
{
    [RequireComponent(typeof(Slider))]
    public class UISliderFeedbackAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        #region Self Variables

        #region Serialized Variables

        [SerializeField] private RectTransform handleTransform;
        [SerializeField] private float dragScaleMultiplier = 1.18f;
        [SerializeField] private float pressDuration = 0.12f;
        [SerializeField] private float releaseDuration = 0.15f;

        #endregion

        #region Private Variables

        private Slider _slider;
        private Vector3 _initialHandleScale;
        private Tween _handleTween;
        private bool _hasHandleTween;

        #endregion

        #endregion

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _slider = GetComponent<Slider>();
            _initialHandleScale = handleTransform.localScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_slider.interactable)
            {
                return;
            }

            KillHandleTween();
            _handleTween = handleTransform
                .DOScale(_initialHandleScale * dragScaleMultiplier, pressDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
            _hasHandleTween = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            KillHandleTween();
            _handleTween = handleTransform
                .DOScale(_initialHandleScale, releaseDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
            _hasHandleTween = true;
        }

        private void KillHandleTween()
        {
            if (!_hasHandleTween)
            {
                return;
            }

            _handleTween.Kill();
            _hasHandleTween = false;
        }

        private void OnDisable()
        {
            KillHandleTween();
            handleTransform.localScale = _initialHandleScale;
        }

        private void OnDestroy()
        {
            KillHandleTween();
        }
    }
}
