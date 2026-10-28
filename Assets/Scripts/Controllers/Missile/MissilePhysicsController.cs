using Data.ValueObject;
using Managers;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Signals;
using Enums;
using UnityEngine.Events;

namespace Controllers
{
    public class MissilePhysicsController : MonoBehaviour
    {
        #region Self Variables
        public int CurrentHealth = 1;
        public UnityAction OnHitted = delegate { };

        #region Serialized Variables
        [SerializeField] private MissileManager manager;
        #endregion

        #region Private Variables
        private const float HitCooldown = 0.30f;

        private bool _isFailed = false;
        private float _lastDamageTime = -1f;
        private Collider _selfCollider;
        private bool _waitingForUnshieldedStay = false;
        #endregion
        #endregion

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _selfCollider = GetComponent<Collider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Boomerang"))
            {
                Vector3 touchPoint = GetTouchPointOnSelf(other.bounds.center);
                if (IsTouchPointShielded(touchPoint))
                {
                    _waitingForUnshieldedStay = true;
                    return;
                }

                _waitingForUnshieldedStay = false;
                ApplyBoomerangHit(other);
            }
            else if (other.CompareTag("SafeArea"))
            {
                //transform.parent.gameObject.SetActive(false);
                if (!_isFailed)
                {
                    CoreGameSignals.Instance.onLevelFailed?.Invoke();
                    AudioSignals.Instance.onPlaySound(AudioSoundEnums.Lose);
                }
                _isFailed = true;
                manager.Explode();
            }
            else if (other.CompareTag("Missile"))
            {
                manager.Explode();
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (!_waitingForUnshieldedStay || !other.CompareTag("Boomerang"))
            {
                return;
            }

            Vector3 touchPoint = GetTouchPointOnSelf(other.bounds.center);
            if (!IsTouchPointShielded(touchPoint))
            {
                _waitingForUnshieldedStay = false;
                ApplyBoomerangHit(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_waitingForUnshieldedStay && other.CompareTag("Boomerang"))
            {
                _waitingForUnshieldedStay = false;
            }
        }

        private void ApplyBoomerangHit(Collider boomerangCollider)
        {
            var boomerangManager = boomerangCollider.GetComponentInParent<BoomerangManager>();
            if (boomerangManager != null && !boomerangManager.IsThrown)
            {
                return;
            }

            if (!CanAcceptHit())
            {
                return;
            }

            bool isFullyCharged = boomerangManager != null && boomerangManager.IsFullyCharged;
            int baseDamage = boomerangManager != null ? boomerangManager.EffectiveDamage : 1;
            MissileHitContext hitContext = new MissileHitContext
            {
                Damage = isFullyCharged ? baseDamage * 2 : baseDamage,
                IsFullyCharged = isFullyCharged
            };

            _lastDamageTime = Time.time;
            BoomerangSignals.Instance.onBoomerangNextTarget?.Invoke();
            ProcessAcceptedHit(hitContext);
        }

        private Vector3 GetTouchPointOnSelf(Vector3 boomerangCenter)
        {
            if (_selfCollider == null)
            {
                _selfCollider = GetComponent<Collider>();
            }

            if (_selfCollider is BoxCollider box)
            {
                Vector3 localPoint = box.transform.InverseTransformPoint(boomerangCenter) - box.center;
                Vector3 halfExtents = box.size * 0.5f;
                Vector3 clampedLocal = new Vector3(
                    Mathf.Clamp(localPoint.x, -halfExtents.x, halfExtents.x),
                    Mathf.Clamp(localPoint.y, -halfExtents.y, halfExtents.y),
                    Mathf.Clamp(localPoint.z, -halfExtents.z, halfExtents.z)
                );
                return box.transform.TransformPoint(box.center + clampedLocal);
            }

            if (_selfCollider != null && _selfCollider.enabled && gameObject.activeInHierarchy)
            {
                return _selfCollider.ClosestPoint(boomerangCenter);
            }

            return transform.position;
        }

        private bool IsTouchPointShielded(Vector3 touchPoint)
        {
            return MissileSignals.Instance != null && MissileSignals.Instance.onIsPointShielded(touchPoint, transform);
        }

        public void OnPlay()
        {
            
        }

        public void OnLevelFailed()
        {
            _isFailed = true;
        }

        private void OnDisable()
        {
            _isFailed = false;
            _waitingForUnshieldedStay = false;
        }

        public void ResetHealth(int maxHealth)
        {
            CurrentHealth = Mathf.Max(1, maxHealth);
            _lastDamageTime = -1f;
            _waitingForUnshieldedStay = false;
        }

        public void TakeDamage(int damage = 1)
        {
            TakeHit(new MissileHitContext
            {
                Damage = damage,
                IsFullyCharged = false
            });
        }

        public void TakeHit(MissileHitContext ctx)
        {
            if (!CanAcceptHit())
            {
                return;
            }
            _lastDamageTime = Time.time;
            ProcessAcceptedHit(ctx);
        }

        private bool CanAcceptHit()
        {
            if (!gameObject.activeInHierarchy || (manager != null && manager.IsDead))
            {
                return false;
            }

            if (_lastDamageTime >= 0f && Time.time - _lastDamageTime < HitCooldown)
            {
                return false;
            }

            return true;
        }

        private void ProcessAcceptedHit(MissileHitContext ctx)
        {
            if (manager != null && !manager.FilterHit(ref ctx))
            {
                return;
            }

            CurrentHealth -= ctx.Damage;
            if (CurrentHealth <= 0)
            {
                manager.Explode();
            }
            else
            {
                if (manager != null)
                {
                    manager.NotifyDamageTaken(ctx.Damage, CurrentHealth);
                }
                OnHitted?.Invoke();
            }
        }
    }
}
