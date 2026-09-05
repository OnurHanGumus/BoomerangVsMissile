using DG.Tweening;
using Data.UnityObject;
using Data.ValueObject;
using Enums;
using Interfaces;
using Managers;
using Signals;
using UnityEngine;

namespace Controllers.Missile.Abilities
{
    public class ClusterMissileAbility : MonoBehaviour, IMissileDeathEffect
    {
        [Header("Cluster Data")]
        [SerializeField] private CD_ClusterMissile cdClusterMissile;

        private ClusterMissileData _data;
        public ClusterMissileData Data
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

        public int ChildCount => Data.ClusterChildCount;
        public PoolEnums ChildType => Data.ClusterChildType;

        private MissileManager _manager;
        private Tween _clusterDelayedCall;

        private void Awake()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            CoreGameSignals.Instance.onLevelFailed += CancelDelayedSpawn;
            CoreGameSignals.Instance.onLevelSuccessful += CancelDelayedSpawn;
            CoreGameSignals.Instance.onRestartLevel += CancelDelayedSpawn;
        }

        private void UnsubscribeEvents()
        {
            CoreGameSignals.Instance.onLevelFailed -= CancelDelayedSpawn;
            CoreGameSignals.Instance.onLevelSuccessful -= CancelDelayedSpawn;
        }

        public void Initialize(MissileManager manager)
        {
            _manager = manager;
            LoadData();
        }

        private void LoadData()
        {
            if (cdClusterMissile == null)
            {
                cdClusterMissile = Resources.Load<CD_ClusterMissile>("Data/Missiles/CD_ClusterMissile");
            }
            _data = cdClusterMissile != null && cdClusterMissile.Data != null 
                ? cdClusterMissile.Data 
                : new ClusterMissileData();
        }

        public void Configure(PoolEnums childType, int childCount, float spacing, float spawnDelay)
        {
            Data.ClusterChildType = childType;
            Data.ClusterChildCount = childCount;
            Data.ClusterChildSpacing = spacing;
            Data.ClusterSpawnDelay = spawnDelay;
        }

        public bool OnMissileDeath(Vector3 position, bool isLevelEnd)
        {
            if (isLevelEnd)
            {
                return false;
            }

            Vector3 spawnPos = position;
            float spawnDelay = Data.ClusterSpawnDelay;
            if (spawnDelay > 0f)
            {
                _clusterDelayedCall = DOVirtual.DelayedCall(spawnDelay, () =>
                {
                    SpawnClusterChildren(spawnPos);
                });
            }
            else
            {
                SpawnClusterChildren(spawnPos);
            }

            return false;
        }

        private void SpawnClusterChildren(Vector3 centerPos)
        {
            float halfSpacing = Data.ClusterChildSpacing * 1f;
            int childCount = Data.ClusterChildCount;
            PoolEnums childType = Data.ClusterChildType;

            for (int i = 0; i < childCount; i++)
            {
                GameObject child = PoolSignals.Instance.onGetObject?.Invoke(childType);
                if (child != null)
                {
                    float yOffset = (childCount == 1) ? 0f : ((i == 0) ? halfSpacing : -halfSpacing);
                    child.transform.position = centerPos + new Vector3(0, yOffset, 0);

                    var rb = child.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }

                    child.SetActive(true);
                }
            }
        }

        public void CancelDelayedSpawn()
        {
            _clusterDelayedCall?.Kill();
        }

        private void OnEnable()
        {
            CancelDelayedSpawn();
        }

        private void OnDestroy()
        {
            CancelDelayedSpawn();
            UnsubscribeEvents();
        }
    }
}
