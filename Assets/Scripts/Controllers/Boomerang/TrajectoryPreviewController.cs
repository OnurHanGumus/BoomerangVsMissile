using System.Collections.Generic;
using Data.UnityObject;
using Data.ValueObject;
using Extensions;
using Signals;
using UnityEngine;

namespace Controllers.Boomerang
{
    [RequireComponent(typeof(LineRenderer))]
    public class TrajectoryPreviewController : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables

        [Header("Line Settings (Optional Reference / Auto-Generated)")]
        [SerializeField] private LineRenderer lineRenderer;
        [Header("Target Reticle Settings (Optional / Auto-Generated)")]
        [SerializeField] private LineRenderer targetReticleRenderer;

        #endregion

        #region Private Variables

        private PlayerData _playerData;
        private CatmullRomSpline _previewSpline;
        private bool _isAiming = false;
        private Material _lineMaterial;
        private float _textureOffset = 0f;
        private float _reticleAngle = 0f;

        #endregion

        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
            Hide();
        }

        private void Init()
        {
            _playerData = Resources.Load<CD_Player>("Data/CD_Player").Data;
            _previewSpline = new CatmullRomSpline();

            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }

            SetupLineRenderer();
            SetupTargetReticle();
        }

        private void SetupLineRenderer()
        {
            lineRenderer.useWorldSpace = true;
            lineRenderer.startWidth = _playerData.StartLineWidth;
            lineRenderer.endWidth = _playerData.EndLineWidth;
            lineRenderer.numCornerVertices = 5;
            lineRenderer.numCapVertices = 5;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;

            // Use a shader that respects LineRenderer vertex color gradients (Sprites/Default or URP Particles/Unlit)
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            }
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            }

            _lineMaterial = new Material(shader);
            _lineMaterial.name = "TrajectoryPreview_Mat";
            lineRenderer.material = _lineMaterial;
        }

        private void SetupTargetReticle()
        {
            if (targetReticleRenderer == null)
            {
                Transform reticleChild = transform.Find("TargetReticle");
                if (reticleChild == null)
                {
                    GameObject reticleObj = new GameObject("TargetReticle");
                    reticleObj.transform.SetParent(transform, false);
                    reticleChild = reticleObj.transform;
                }
                targetReticleRenderer = reticleChild.GetComponent<LineRenderer>();
                if (targetReticleRenderer == null)
                {
                    targetReticleRenderer = reticleChild.gameObject.AddComponent<LineRenderer>();
                }
            }

            targetReticleRenderer.useWorldSpace = true;
            targetReticleRenderer.loop = true;
            targetReticleRenderer.positionCount = 24;
            targetReticleRenderer.startWidth = 0.04f;
            targetReticleRenderer.endWidth = 0.04f;
            targetReticleRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            targetReticleRenderer.receiveShadows = false;
            targetReticleRenderer.material = _lineMaterial;
            targetReticleRenderer.enabled = false;
        }

        private void SubscribeEvents()
        {
            InputSignals.Instance.onChargeUpdated += OnChargeUpdated;
            InputSignals.Instance.onChargeEnded += OnChargeEnded;
            CoreGameSignals.Instance.onRestartLevel += OnChargeEnded;
            CoreGameSignals.Instance.onLevelFailed += OnChargeEnded;
            CoreGameSignals.Instance.onPlay += OnChargeEnded;
        }

        private void OnDestroy()
        {
            if (InputSignals.Instance != null)
            {
                InputSignals.Instance.onChargeUpdated -= OnChargeUpdated;
                InputSignals.Instance.onChargeEnded -= OnChargeEnded;
            }
            if (CoreGameSignals.Instance != null)
            {
                CoreGameSignals.Instance.onRestartLevel -= OnChargeEnded;
                CoreGameSignals.Instance.onLevelFailed -= OnChargeEnded;
                CoreGameSignals.Instance.onPlay -= OnChargeEnded;
            }
        }

        private void Update()
        {
            if (!_isAiming || lineRenderer == null || !lineRenderer.enabled)
            {
                return;
            }

            // Animate line texture scroll for energy flow feedback
            _textureOffset -= Time.unscaledDeltaTime * 2f;
            _reticleAngle += Time.unscaledDeltaTime * 90f;
            if (_lineMaterial != null && _lineMaterial.HasProperty("_BaseMap"))
            {
                _lineMaterial.SetTextureOffset("_BaseMap", new Vector2(_textureOffset, 0f));
            }
        }

        private void OnChargeUpdated(float progress, Vector3 targetWorldPos, float returnSwingDir)
        {
            _isAiming = true;
            lineRenderer.enabled = true;
            lineRenderer.startWidth = _playerData.StartLineWidth;
            lineRenderer.endWidth = _playerData.EndLineWidth;

            // Calculate charged dimensions
            float chargedWidth = Mathf.Lerp(_playerData.MinReturnArcWidth, _playerData.MaxReturnArcWidth, progress);
            float chargedHeight = Mathf.Lerp(_playerData.MinReturnArcHeight, _playerData.MaxReturnArcHeight, progress);

            // Origin position at player
            Vector3 initPos = new Vector3(_playerData.BoomerangInitPosX, _playerData.BoomerangInitPosY, 0f);
            Vector3 target = new Vector3(targetWorldPos.x, targetWorldPos.y, 0f);

            // Determine swing direction (selected via drag or default)
            float swingDir = returnSwingDir;

            // Apex loop point
            Vector3 apexPoint = new Vector3(
                target.x + (swingDir * chargedWidth),
                target.y + chargedHeight,
                0f
            );

            // Mid descent swoop point
            Vector3 midDescentPoint = new Vector3(
                (apexPoint.x + initPos.x) * 0.5f + (swingDir * chargedWidth * 0.4f),
                (apexPoint.y + initPos.y) * 0.5f,
                0f
            );

            // Setup Catmull-Rom spline
            List<Vector3> splinePoints = new List<Vector3>
            {
                target,
                apexPoint,
                midDescentPoint,
                initPos
            };

            _previewSpline.SetControlPoints(splinePoints);

            int directSegs = Mathf.Max(2, _playerData.DirectSegments);
            int splineSegs = Mathf.Max(2, _playerData.SplineSegments);
            int totalPoints = directSegs + splineSegs;
            lineRenderer.positionCount = totalPoints;

            // 1. Direct Phase: from initPos to target
            for (int i = 0; i < directSegs; i++)
            {
                float t = (float)i / directSegs;
                Vector3 p = Vector3.Lerp(initPos, target, t);
                p.z = 0f;
                lineRenderer.SetPosition(i, p);
            }

            // 2. Return Arc Phase: along Catmull-Rom spline
            for (int i = 0; i < splineSegs; i++)
            {
                float t = (float)i / (splineSegs - 1);
                Vector3 p = _previewSpline.Evaluate(t);
                p.z = 0f;
                lineRenderer.SetPosition(directSegs + i, p);
            }

            // 3. Render precision target reticle ring at exact clicked point
            if (targetReticleRenderer != null)
            {
                targetReticleRenderer.enabled = true;
                float reticleRadius = Mathf.Lerp(0.35f, 0.2f, progress);
                int reticlePoints = 24;
                targetReticleRenderer.positionCount = reticlePoints;
                for (int i = 0; i < reticlePoints; i++)
                {
                    float angle = (i / (float)reticlePoints) * Mathf.PI * 2f + (_reticleAngle * Mathf.Deg2Rad);
                    Vector3 reticlePoint = new Vector3(
                        target.x + Mathf.Cos(angle) * reticleRadius,
                        target.y + Mathf.Sin(angle) * reticleRadius,
                        0f
                    );
                    targetReticleRenderer.SetPosition(i, reticlePoint);
                }
            }

            // Dynamic color gradient based on charge level
            UpdateLineColors(progress);
        }

        private void UpdateLineColors(float progress)
        {
            Color currentColor;
            if (progress < 0.5f)
            {
                currentColor = Color.Lerp(_playerData.PreviewInitialColor, _playerData.PreviewMidColor, progress * 2f);
            }
            else
            {
                currentColor = Color.Lerp(_playerData.PreviewMidColor, _playerData.PreviewMaxColor, (progress - 0.5f) * 2f);
            }

            Gradient gradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[2];
            colorKeys[0] = new GradientColorKey(currentColor, 0f);
            colorKeys[1] = new GradientColorKey(currentColor, 1f);

            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[3];
            alphaKeys[0] = new GradientAlphaKey(0.9f, 0f);
            alphaKeys[1] = new GradientAlphaKey(0.85f, 0.7f);
            alphaKeys[2] = new GradientAlphaKey(0.3f, 1f); // Fades toward the catch point

            gradient.SetKeys(colorKeys, alphaKeys);
            lineRenderer.colorGradient = gradient;
            lineRenderer.startColor = currentColor;
            lineRenderer.endColor = currentColor;

            if (_lineMaterial != null)
            {
                if (_lineMaterial.HasProperty("_Color"))
                {
                    _lineMaterial.SetColor("_Color", currentColor);
                }
                if (_lineMaterial.HasProperty("_BaseColor"))
                {
                    _lineMaterial.SetColor("_BaseColor", currentColor);
                }
            }

            if (targetReticleRenderer != null)
            {
                targetReticleRenderer.startColor = currentColor;
                targetReticleRenderer.endColor = currentColor;
                targetReticleRenderer.colorGradient = gradient;
            }
        }

        private void OnChargeEnded()
        {
            Hide();
        }

        private void Hide()
        {
            _isAiming = false;
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
                lineRenderer.positionCount = 0;
            }
            if (targetReticleRenderer != null)
            {
                targetReticleRenderer.enabled = false;
                targetReticleRenderer.positionCount = 0;
            }
        }
    }
}
