using DG.Tweening;
using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers.UI
{
    public class EnergyPanelController : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables
        [Header("UI References")]
        [SerializeField] private RectTransform panelRectTransform;
        [SerializeField] private RectTransform heartIconTransform;
        [SerializeField] private TextMeshProUGUI energyCountText;
        [SerializeField] private TextMeshProUGUI countdownText;

        [Header("Visual Feedback")]
        [SerializeField] private Color insufficientFlashColor = new Color(1f, 0.28f, 0.28f, 1f);
        #endregion

        #region Private Variables
        private Vector2 _initialAnchoredPosition;
        private Vector3 _initialHeartScale = Vector3.one;
        private Color _defaultCountColor = Color.white;
        private bool _hasCapturedInitialTransform;
        private int _lastDisplayedEnergy = -1;
        private int _lastDisplayedSeconds = -1;
        private Tween _shakeTween;
        private Tween _punchTween;
        private Tween _colorTween;
        #endregion

        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }

        private void Start()
        {
            RefreshFromSignals();
        }

        private void OnEnable()
        {
            RefreshFromSignals();
        }

        private void Init()
        {
            if (panelRectTransform == null)
            {
                panelRectTransform = GetComponent<RectTransform>();
            }

            if (energyCountText == null)
            {
                Transform countChild = transform.Find("Text (TMP)");
                if (countChild != null)
                {
                    energyCountText = countChild.GetComponent<TextMeshProUGUI>();
                }
            }

            if (heartIconTransform == null)
            {
                Transform heartChild = transform.Find("Hearth icon");
                if (heartChild != null)
                {
                    heartIconTransform = heartChild.GetComponent<RectTransform>();
                }
            }

            if (countdownText == null)
            {
                Transform timerChild = transform.Find("CountdownText (TMP)");
                if (timerChild != null)
                {
                    countdownText = timerChild.GetComponent<TextMeshProUGUI>();
                }
                else
                {
                    countdownText = CreateFallbackCountdownText();
                }
            }

            if (panelRectTransform != null)
            {
                _initialAnchoredPosition = panelRectTransform.anchoredPosition;
                _hasCapturedInitialTransform = true;
            }

            if (heartIconTransform != null)
            {
                _initialHeartScale = heartIconTransform.localScale;
            }

            if (energyCountText != null)
            {
                _defaultCountColor = energyCountText.color;
            }
        }

        private TextMeshProUGUI CreateFallbackCountdownText()
        {
            GameObject timerObj = new GameObject("CountdownText (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
            timerObj.transform.SetParent(transform, false);

            RectTransform rect = timerObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(-60f, -98f);
            rect.sizeDelta = new Vector2(360f, 64f);
            rect.localScale = Vector3.one;

            TextMeshProUGUI tmp = timerObj.GetComponent<TextMeshProUGUI>();
            if (energyCountText != null)
            {
                if (energyCountText.font != null)
                {
                    tmp.font = energyCountText.font;
                }
                if (energyCountText.fontSharedMaterial != null)
                {
                    tmp.fontSharedMaterial = energyCountText.fontSharedMaterial;
                }
                tmp.color = energyCountText.color;
            }
            else
            {
                tmp.color = new Color(0.314f, 0.729f, 0.980f, 1f);
            }
            tmp.fontSize = 54f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            tmp.text = string.Empty;

            timerObj.SetActive(false);
            return tmp;
        }

        #region Event Subscription
        private void SubscribeEvents()
        {
            EnergySignals.Instance.onEnergyChanged += OnEnergyChanged;
            EnergySignals.Instance.onEnergyTimerUpdated += OnEnergyTimerUpdated;
            EnergySignals.Instance.onInsufficientEnergy += OnInsufficientEnergy;
        }

        private void UnsubscribeEvents()
        {
            if (EnergySignals.Instance != null)
            {
                EnergySignals.Instance.onEnergyChanged -= OnEnergyChanged;
                EnergySignals.Instance.onEnergyTimerUpdated -= OnEnergyTimerUpdated;
                EnergySignals.Instance.onInsufficientEnergy -= OnInsufficientEnergy;
            }
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
            _shakeTween?.Kill();
            _punchTween?.Kill();
            _colorTween?.Kill();
        }
        #endregion

        private void RefreshFromSignals()
        {
            if (EnergySignals.Instance == null)
            {
                return;
            }

            int current = EnergySignals.Instance.onGetCurrentEnergy();
            int max = EnergySignals.Instance.onGetMaxEnergy();
            int remainingSeconds = EnergySignals.Instance.onGetRemainingSeconds();

            UpdateEnergyDisplay(current, max, false);
            OnEnergyTimerUpdated(remainingSeconds);
        }

        private void OnEnergyChanged(int currentEnergy, int maxEnergy)
        {
            bool shouldAnimate = _lastDisplayedEnergy >= 0 && _lastDisplayedEnergy != currentEnergy;
            UpdateEnergyDisplay(currentEnergy, maxEnergy, shouldAnimate);
        }

        private void UpdateEnergyDisplay(int currentEnergy, int maxEnergy, bool animate)
        {
            _lastDisplayedEnergy = currentEnergy;

            if (energyCountText != null)
            {
                energyCountText.text = currentEnergy.ToString();
            }

            if (animate && heartIconTransform != null)
            {
                _punchTween?.Kill();
                heartIconTransform.localScale = _initialHeartScale;
                _punchTween = heartIconTransform
                    .DOPunchScale(_initialHeartScale * 0.2f, 0.25f, 8, 1f)
                    .SetUpdate(true)
                    .SetLink(heartIconTransform.gameObject);
            }
        }

        private void OnEnergyTimerUpdated(int remainingSeconds)
        {
            if (countdownText == null)
            {
                return;
            }

            if (remainingSeconds <= 0)
            {
                _lastDisplayedSeconds = 0;
                if (countdownText.gameObject.activeSelf)
                {
                    countdownText.gameObject.SetActive(false);
                }
                return;
            }

            if (!countdownText.gameObject.activeSelf)
            {
                countdownText.gameObject.SetActive(true);
            }

            if (_lastDisplayedSeconds == remainingSeconds)
            {
                return;
            }

            _lastDisplayedSeconds = remainingSeconds;
            int minutes = remainingSeconds / 60;
            int seconds = remainingSeconds % 60;
            countdownText.text = $"{minutes:00}:{seconds:00}";
        }

        private void OnInsufficientEnergy()
        {
            if (panelRectTransform != null && _hasCapturedInitialTransform)
            {
                _shakeTween?.Kill();
                panelRectTransform.anchoredPosition = _initialAnchoredPosition;
                _shakeTween = panelRectTransform
                    .DOShakeAnchorPos(0.35f, new Vector2(22f, 0f), 18, 90f, false, true)
                    .SetUpdate(true)
                    .SetLink(panelRectTransform.gameObject)
                    .OnComplete(() =>
                    {
                        if (panelRectTransform != null)
                        {
                            panelRectTransform.anchoredPosition = _initialAnchoredPosition;
                        }
                    });
            }

            if (energyCountText != null)
            {
                _colorTween?.Kill();
                energyCountText.color = insufficientFlashColor;
                _colorTween = energyCountText
                    .DOColor(_defaultCountColor, 0.45f)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true)
                    .SetLink(energyCountText.gameObject);
            }
        }
    }
}
