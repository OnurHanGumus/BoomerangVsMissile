using System;
using System.Collections.Generic;
using Commands;
using Controllers;
using Controllers.Missile.Abilities;
using Data.UnityObject;
using Data.ValueObject;
using DG.Tweening;
using Enums;
using Interfaces;
using Signals;
using UnityEngine;

namespace Managers
{
    public class MissileManager : MonoBehaviour
    {
        #region Self Variables

        #region Public Variables
        public PoolEnums ParticleType;
        public bool IsCluster => _clusterAbility != null || isCluster;
        public int ClusterChildCount => _clusterAbility != null ? _clusterAbility.ChildCount : (isCluster ? clusterChildCount : 0);
        public int CurrentHealth => _currentHealth;
        public bool IsDead => _isDead || !gameObject.activeInHierarchy;

        #endregion

        #region Serialized Variables
        [SerializeField] private MissilePhysicsController physicsController;
        [SerializeField] private MissileLightController lightController;
        [SerializeField] private CD_Missile cdMissile;

        [Header("Legacy Settings (Auto-migrated if abilities not attached)")]
        [SerializeField] private bool isBoss = false;
        [SerializeField] private bool isArmored = false;
        [SerializeField] private bool isCluster = false;
        [SerializeField] private PoolEnums clusterChildType = PoolEnums.Missile6;
        [SerializeField] private int clusterChildCount = 1;
        [SerializeField] private float clusterChildSpacing = 0.8f;
        [SerializeField] private float clusterSpawnDelay = 0f;
        #endregion

        #region Private Variables
        private PlayerData _data;
        private MissileData _missileData;
        private int _currentHealth = 1;
        private float _lastDamageTime = -1f;
        private bool _isDead = false;
        private Collider[] _colliders;

        private IMissileAbility[] _abilities;
        private IMissileDeathEffect[] _deathEffects;
        private IMissileDamageHandler[] _damageHandlers;
        private IMissileHealthProvider[] _healthProviders;

        private ClusterMissileAbility _clusterAbility;
        private bool _hasBossAbility;
        private bool _hasArmoredAbility;
        #endregion

        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }

        private void OnEnable()
        {
            _isDead = false;
            EnableColliders(true);
            ResetHealth();
        }

        private void OnDisable()
        {
            EnableColliders(false);
        }

        private void Init()
        {
            _data = GetData();
            if (cdMissile == null)
            {
                cdMissile = Resources.Load<CD_Missile>("Data/CD_Missile");
            }
            _missileData = cdMissile != null ? cdMissile.Data : new MissileData();

            _colliders = GetComponentsInChildren<Collider>(true);
            InitAbilities();
            ResetHealth();
        }

        private void InitAbilities()
        {
            // Auto-migrate legacy serialized flags if specialized component is missing
            if (isBoss && GetComponent<BossMissileAbility>() == null)
            {
                gameObject.AddComponent<BossMissileAbility>();
            }
            if (isArmored && GetComponent<ArmoredMissileAbility>() == null)
            {
                gameObject.AddComponent<ArmoredMissileAbility>();
            }
            if (isCluster && GetComponent<ClusterMissileAbility>() == null)
            {
                var cluster = gameObject.AddComponent<ClusterMissileAbility>();
                cluster.Configure(clusterChildType, clusterChildCount, clusterChildSpacing, clusterSpawnDelay);
            }

            _abilities = GetComponents<IMissileAbility>();
            _deathEffects = GetComponents<IMissileDeathEffect>();
            _damageHandlers = GetComponents<IMissileDamageHandler>();
            _healthProviders = GetComponents<IMissileHealthProvider>();

            _clusterAbility = GetComponent<ClusterMissileAbility>();
            _hasBossAbility = GetComponent<BossMissileAbility>() != null;
            _hasArmoredAbility = GetComponent<ArmoredMissileAbility>() != null;

            if (_abilities != null)
            {
                for (int i = 0; i < _abilities.Length; i++)
                {
                    _abilities[i].Initialize(this);
                }
            }
        }

        public void ResetHealth()
        {
            int baseHealth = _missileData != null && _missileData.MaxHealth > 0
                ? _missileData.MaxHealth
                : 1;

            int extraHealth = 0;
            if (_healthProviders != null && _healthProviders.Length > 0)
            {
                for (int i = 0; i < _healthProviders.Length; i++)
                {
                    extraHealth += _healthProviders[i].GetAdditionalHealth();
                }
            }
            else if (isArmored || (_missileData != null && _missileData.IsArmored))
            {
                extraHealth = 1;
            }

            _currentHealth = baseHealth + extraHealth;
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
                if (_damageHandlers != null)
                {
                    for (int i = 0; i < _damageHandlers.Length; i++)
                    {
                        _damageHandlers[i].OnDamageTaken(damage, _currentHealth);
                    }
                }
            }
        }

        public void Explode(bool isLevelEnd = false)
        {
            if (_isDead || !gameObject.activeInHierarchy)
            {
                return;
            }
            _isDead = true;
            EnableColliders(false);

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

            bool customAudioPlayed = false;
            if (_deathEffects != null)
            {
                for (int i = 0; i < _deathEffects.Length; i++)
                {
                    if (_deathEffects[i].OnMissileDeath(transform.position, isLevelEnd))
                    {
                        customAudioPlayed = true;
                    }
                }
            }

            if (!customAudioPlayed)
            {
                AudioSignals.Instance.onPlaySound(AudioSoundEnums.Explosion2);
            }

            gameObject.SetActive(false);
        }

        private void EnableColliders(bool enable)
        {
            if (_colliders == null)
            {
                _colliders = GetComponentsInChildren<Collider>(true);
            }
            for (int i = 0; i < _colliders.Length; i++)
            {
                if (_colliders[i] != null)
                {
                    _colliders[i].enabled = enable;
                }
            }
        }

        private void OnPlay()
        {

        }

        private void OnLevelFailed()
        {
            _clusterAbility?.CancelDelayedSpawn();
        }

        private void OnLevelSuccessful()
        {
            _clusterAbility?.CancelDelayedSpawn();
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            Explode(isLevelEnd: true);
        }

        private void OnResetLevel()
        {
            _clusterAbility?.CancelDelayedSpawn();
        }

        private void OnDestroy()
        {
            _clusterAbility?.CancelDelayedSpawn();
        }
    }
}