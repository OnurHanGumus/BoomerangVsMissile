using System.Collections.Generic;
using Data.UnityObject;
using Data.ValueObject;
using DG.Tweening;
using Enums;
using Interfaces;
using Managers;
using Signals;
using UnityEngine;

namespace Controllers.Missile.Abilities
{
    public class AegisMissileAbility : MonoBehaviour, IMissileDeathEffect
    {
        #region Self Variables

        #region Public Variables
        public static bool HasActiveGenerators => ActiveGenerators.Count > 0;

        public AegisMissileData Data
        {
            get
            {
                if (_data == null)
                {
                    LoadData();
                }
                return _data;
            }
        }

        public float ShieldRadius => Data != null ? Mathf.Max(0.5f, Data.ShieldRadius) : 3.0f;
        #endregion

        #region Serialized Variables
        [Header("Aegis Data")]
        [SerializeField] private CD_AegisMissile cdAegisMissile;
        [SerializeField] private Transform bubbleVisual;
        #endregion

        #region Private Variables
        private static readonly List<AegisMissileAbility> ActiveGenerators = new List<AegisMissileAbility>(8);
        private static readonly HashSet<Transform> RegisteredAegisTransforms = new HashSet<Transform>();

        private AegisMissileData _data;
        private MissileManager _manager;
        private Transform _cachedPhysicsTransform;
        private float _cachedParentLossyScale = 0.8f;
        private float _popInScale = 1f;
        private float _pulseTimer;
        private bool _hasCachedParentScale;
        private Tween _popInTween;
        #endregion

        #endregion

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticRegistry()
        {
            ActiveGenerators.Clear();
            RegisteredAegisTransforms.Clear();
        }

        public static bool IsPointShielded(Vector3 worldPoint, Transform targetTransform = null)
        {
            if (ActiveGenerators.Count == 0)
            {
                return false;
            }

            if (targetTransform != null && RegisteredAegisTransforms.Contains(targetTransform))
            {
                return false;
            }

            for (int i = 0; i < ActiveGenerators.Count; i++)
            {
                AegisMissileAbility gen = ActiveGenerators[i];
                if (gen == null || !gen.isActiveAndEnabled || !gen.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (gen._manager != null && gen._manager.IsDead)
                {
                    continue;
                }

                Vector3 genPos = gen.transform.position;
                float dx = worldPoint.x - genPos.x;
                float dy = worldPoint.y - genPos.y;
                float radius = gen.ShieldRadius;

                if ((dx * dx) + (dy * dy) <= radius * radius)
                {
                    return true;
                }
            }

            return false;
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        public void Initialize(MissileManager manager)
        {
            _manager = manager;
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_manager == null)
            {
                _manager = GetComponent<MissileManager>();
            }

            RegisteredAegisTransforms.Add(transform);
            if (_cachedPhysicsTransform == null)
            {
                MissilePhysicsController physicsCtrl = GetComponentInChildren<MissilePhysicsController>(true);
                if (physicsCtrl != null)
                {
                    _cachedPhysicsTransform = physicsCtrl.transform;
                }
                else
                {
                    _cachedPhysicsTransform = transform.Find("Parent/Physics");
                }
            }

            if (_cachedPhysicsTransform != null)
            {
                RegisteredAegisTransforms.Add(_cachedPhysicsTransform);
            }

            if (MissileSignals.Instance != null)
            {
                MissileSignals.Instance.onIsPointShielded = IsPointShielded;
            }

            LoadData();
            EnsureBubbleVisual();
        }

        private void LoadData()
        {
            if (cdAegisMissile == null)
            {
                cdAegisMissile = Resources.Load<CD_AegisMissile>("Data/Missiles/CD_AegisMissile");
            }

            _data = cdAegisMissile != null && cdAegisMissile.Data != null
                ? cdAegisMissile.Data
                : new AegisMissileData();
        }

        private void EnsureBubbleVisual()
        {
            if (bubbleVisual == null)
            {
                bubbleVisual = transform.Find("Parent/BubbleVisual");
                if (bubbleVisual == null)
                {
                    bubbleVisual = transform.Find("BubbleVisual");
                }
            }

            if (bubbleVisual == null)
            {
                Transform parentNode = transform.Find("Parent");
                GameObject fallbackSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                fallbackSphere.name = "BubbleVisual";
                Collider sphereCollider = fallbackSphere.GetComponent<Collider>();
                if (sphereCollider != null)
                {
                    Destroy(sphereCollider);
                }

                bubbleVisual = fallbackSphere.transform;
                bubbleVisual.SetParent(parentNode != null ? parentNode : transform, false);
                bubbleVisual.localPosition = Vector3.zero;
                bubbleVisual.localRotation = Quaternion.identity;
            }

            if (!_hasCachedParentScale && bubbleVisual.parent != null)
            {
                float parentScale = Mathf.Abs(bubbleVisual.parent.lossyScale.x);
                if (parentScale > 0.001f)
                {
                    _cachedParentLossyScale = parentScale;
                    _hasCachedParentScale = true;
                }
            }
        }

        private void OnEnable()
        {
            EnsureInitialized();

            if (!ActiveGenerators.Contains(this))
            {
                ActiveGenerators.Add(this);
            }

            _pulseTimer = 0f;
            _popInTween?.Kill();
            _popInScale = 0.2f;

            if (bubbleVisual != null)
            {
                bubbleVisual.gameObject.SetActive(true);
                UpdateBubbleScale();
            }

            _popInTween = DOVirtual.Float(0.2f, 1f, 0.22f, value =>
            {
                _popInScale = value;
                UpdateBubbleScale();
            })
            .SetEase(Ease.OutBack)
            .SetLink(gameObject);

            if (MissileSignals.Instance != null)
            {
                MissileSignals.Instance.onIsPointShielded = IsPointShielded;
                MissileSignals.Instance.onAegisStateChanged?.Invoke();
            }
        }

        private void Update()
        {
            if (bubbleVisual == null)
            {
                return;
            }

            _pulseTimer += Time.deltaTime * Data.PulseSpeed;
            UpdateBubbleScale();
        }

        private void UpdateBubbleScale()
        {
            if (bubbleVisual == null)
            {
                return;
            }

            float parentScale = _cachedParentLossyScale > 0.001f ? _cachedParentLossyScale : 0.8f;
            float baseLocalDiameter = (ShieldRadius * 2f) / parentScale;
            float pulseMultiplier = 1f + Mathf.Sin(_pulseTimer) * Data.PulseScaleAmount;
            float finalScale = baseLocalDiameter * _popInScale * pulseMultiplier;
            bubbleVisual.localScale = new Vector3(finalScale, finalScale, finalScale);
        }

        public bool OnMissileDeath(Vector3 position, bool isLevelEnd)
        {
            UnregisterGenerator(notify: true);

            if (!isLevelEnd)
            {
                if (PoolSignals.Instance != null)
                {
                    GameObject spark = PoolSignals.Instance.onGetObject?.Invoke(PoolEnums.HitSpark);
                    if (spark != null)
                    {
                        spark.transform.position = position;
                        spark.SetActive(true);
                    }
                }

                if (AudioSignals.Instance != null)
                {
                    AudioSignals.Instance.onPlaySound?.Invoke(AudioSoundEnums.ShieldBroke);
                }
            }

            return false;
        }

        private void OnDisable()
        {
            _popInTween?.Kill();
            UnregisterGenerator(notify: true);
        }

        private void OnDestroy()
        {
            _popInTween?.Kill();
            UnregisterGenerator(notify: false);
            RegisteredAegisTransforms.Remove(transform);
            if (_cachedPhysicsTransform != null)
            {
                RegisteredAegisTransforms.Remove(_cachedPhysicsTransform);
            }
        }

        private void UnregisterGenerator(bool notify)
        {
            bool removed = ActiveGenerators.Remove(this);
            if (removed && notify && MissileSignals.Instance != null)
            {
                MissileSignals.Instance.onAegisStateChanged?.Invoke();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.85f);
            Gizmos.DrawWireSphere(transform.position, ShieldRadius);
        }
    }
}
