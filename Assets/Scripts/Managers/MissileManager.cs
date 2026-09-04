using System;
using System.Collections.Generic;
using Commands;
using Controllers;
using Data.UnityObject;
using Data.ValueObject;
using DG.Tweening;
using Enums;
using Signals;
using UnityEngine;

namespace Managers
{
    public class MissileManager : MonoBehaviour
    {
        #region Self Variables

        #region Public Variables
        public PoolEnums ParticleType;
        public bool IsPink = false;
        public bool IsArmored => isArmored || (_missileData != null && _missileData.IsArmored);
        public bool IsCluster => isCluster;
        public int ClusterChildCount => clusterChildCount;
        public int CurrentHealth => _currentHealth;

        #endregion

        #region Serialized Variables
        [SerializeField] private MissilePhysicsController physicsController;
        [SerializeField] private MissileLightController lightController;
        [SerializeField] private CD_Missile cdMissile;
        [SerializeField] private bool isArmored = false;

        [Header("Cluster Settings")]
        [SerializeField] private bool isCluster = false;
        [SerializeField] private PoolEnums clusterChildType = PoolEnums.Missile6;
        [SerializeField] private int clusterChildCount = 2;
        [SerializeField] private float clusterChildSpacing = 0.8f;
        [SerializeField] private float clusterSpawnDelay = 0f;
        #endregion

        #region Private Variables
        private PlayerData _data;
        private MissileData _missileData;
        private int _currentHealth = 1;
        private float _lastDamageTime = -1f;
        private MeshRenderer _meshRenderer;
        private Tween _clusterDelayedCall;
        #endregion

        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }

        private void OnEnable()
        {
            ResetHealth();
        }

        private void Init()
        {
            _data = GetData();
            if (cdMissile == null)
            {
                cdMissile = Resources.Load<CD_Missile>("Data/CD_Missile");
            }
            _missileData = cdMissile != null ? cdMissile.Data : new MissileData();
            _meshRenderer = GetComponentInChildren<MeshRenderer>();
            ResetHealth();
        }

        public void ResetHealth()
        {
            int maxHealth = _missileData != null && _missileData.MaxHealth > 0
                ? _missileData.MaxHealth
                : (isArmored ? 2 : 1);
            _currentHealth = maxHealth;
        }

        public PlayerData GetData() => Resources.Load<CD_Player>("Data/CD_Player").Data;

        #region Event Subscription

        private void SubscribeEvents()
        {
            CoreGameSignals.Instance.onPlay += OnPlay;
            CoreGameSignals.Instance.onLevelSuccessful += OnLevelSuccessful;
            CoreGameSignals.Instance.onLevelFailed += OnLevelFailed;
            CoreGameSignals.Instance.onLevelFailed += physicsController.OnLevelFailed;
            CoreGameSignals.Instance.onRestartLevel += OnResetLevel;
            CoreGameSignals.Instance.onPlay += physicsController.OnPlay;
            MissileSignals.Instance.onPinkMissileDestroyed += OnPinkMissileDestroyed;
        }

        #endregion

        public void TakeDamage(int damage = 1)
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            if (Time.time - _lastDamageTime < 0.15f)
            {
                return;
            }
            _lastDamageTime = Time.time;

            _currentHealth -= damage;
            if (_currentHealth <= 0)
            {
                Explode();
            }
            else
            {
                OnArmorBroken();
            }
        }

        private void OnArmorBroken()
        {
            // 1. Play metallic deflection sound
            AudioSignals.Instance.onPlaySound(AudioSoundEnums.Pitch);

            // 2. Trigger micro hit-stop & camera shake via signal
            MissileSignals.Instance.onMissileArmorHit?.Invoke();

            // 3. Physical flinch / recoil
            transform.DOPunchScale(Vector3.one * 0.25f, 0.2f, 10, 1).SetUpdate(true);

            // 4. Visual flash to indicate armor crack / damaged state
            if (_meshRenderer != null && _meshRenderer.material != null)
            {
                _meshRenderer.material.DOColor(new Color(1f, 0.35f, 0.1f), 0.1f)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetUpdate(true);
            }
        }

        public void Explode(bool isLevelEnd = false)
        {
            float shakeStrength = _missileData != null ? _missileData.ShakeStrength : 0.25f;
            if (!isLevelEnd)
            {
                MissileSignals.Instance.onMissileDestroyed?.Invoke(shakeStrength);
            }

            GameObject particle = PoolSignals.Instance.onGetObject?.Invoke(ParticleType);
            if (particle != null)
            {
                particle.transform.position = transform.position;
                particle.gameObject.SetActive(true);
            }

            if (IsPink)
            {
                if (!isLevelEnd)
                {
                    MissileSignals.Instance.onPinkMissileDestroyed?.Invoke();
                }
                AudioSignals.Instance.onPlaySound(AudioSoundEnums.Explosion1);
            }
            else
            {
                AudioSignals.Instance.onPlaySound(AudioSoundEnums.Explosion2);
            }

            if (isCluster && !isLevelEnd)
            {
                Vector3 spawnPos = transform.position;
                if (clusterSpawnDelay > 0f)
                {
                    _clusterDelayedCall = DOVirtual.DelayedCall(clusterSpawnDelay, () =>
                    {
                        SpawnClusterChildren(spawnPos);
                    });
                }
                else
                {
                    SpawnClusterChildren(spawnPos);
                }
            }

            gameObject.SetActive(false);
        }

        private void SpawnClusterChildren(Vector3 centerPos)
        {
            float halfSpacing = clusterChildSpacing * 1f;
            for (int i = 0; i < clusterChildCount; i++)
            {
                GameObject child = PoolSignals.Instance.onGetObject?.Invoke(clusterChildType);
                if (child != null)
                {
                    float yOffset = (i == 0) ? halfSpacing : -halfSpacing;
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

        private void OnPlay()
        {

        }

        private void OnPinkMissileDestroyed()
        {
            if (IsPink || !gameObject.activeInHierarchy)
            {
                return;
            }
            Explode();
        }

        private void OnLevelFailed()
        {
            _clusterDelayedCall?.Kill();
        }

        private void OnLevelSuccessful()
        {
            _clusterDelayedCall?.Kill();
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            Explode(isLevelEnd: true);
        }

        private void OnResetLevel()
        {
            _clusterDelayedCall?.Kill();
        }

        private void OnDestroy()
        {
            _clusterDelayedCall?.Kill();
        }
    }
}