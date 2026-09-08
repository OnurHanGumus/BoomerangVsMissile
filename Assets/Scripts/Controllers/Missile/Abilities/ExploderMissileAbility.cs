using System.Collections.Generic;
using Data.UnityObject;
using Data.ValueObject;
using Enums;
using Interfaces;
using Managers;
using Signals;
using UnityEngine;

namespace Controllers.Missile.Abilities
{
    public class ExploderMissileAbility : MonoBehaviour, IMissileDeathEffect
    {
        [Header("Exploder Data")]
        [SerializeField] private CD_ExploderMissile cdExploderMissile;

        private ExploderMissileData _data;
        public ExploderMissileData Data
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

        public float ExplosionRadius => Data != null ? Data.ExplosionRadius : 4f;
        public float FallSpeed => Data != null ? Data.FallSpeed : 3.5f;
        public float PathCheckWidth => Data != null ? Data.PathCheckWidth : 0.2f;

        private MissileManager _manager;
        private Rigidbody _rigidbody;

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
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }
            LoadData();
        }

        private void LoadData()
        {
            if (cdExploderMissile == null)
            {
                cdExploderMissile = Resources.Load<CD_ExploderMissile>("Data/Missiles/CD_ExploderMissile");
            }
            _data = cdExploderMissile != null && cdExploderMissile.Data != null 
                ? cdExploderMissile.Data 
                : new ExploderMissileData();
        }

        private void OnEnable()
        {
            EnsureInitialized();

            // Check if another missile is already in our fall path
            if (!CheckPathClear())
            {
                // Path is blocked: abort spawn and disable immediately
                gameObject.SetActive(false);
                return;
            }
            MissileSignals.Instance.onExploderMissileCreated?.Invoke();

            var movement = GetComponent<MissileMovementController>();
            if (movement != null)
            {
                movement.SetSpeedOverride(FallSpeed);
            }
            else if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = new Vector3(0, -FallSpeed, 0);
                _rigidbody.angularVelocity = Vector3.zero;
            }
        }

        public bool CheckPathClear()
        {
            EnsureInitialized();
            Physics.SyncTransforms();

            float checkWidth = PathCheckWidth;
            float currentX = transform.position.x;
            float currentY = transform.position.y;
            float currentZ = transform.position.z;

            // 1. Direct coordinate check on all active missiles in the scene
            var allMissiles = FindObjectsByType<MissileManager>(FindObjectsSortMode.None);
            for (int i = 0; i < allMissiles.Length; i++)
            {
                var other = allMissiles[i];
                if (other == null || other == _manager)
                {
                    continue;
                }

                if (!other.gameObject.activeInHierarchy || !other.gameObject.activeSelf || other.IsDead)
                {
                    continue;
                }

                // Check if other missile is below this missile along the Y axis
                if (other.transform.position.y < currentY)
                {
                    // Check if it is within our fall path corridor in X
                    float distanceX = Mathf.Abs(other.transform.position.x - currentX);
                    if (distanceX <= checkWidth)
                    {
                        Debug.Log($"[ExploderMissileAbility] Path blocked! Detected missile '{other.name}' below at pos ({other.transform.position.x:F2}, {other.transform.position.y:F2}). DistanceX: {distanceX:F2} <= {checkWidth:F2}");
                        return false; // Path blocked!
                    }
                }
            }

            // 2. Physics OverlapBox check along the downward fall path
            float bottomY = -2f;
            float height = Mathf.Max(1f, currentY - bottomY);
            Vector3 boxCenter = new Vector3(currentX, currentY - height * 0.5f, currentZ);
            Vector3 halfExtents = new Vector3(checkWidth, height * 0.5f, checkWidth);

            Collider[] colliders = Physics.OverlapBox(boxCenter, halfExtents, Quaternion.identity, ~0, QueryTriggerInteraction.Collide);
            for (int i = 0; i < colliders.Length; i++)
            {
                var col = colliders[i];
                if (col == null || col.transform == transform || col.transform.IsChildOf(transform))
                {
                    continue;
                }

                var otherMissile = col.GetComponentInParent<MissileManager>();
                if (otherMissile != null && otherMissile != _manager)
                {
                    if (otherMissile.gameObject.activeInHierarchy && otherMissile.gameObject.activeSelf && !otherMissile.IsDead)
                    {
                        Debug.Log($"[ExploderMissileAbility] Physics OverlapBox detected obstacle '{otherMissile.name}' on path!");
                        return false; // Path blocked!
                    }
                }
            }

            return true; // Path is clear
        }

        public bool OnMissileDeath(Vector3 position, bool isLevelEnd)
        {
            if (isLevelEnd)
            {
                return false;
            }

            float radius = ExplosionRadius;
            Collider[] colliders = Physics.OverlapSphere(position, radius);
            HashSet<MissileManager> affectedMissiles = new HashSet<MissileManager>();

            for (int i = 0; i < colliders.Length; i++)
            {
                var col = colliders[i];
                if (col == null || !col.enabled || !col.gameObject.activeInHierarchy)
                {
                    continue;
                }

                var otherMissile = col.GetComponentInParent<MissileManager>();
                if (otherMissile != null && otherMissile != _manager)
                {
                    if (otherMissile.gameObject.activeInHierarchy && otherMissile.gameObject.activeSelf && !otherMissile.IsDead)
                    {
                        affectedMissiles.Add(otherMissile);
                    }
                }
            }

            foreach (var missile in affectedMissiles)
            {
                if (missile != null && missile.gameObject.activeInHierarchy && !missile.IsDead)
                {
                    missile.Explode();
                }
            }

            AudioSignals.Instance.onPlaySound(AudioSoundEnums.Explosion1);
            return true; // Handled custom explosion audio
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, ExplosionRadius);

            Gizmos.color = Color.yellow;
            float checkWidth = PathCheckWidth;
            float bottomY = -2f;
            float height = Mathf.Max(1f, transform.position.y - bottomY);
            Vector3 boxCenter = new Vector3(transform.position.x, transform.position.y - height * 0.5f, transform.position.z);
            Gizmos.DrawWireCube(boxCenter, new Vector3(checkWidth * 2f, height, 2f));
        }
    }
}
