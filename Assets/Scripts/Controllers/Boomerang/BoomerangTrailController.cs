using Managers;
using Signals;
using UnityEngine;

namespace Controllers.Boomerang
{
    public class BoomerangTrailController : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables

        [Header("Trail References")]
        [SerializeField] private TrailRenderer outerTrail;
        [SerializeField] private TrailRenderer coreTrail;

        [Header("Dimensions & Timing")]
        [SerializeField] private float normalWidth = 0.48f;
        [SerializeField] private float chargedWidth = 0.72f;
        [SerializeField] private float comboWidthBonus = 0.06f;
        [SerializeField] private float maxComboWidthBonus = 0.24f;
        [SerializeField] private float coreWidthRatio = 0.36f;
        [SerializeField] private float normalTime = 0.30f;
        [SerializeField] private float chargedTime = 0.40f;

        #endregion

        #region Private Variables

        private Transform _trackedBoomerang;
        private bool _isTracking;
        private bool _isFullyCharged;
        private int _comboCount;

        private Gradient _normalOuterGradient;
        private Gradient _normalCoreGradient;
        private Gradient _chargedOuterGradient;
        private Gradient _chargedCoreGradient;
        private Gradient _comboOuterGradient;
        private Gradient _comboCoreGradient;
        private Gradient _recallOuterGradient;
        private Gradient _recallCoreGradient;

        #endregion

        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }

        private void Init()
        {
            BuildGradients();
            StopAndClear();
        }

        #region Event Subscription

        private void SubscribeEvents()
        {
            BoomerangSignals.Instance.onSetChargedArc += OnSetChargedArc;
            BoomerangSignals.Instance.onBoomerangThrown += OnBoomerangThrown;
            BoomerangSignals.Instance.onPlayerTouchedBoomerang += OnBoomerangHasReturned;
            BoomerangSignals.Instance.onCombo += OnCombo;
            BoomerangSignals.Instance.onEmergencyRecall += OnEmergencyRecall;

            CoreGameSignals.Instance.onPlay += StopAndClear;
            CoreGameSignals.Instance.onRestartLevel += StopAndClear;
            CoreGameSignals.Instance.onLevelFailed += StopAndClear;
            CoreGameSignals.Instance.onLevelSuccessful += StopAndClear;
        }

        private void UnsubscribeEvents()
        {
            BoomerangSignals.Instance.onSetChargedArc -= OnSetChargedArc;
            BoomerangSignals.Instance.onBoomerangThrown -= OnBoomerangThrown;
            BoomerangSignals.Instance.onPlayerTouchedBoomerang -= OnBoomerangHasReturned;
            BoomerangSignals.Instance.onCombo -= OnCombo;
            BoomerangSignals.Instance.onEmergencyRecall -= OnEmergencyRecall;

            CoreGameSignals.Instance.onPlay -= StopAndClear;
            CoreGameSignals.Instance.onRestartLevel -= StopAndClear;
            CoreGameSignals.Instance.onLevelFailed -= StopAndClear;
            CoreGameSignals.Instance.onLevelSuccessful -= StopAndClear;
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        #endregion

        private void BuildGradients()
        {
            _normalOuterGradient = CreateGradient(
                new Color(0.10f, 0.85f, 1.35f, 1f),
                new Color(0.02f, 0.45f, 1.05f, 1f),
                0.85f);
            _normalCoreGradient = CreateGradient(
                new Color(1.25f, 1.45f, 1.60f, 1f),
                new Color(0.25f, 0.90f, 1.35f, 1f),
                0.95f);

            _chargedOuterGradient = CreateGradient(
                new Color(1.75f, 0.85f, 0.08f, 1f),
                new Color(1.55f, 0.16f, 0.06f, 1f),
                0.92f);
            _chargedCoreGradient = CreateGradient(
                new Color(2.00f, 1.85f, 1.25f, 1f),
                new Color(1.65f, 0.65f, 0.10f, 1f),
                1.00f);

            _comboOuterGradient = CreateGradient(
                new Color(1.85f, 0.60f, 0.15f, 1f),
                new Color(1.45f, 0.12f, 1.05f, 1f),
                0.95f);
            _comboCoreGradient = CreateGradient(
                new Color(2.00f, 1.80f, 1.40f, 1f),
                new Color(1.60f, 0.40f, 1.35f, 1f),
                1.00f);

            _recallOuterGradient = CreateGradient(
                new Color(1.10f, 1.55f, 1.85f, 1f),
                new Color(0.20f, 0.85f, 1.45f, 1f),
                0.90f);
            _recallCoreGradient = CreateGradient(
                new Color(1.80f, 1.95f, 2.00f, 1f),
                new Color(0.70f, 1.35f, 1.80f, 1f),
                1.00f);
        }

        private Gradient CreateGradient(Color startColor, Color endColor, float startAlpha)
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(startColor, 0f),
                    new GradientColorKey(endColor, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(startAlpha, 0f),
                    new GradientAlphaKey(startAlpha * 0.55f, 0.55f),
                    new GradientAlphaKey(0f, 1f)
                });
            return gradient;
        }

        private void OnSetChargedArc(float width, float height, float returnSwingDir, float progress)
        {
            _isFullyCharged = progress >= 0.99f;
        }

        private void LateUpdate()
        {
            if (!_isTracking)
            {
                return;
            }

            transform.position = _trackedBoomerang.position;
        }

        private void OnBoomerangThrown()
        {
            _comboCount = 0;
            _trackedBoomerang = FindFirstObjectByType<BoomerangManager>().transform;
            transform.position = _trackedBoomerang.position;
            _isTracking = true;

            outerTrail.Clear();
            coreTrail.Clear();

            if (_isFullyCharged)
            {
                ApplyTrailStyle(_chargedOuterGradient, _chargedCoreGradient, chargedWidth, chargedTime);
            }
            else
            {
                ApplyTrailStyle(_normalOuterGradient, _normalCoreGradient, normalWidth, normalTime);
            }

            outerTrail.emitting = true;
            coreTrail.emitting = true;
        }

        private void OnCombo(int value)
        {
            _comboCount = value + 1;
            float baseWidth = _isFullyCharged ? chargedWidth : normalWidth;
            float bonus = Mathf.Min(_comboCount * comboWidthBonus, maxComboWidthBonus);
            ApplyTrailStyle(_comboOuterGradient, _comboCoreGradient, baseWidth + bonus, chargedTime);
        }

        private void OnEmergencyRecall()
        {
            ApplyTrailStyle(_recallOuterGradient, _recallCoreGradient, normalWidth * 0.85f, normalTime * 0.8f);
        }

        private void OnBoomerangHasReturned()
        {
            _isTracking = false;
            _isFullyCharged = false;
            _comboCount = 0;
            outerTrail.emitting = false;
            coreTrail.emitting = false;
        }

        private void StopAndClear()
        {
            _isTracking = false;
            _isFullyCharged = false;
            _comboCount = 0;
            outerTrail.emitting = false;
            coreTrail.emitting = false;
            outerTrail.Clear();
            coreTrail.Clear();
        }

        private void ApplyTrailStyle(Gradient outerGradient, Gradient coreGradient, float width, float duration)
        {
            outerTrail.colorGradient = outerGradient;
            outerTrail.widthMultiplier = width;
            outerTrail.time = duration;

            coreTrail.colorGradient = coreGradient;
            coreTrail.widthMultiplier = width * coreWidthRatio;
            coreTrail.time = duration * 0.78f;
        }
    }
}
