using Signals;
using UnityEngine;

namespace Controllers
{
    public class SafeAreaVisualController : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables

        [Header("Line References")]
        [SerializeField] private LineRenderer coreLine;
        [SerializeField] private LineRenderer glowLine;

        [Header("Geometry")]
        [SerializeField] private float barrierYOffset = 0.50f;
        [SerializeField] private float halfWidth = 5.6f;
        [SerializeField] private float warningHeight = 6.5f;

        [Header("Width & Intensity")]
        [SerializeField] private float calmCoreWidth = 0.09f;
        [SerializeField] private float calmGlowWidth = 0.42f;
        [SerializeField] private float dangerCoreWidth = 0.14f;
        [SerializeField] private float dangerGlowWidth = 0.68f;
        [SerializeField] private float menuAlphaMultiplier = 0.16f;

        #endregion

        #region Private Variables

        private static readonly Color CalmColor = new Color(0.06f, 0.82f, 1.25f, 1f);
        private static readonly Color WarnColor = new Color(1.45f, 0.85f, 0.08f, 1f);
        private static readonly Color DangerColor = new Color(1.85f, 0.14f, 0.18f, 1f);
        private static readonly Color VictoryColor = new Color(0.25f, 1.45f, 0.65f, 1f);

        private readonly Collider[] _overlapBuffer = new Collider[24];
        private readonly Gradient _coreGradient = new Gradient();
        private readonly Gradient _glowGradient = new Gradient();
        private readonly GradientColorKey[] _colorKeys = new GradientColorKey[3];
        private readonly GradientAlphaKey[] _coreAlphaKeys = new GradientAlphaKey[4];
        private readonly GradientAlphaKey[] _glowAlphaKeys = new GradientAlphaKey[4];

        private bool _isGameplayActive;
        private float _currentDanger;
        private float _currentVisibility;
        private float _breachFlashTimer;
        private float _victoryFlashTimer;
        private float _phase;

        #endregion

        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }

        private void Init()
        {
            SetupGeometry();
            _currentVisibility = menuAlphaMultiplier;
            ApplyVisualState(CalmColor, 0f, _currentVisibility);
        }

        #region Event Subscription

        private void SubscribeEvents()
        {
            CoreGameSignals.Instance.onPlay += OnPlay;
            CoreGameSignals.Instance.onRestartLevel += OnRestartLevel;
            CoreGameSignals.Instance.onLevelFailed += OnLevelFailed;
            CoreGameSignals.Instance.onLevelSuccessful += OnLevelSuccessful;
        }

        private void UnsubscribeEvents()
        {
            CoreGameSignals.Instance.onPlay -= OnPlay;
            CoreGameSignals.Instance.onRestartLevel -= OnRestartLevel;
            CoreGameSignals.Instance.onLevelFailed -= OnLevelFailed;
            CoreGameSignals.Instance.onLevelSuccessful -= OnLevelSuccessful;
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        #endregion

        private void SetupGeometry()
        {
            float y = transform.position.y + barrierYOffset;
            Vector3 left = new Vector3(-halfWidth, y, -0.05f);
            Vector3 mid = new Vector3(0f, y, -0.05f);
            Vector3 right = new Vector3(halfWidth, y, -0.05f);

            ConfigureLinePositions(coreLine, left, mid, right);
            ConfigureLinePositions(glowLine, left, mid, right);
        }

        private void ConfigureLinePositions(LineRenderer lr, Vector3 left, Vector3 mid, Vector3 right)
        {
            lr.useWorldSpace = true;
            lr.positionCount = 3;
            lr.SetPosition(0, left);
            lr.SetPosition(1, mid);
            lr.SetPosition(2, right);
        }

        private void OnPlay()
        {
            _isGameplayActive = true;
            _breachFlashTimer = 0f;
            _victoryFlashTimer = 0f;
            _currentDanger = 0f;
        }

        private void OnRestartLevel()
        {
            _isGameplayActive = false;
            _breachFlashTimer = 0f;
            _victoryFlashTimer = 0f;
            _currentDanger = 0f;
        }

        private void OnLevelFailed()
        {
            _isGameplayActive = false;
            _breachFlashTimer = 0.65f;
        }

        private void OnLevelSuccessful()
        {
            _isGameplayActive = false;
            _victoryFlashTimer = 0.85f;
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            float pulseSpeed = Mathf.Lerp(2.4f, 14f, _currentDanger);
            _phase += dt * pulseSpeed;

            if (_breachFlashTimer > 0f)
            {
                _breachFlashTimer = Mathf.Max(0f, _breachFlashTimer - dt);
                float flash = Mathf.Sin(_breachFlashTimer * 28f) * 0.3f + 0.85f;
                ApplyVisualState(DangerColor * 1.15f, 1f, flash);
                return;
            }

            if (_victoryFlashTimer > 0f)
            {
                _victoryFlashTimer = Mathf.Max(0f, _victoryFlashTimer - dt);
                float fade = Mathf.Clamp01(_victoryFlashTimer / 0.85f);
                ApplyVisualState(VictoryColor, 0.25f, fade);
                return;
            }

            float targetVisibility = _isGameplayActive ? 1f : menuAlphaMultiplier;
            _currentVisibility = Mathf.MoveTowards(_currentVisibility, targetVisibility, dt * 3.5f);

            float targetDanger = _isGameplayActive ? EvaluateClosestMissileDanger() : 0f;
            _currentDanger = Mathf.Lerp(_currentDanger, targetDanger, dt * 8f);

            Color blendedColor = _currentDanger < 0.5f
                ? Color.Lerp(CalmColor, WarnColor, _currentDanger * 2f)
                : Color.Lerp(WarnColor, DangerColor, (_currentDanger - 0.5f) * 2f);

            float shimmer = 0.86f + Mathf.Sin(_phase) * Mathf.Lerp(0.08f, 0.22f, _currentDanger);
            ApplyVisualState(blendedColor, _currentDanger, _currentVisibility * shimmer);
        }

        private float EvaluateClosestMissileDanger()
        {
            float barrierY = transform.position.y + barrierYOffset;
            Vector3 scanCenter = new Vector3(0f, barrierY + warningHeight * 0.5f, 0f);
            Vector3 scanHalfExtents = new Vector3(halfWidth + 1.5f, warningHeight * 0.5f, 1.5f);

            int hitCount = Physics.OverlapBoxNonAlloc(
                scanCenter,
                scanHalfExtents,
                _overlapBuffer,
                Quaternion.identity,
                ~0,
                QueryTriggerInteraction.Collide);

            float closestDist = warningHeight;
            for (int i = 0; i < hitCount; i++)
            {
                Collider col = _overlapBuffer[i];
                if (!col.CompareTag("Missile"))
                {
                    continue;
                }

                float missileBottomY = col.bounds.min.y;
                float dist = Mathf.Max(0f, missileBottomY - barrierY);
                if (dist < closestDist)
                {
                    closestDist = dist;
                }
            }

            return Mathf.Clamp01(1f - (closestDist / warningHeight));
        }

        private void ApplyVisualState(Color baseColor, float danger, float visibility)
        {
            float coreWidth = Mathf.Lerp(calmCoreWidth, dangerCoreWidth, danger);
            float glowWidth = Mathf.Lerp(calmGlowWidth, dangerGlowWidth, danger);
            coreLine.widthMultiplier = coreWidth;
            glowLine.widthMultiplier = glowWidth;

            Color brightCore = Color.Lerp(baseColor, Color.white * 1.4f, 0.45f);
            _colorKeys[0] = new GradientColorKey(baseColor, 0f);
            _colorKeys[1] = new GradientColorKey(brightCore, 0.5f);
            _colorKeys[2] = new GradientColorKey(baseColor, 1f);

            float coreAlpha = Mathf.Clamp01(0.95f * visibility);
            _coreAlphaKeys[0] = new GradientAlphaKey(0f, 0f);
            _coreAlphaKeys[1] = new GradientAlphaKey(coreAlpha, 0.12f);
            _coreAlphaKeys[2] = new GradientAlphaKey(coreAlpha, 0.88f);
            _coreAlphaKeys[3] = new GradientAlphaKey(0f, 1f);
            _coreGradient.SetKeys(_colorKeys, _coreAlphaKeys);
            coreLine.colorGradient = _coreGradient;

            _colorKeys[1] = new GradientColorKey(baseColor, 0.5f);
            float glowAlpha = Mathf.Clamp01(Mathf.Lerp(0.35f, 0.62f, danger) * visibility);
            _glowAlphaKeys[0] = new GradientAlphaKey(0f, 0f);
            _glowAlphaKeys[1] = new GradientAlphaKey(glowAlpha, 0.14f);
            _glowAlphaKeys[2] = new GradientAlphaKey(glowAlpha, 0.86f);
            _glowAlphaKeys[3] = new GradientAlphaKey(0f, 1f);
            _glowGradient.SetKeys(_colorKeys, _glowAlphaKeys);
            glowLine.colorGradient = _glowGradient;
        }
    }
}
