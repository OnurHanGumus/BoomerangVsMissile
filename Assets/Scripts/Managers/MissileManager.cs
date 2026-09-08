using System;
using System.Collections.Generic;
using Commands;
using Controllers;
using Controllers.Missile;
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
        public int CurrentHealth => _currentHealth;
        public bool IsDead => _isDead || !gameObject.activeInHierarchy;
        public MissileData MissileData => _missileData;

        #endregion

        #region Serialized Variables
        [SerializeField] private MissileMovementController movementController;
        [SerializeField] private MissilePhysicsController physicsController;
        [SerializeField] private MissileLightController lightController;
        [SerializeField] private CD_Missile cdMissile;

        [Header("Legacy Settings (Auto-migrated if abilities not attached)")]
        [SerializeField] private bool isExploder = false;
        [SerializeField] private bool isArmored = false;
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

        private bool _hasExploderAbility;
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
            if (movementController == null)
            {
                movementController = GetComponent<MissileMovementController>();
                if (movementController == null)
                {
                    movementController = gameObject.AddComponent<MissileMovementController>();
                }
            }
            InitAbilities();
            ResetHealth();
        }

        private void InitAbilities()
        {
            // Auto-migrate legacy serialized flags if specialized component is missing
            if (isExploder && GetComponent<ExploderMissileAbility>() == null)
            {
                gameObject.AddComponent<ExploderMissileAbility>();
            }
            if (isArmored && GetComponent<ArmoredMissileAbility>() == null)
            {
                gameObject.AddComponent<ArmoredMissileAbility>();
            }

            _abilities = GetComponents<IMissileAbility>();
            _deathEffects = GetComponents<IMissileDeathEffect>();
            _damageHandlers = GetComponents<IMissileDamageHandler>();
            _healthProviders = GetComponents<IMissileHealthProvider>();

            _hasExploderAbility = GetComponent<ExploderMissileAbility>() != null;
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
            CoreGameSignals.Instance.onLevelFailed += physicsController.OnLevelFailed;
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

        private void OnLevelSuccessful()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            Explode(isLevelEnd: true);
        }
    }
}