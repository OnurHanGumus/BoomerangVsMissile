using DG.Tweening;
using Signals;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers.UI
{
    public class ChargeSliderController : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables

        [Header("UI Elements (Optional / Auto-Generated)")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Slider slider;
        [SerializeField] private Image fillImage;
        [SerializeField] private Image backgroundImage;

        [Header("Visual Tuning")]
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.9f, 0f);
        [SerializeField] private Gradient chargeGradient;

        #endregion

        #region Private Variables

        private bool _isFullyCharged = false;
        private Tween _pulseTween;
        private Transform _targetTransform;

        #endregion

        #endregion

        private void Awake()
        {
            SetupGradient();
            EnsureUIComponents();
            SubscribeEvents();
            HideImmediate();
        }

        private void SetupGradient()
        {
            if (chargeGradient == null || chargeGradient.colorKeys.Length == 0)
            {
                chargeGradient = new Gradient();
                GradientColorKey[] colorKeys = new GradientColorKey[3];
                colorKeys[0] = new GradientColorKey(new Color(0.2f, 0.9f, 1f), 0f);    // Cyan
                colorKeys[1] = new GradientColorKey(new Color(1f, 0.85f, 0.1f), 0.6f); // Amber / Yellow
                colorKeys[2] = new GradientColorKey(new Color(1f, 0.2f, 0.35f), 1f);   // Crimson / Red

                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
                alphaKeys[0] = new GradientAlphaKey(1f, 0f);
                alphaKeys[1] = new GradientAlphaKey(1f, 1f);

                chargeGradient.SetKeys(colorKeys, alphaKeys);
            }
        }

        private void EnsureUIComponents()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponentInChildren<CanvasGroup>();
            }

            if (slider == null)
            {
                slider = GetComponentInChildren<Slider>();
            }

            if (fillImage == null && slider != null && slider.fillRect != null)
            {
                fillImage = slider.fillRect.GetComponent<Image>();
            }

            // Auto-create World-Space Canvas UI hierarchy if not present
            if (canvasGroup == null || slider == null)
            {
                CreateProceduralWorldCanvas();
            }
        }

        private void CreateProceduralWorldCanvas()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 50;

            RectTransform canvasRect = GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(1.5f, 0.3f);
            transform.localScale = Vector3.one;

            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (slider == null)
            {
                GameObject sliderObj = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
                sliderObj.transform.SetParent(transform, false);
                slider = sliderObj.GetComponent<Slider>();
                RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
                sliderRect.sizeDelta = new Vector2(1.2f, 0.18f);

                // Background
                GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
                bgObj.transform.SetParent(sliderObj.transform, false);
                backgroundImage = bgObj.GetComponent<Image>();
                backgroundImage.color = new Color(0.1f, 0.1f, 0.15f, 0.75f);
                RectTransform bgRect = bgObj.GetComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.sizeDelta = Vector2.zero;

                // Fill Area & Fill Image
                GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
                fillArea.transform.SetParent(sliderObj.transform, false);
                RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
                fillAreaRect.anchorMin = Vector2.zero;
                fillAreaRect.anchorMax = Vector2.one;
                fillAreaRect.sizeDelta = new Vector2(-0.04f, -0.04f);

                GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
                fillObj.transform.SetParent(fillArea.transform, false);
                fillImage = fillObj.GetComponent<Image>();
                fillImage.color = chargeGradient.Evaluate(0f);
                RectTransform fillRect = fillObj.GetComponent<RectTransform>();
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = Vector2.one;
                fillRect.sizeDelta = Vector2.zero;

                slider.targetGraphic = fillImage;
                slider.fillRect = fillRect;
                slider.minValue = 0f;
                slider.maxValue = 1f;
                slider.value = 0f;
            }
        }

        private void SubscribeEvents()
        {
            InputSignals.Instance.onChargeUpdated += OnChargeUpdated;
            InputSignals.Instance.onChargeEnded += OnChargeEnded;
            CoreGameSignals.Instance.onRestartLevel += OnChargeEnded;
            CoreGameSignals.Instance.onLevelFailed += OnChargeEnded;
        }

        private void OnChargeUpdated(float progress, Vector3 worldPosition)
        {
            transform.position = worldPosition + worldOffset;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            if (slider != null)
            {
                slider.value = progress;
            }

            if (fillImage != null && chargeGradient != null)
            {
                fillImage.color = chargeGradient.Evaluate(progress);
            }

            // Pulse effect when fully charged
            if (progress >= 0.99f && !_isFullyCharged)
            {
                _isFullyCharged = true;
                _pulseTween?.Kill();
                transform.localScale = Vector3.one;
                _pulseTween = transform.DOPunchScale(new Vector3(0.25f, 0.25f, 0f), 0.25f, 10, 1)
                    .SetUpdate(true);
            }
            else if (progress < 0.99f)
            {
                _isFullyCharged = false;
            }
        }

        private void OnChargeEnded()
        {
            _isFullyCharged = false;
            _pulseTween?.Kill();

            if (canvasGroup != null)
            {
                canvasGroup.DOFade(0f, 0.15f).SetUpdate(true).OnComplete(() =>
                {
                    if (slider != null)
                    {
                        slider.value = 0f;
                    }
                    transform.localScale = Vector3.one;
                });
            }
        }

        private void HideImmediate()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
            if (slider != null)
            {
                slider.value = 0f;
            }
        }
    }
}
