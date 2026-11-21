using Enums;
using Interfaces;
using Managers;
using UnityEngine;

namespace Controllers.Missile.Abilities
{
    public class MissileThrusterAbility : MonoBehaviour, IMissileAbility
    {
        #region Self Variables

        #region Serialized Variables

        [SerializeField] private Transform thrusterMount;
        [SerializeField] private ParticleSystem flameParticles;
        [SerializeField] private TrailRenderer contrailRenderer;
        [SerializeField] private float baseContrailTime = 0.42f;
        [SerializeField] private float nozzleYInset = 0.04f;

        #endregion

        #region Private Variables

        private int _pendingStartFrames;

        #endregion

        #endregion

        public void Initialize(MissileManager manager)
        {
            ConfigureNozzlePlacement();
            ConfigurePalette(manager.MissileType);
        }

        private void OnEnable()
        {
            contrailRenderer.emitting = false;
            contrailRenderer.Clear();
            flameParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _pendingStartFrames = 2;
        }

        private void LateUpdate()
        {
            if (_pendingStartFrames <= 0)
            {
                return;
            }

            _pendingStartFrames--;
            if (_pendingStartFrames == 0)
            {
                contrailRenderer.Clear();
                contrailRenderer.emitting = true;
                flameParticles.Play();
            }
        }

        private void OnDisable()
        {
            _pendingStartFrames = 0;
            contrailRenderer.emitting = false;
            contrailRenderer.Clear();
            flameParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void ConfigureNozzlePlacement()
        {
            Transform meshTransform = transform.Find("Parent/Mesh");
            Bounds localBounds = meshTransform.GetComponent<MeshRenderer>().localBounds;

            Vector3 pMin = transform.InverseTransformPoint(meshTransform.TransformPoint(localBounds.min));
            Vector3 pMax = transform.InverseTransformPoint(meshTransform.TransformPoint(localBounds.max));

            float centerX = (pMin.x + pMax.x) * 0.5f;
            float centerZ = (pMin.z + pMax.z) * 0.5f;
            float topY = Mathf.Max(pMin.y, pMax.y) - nozzleYInset;
            float bodyWidth = Mathf.Abs(pMax.x - pMin.x);

            thrusterMount.localPosition = new Vector3(centerX, topY, centerZ);

            float sizeFactor = Mathf.Clamp(bodyWidth / 0.58f, 0.55f, 1.75f);
            contrailRenderer.widthMultiplier = 0.28f * sizeFactor;
            contrailRenderer.time = baseContrailTime;

            ParticleSystem.MainModule main = flameParticles.main;
            main.startSize = new ParticleSystem.MinMaxCurve(0.30f * sizeFactor, 0.52f * sizeFactor);

            ParticleSystem.ShapeModule shape = flameParticles.shape;
            shape.radius = 0.07f * sizeFactor;
        }

        private void ConfigurePalette(MissileEnums missileType)
        {
            Color flameStart;
            Color flameEnd;
            Color contrailStart;
            Color contrailEnd;

            switch (missileType)
            {
                case MissileEnums.Missile_3_Exploder:
                    flameStart = new Color(1.85f, 0.28f, 0.10f, 1f);
                    flameEnd = new Color(1.40f, 0.06f, 0.04f, 0f);
                    contrailStart = new Color(0.95f, 0.24f, 0.14f, 0.80f);
                    contrailEnd = new Color(0.45f, 0.10f, 0.08f, 0f);
                    break;

                case MissileEnums.Missile_4_Armored:
                case MissileEnums.Missile_7_Armored_v2:
                case MissileEnums.Missile_9_Armored_v3:
                case MissileEnums.Missile_10_Armored_v4:
                    flameStart = new Color(0.25f, 1.15f, 1.80f, 1f);
                    flameEnd = new Color(0.08f, 0.42f, 1.25f, 0f);
                    contrailStart = new Color(0.30f, 0.68f, 0.96f, 0.74f);
                    contrailEnd = new Color(0.24f, 0.38f, 0.55f, 0f);
                    break;

                case MissileEnums.Missile_5_Cluster:
                case MissileEnums.Missile_6_Cluster_Small:
                case MissileEnums.Missile_8_Cluster_v2:
                    flameStart = new Color(1.65f, 0.22f, 1.50f, 1f);
                    flameEnd = new Color(0.85f, 0.08f, 1.20f, 0f);
                    contrailStart = new Color(0.82f, 0.28f, 0.92f, 0.76f);
                    contrailEnd = new Color(0.42f, 0.14f, 0.54f, 0f);
                    break;

                case MissileEnums.Missile_11_Zigzag:
                    flameStart = new Color(1.45f, 1.75f, 0.10f, 1f);
                    flameEnd = new Color(0.75f, 1.25f, 0.04f, 0f);
                    contrailStart = new Color(0.88f, 0.95f, 0.12f, 0.82f);
                    contrailEnd = new Color(0.50f, 0.65f, 0.08f, 0f);
                    break;

                case MissileEnums.Missile_15_Aegis:
                case MissileEnums.Missile_16_Aegis_v2:
                    flameStart = new Color(0.10f, 1.65f, 1.55f, 1f);
                    flameEnd = new Color(0.04f, 0.85f, 1.10f, 0f);
                    contrailStart = new Color(0.18f, 0.90f, 0.90f, 0.78f);
                    contrailEnd = new Color(0.12f, 0.50f, 0.58f, 0f);
                    break;

                case MissileEnums.Missile_17_Reward:
                    flameStart = new Color(1.90f, 1.45f, 0.12f, 1f);
                    flameEnd = new Color(1.50f, 0.78f, 0.04f, 0f);
                    contrailStart = new Color(1.00f, 0.82f, 0.18f, 0.82f);
                    contrailEnd = new Color(0.82f, 0.50f, 0.08f, 0f);
                    break;

                default:
                    flameStart = new Color(1.80f, 0.85f, 0.08f, 1f);
                    flameEnd = new Color(1.38f, 0.24f, 0.04f, 0f);
                    contrailStart = new Color(0.78f, 0.52f, 0.30f, 0.70f);
                    contrailEnd = new Color(0.50f, 0.44f, 0.38f, 0f);
                    break;
            }

            ParticleSystem.MainModule main = flameParticles.main;
            main.startColor = new ParticleSystem.MinMaxGradient(flameStart, Color.Lerp(flameStart, Color.white, 0.25f));

            ParticleSystem.ColorOverLifetimeModule col = flameParticles.colorOverLifetime;
            col.enabled = true;
            Gradient flameGrad = new Gradient();
            flameGrad.SetKeys(
                new[]
                {
                    new GradientColorKey(flameStart, 0f),
                    new GradientColorKey(flameEnd, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.95f, 0f),
                    new GradientAlphaKey(0.65f, 0.45f),
                    new GradientAlphaKey(0f, 1f)
                });
            col.color = flameGrad;

            Gradient contrailGrad = new Gradient();
            contrailGrad.SetKeys(
                new[]
                {
                    new GradientColorKey(contrailStart, 0f),
                    new GradientColorKey(contrailEnd, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(contrailStart.a, 0f),
                    new GradientAlphaKey(contrailStart.a * 0.45f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                });
            contrailRenderer.colorGradient = contrailGrad;
        }
    }
}
