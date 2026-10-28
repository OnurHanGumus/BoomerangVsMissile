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
        public bool IsDead => _isDead || !gameObject.activeInHierarchy;
        public MissileData MissileData => _missileData;
        public MissileHitContext LastHitContext => _lastHitContext;

        #endregion

        #region Serialized Variables
        [SerializeField] private MissileMovementController movementController;
        [SerializeField] private MissilePhysicsController physicsController;
        [SerializeField] private MissileLightController lightController;
        [SerializeField] private CD_Missile cdMissile;

        [Header("Legacy Settings (Auto-migrated if abilities not attached)")]
        [SerializeField] private bool isExploder = false;
        #endregion

        #region Private Variables
        private PlayerData _data;
        private MissileData _missileData;
        private MissileHitContext _lastHitContext;
        private bool _isDead = false;
        private Collider[] _colliders;

        private IMissileAbility[] _abilities;
        private IMissileDeathEffect[] _deathEffects;
        private IMissileDamageFilter[] _damageFilters;
        private IMissileDamageHandler[] _damageHandlers;
        private IMissileHealthProvider[] _healthProviders;

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
            _lastHitContext = default;
            if (physicsController != null)
            {
                physicsController.ResetHealth(GetMaxHealth());
            }
            EnableColliders(true);
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
        }

        private void InitAbilities()
        {
            // Auto-migrate legacy serialized flags if specialized component is missing
            if (isExploder && GetComponent<ExploderMissileAbility>() == null)
            {
                gameObject.AddComponent<ExploderMissileAbility>();
            }

            _abilities = GetComponents<IMissileAbility>();
            _deathEffects = GetComponents<IMissileDeathEffect>();
            _damageFilters = GetComponents<IMissileDamageFilter>();
            _damageHandlers = GetComponents<IMissileDamageHandler>();
            _healthProviders = GetComponents<IMissileHealthProvider>();


            if (_abilities != null)
            {
                for (int i = 0; i < _abilities.Length; i++)
                {
                    _abilities[i].Initialize(this);
                }
            }
        }

        public int GetMaxHealth()
        {
            int baseHealth = _missileData != null ? _missileData.MaxHealth : 1;
            int additionalHealth = 0;
            if (_healthProviders != null)
            {
                for (int i = 0; i < _healthProviders.Length; i++)
                {
                    if (_healthProviders[i] != null)
                    {
                        additionalHealth += _healthProviders[i].GetAdditionalHealth();
                    }
                }
            }
            return Mathf.Max(1, baseHealth + additionalHealth);
        }

        public bool FilterHit(ref MissileHitContext ctx)
        {
            _lastHitContext = ctx;
            if (_damageFilters != null)
            {
                for (int i = 0; i < _damageFilters.Length; i++)
                {
                    if (_damageFilters[i] != null && !_damageFilters[i].FilterHit(ref ctx))
                    {
                        _lastHitContext = ctx;
                        return false;
                    }
                }
            }
            _lastHitContext = ctx;
            return true;
        }

        public void NotifyDamageTaken(int damage, int currentHealth)
        {
            if (_damageHandlers != null)
            {
                for (int i = 0; i < _damageHandlers.Length; i++)
                {
                    if (_damageHandlers[i] != null)
                    {
                        _damageHandlers[i].OnDamageTaken(damage, currentHealth);
                    }
                }
            }
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

            if (!isLevelEnd)
            {
                PitchSignals.Instance.onPlayPitch?.Invoke(PitchEnums.Explosion);
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
