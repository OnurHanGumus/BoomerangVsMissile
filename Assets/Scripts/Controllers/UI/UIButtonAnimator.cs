using Data.UnityObject;
using Data.ValueObject;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Controllers.UI
{
    [RequireComponent(typeof(Button))]
    public class UIButtonAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        #region Self Variables

        #region Serialized Variables

        [SerializeField] private RectTransform targetTransform;
        [SerializeField] private bool useGlobalDefaults = true;
        [SerializeField] private float pressScaleMultiplier = 0.92f;
        [SerializeField] private float pressDuration = 0.08f;
        [SerializeField] private float releaseDuration = 0.20f;

        #endregion

        #region Private Variables

        private Button _button;
        private Vector3 _initialScale;
        private Tween _scaleTween;
        private bool _hasScaleTween;
        private bool _isPressed;

        #endregion

        #endregion

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _button = GetComponent<Button>();
            _initialScale = targetTransform.localScale;

            if (useGlobalDefaults)
            {
                UIData uiData = Resources.Load<CD_UI>("Data/CD_UI").Data;
                pressScaleMultiplier = uiData.ButtonPressScaleMultiplier;
                pressDuration = uiData.ButtonPressDuration;
                releaseDuration = uiData.ButtonReleaseDuration;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_button.interactable)
            {
                return;
            }

            _isPressed = true;
            KillScaleTween();
            _scaleTween = targetTransform
                .DOScale(_initialScale * pressScaleMultiplier, pressDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
            _hasScaleTween = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ReleaseVisual();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ReleaseVisual();
        }

        private void ReleaseVisual()
        {
            if (!_isPressed)
            {
                return;
            }

            _isPressed = false;
            KillScaleTween();
            _scaleTween = targetTransform
                .DOScale(_initialScale, releaseDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
            _hasScaleTween = true;
        }

        private void KillScaleTween()
        {
            if (!_hasScaleTween)
            {
                return;
            }

            _scaleTween.Kill();
            _hasScaleTween = false;
        }

        private void OnDisable()
        {
            _isPressed = false;
            KillScaleTween();
            targetTransform.localScale = _initialScale;
        }

        private void OnDestroy()
        {
            KillScaleTween();
        }
    }
}
