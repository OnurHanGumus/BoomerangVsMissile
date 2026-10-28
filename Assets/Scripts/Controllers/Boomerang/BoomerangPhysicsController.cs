using System.Collections.Generic;
using Data.ValueObject;
using DG.Tweening;
using Managers;
using Signals;
using UnityEngine;

namespace Controllers
{
    public class BoomerangPhysicsController : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables
        [SerializeField] private BoomerangManager manager;
        #endregion

        #region Private Variables
        private PlayerData _data;
        private Collider _selfCollider;
        private readonly HashSet<Collider> _deferredShieldedColliders = new HashSet<Collider>();
        #endregion

        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }

        private void Init()
        {
            _data = manager.GetData();
            _selfCollider = GetComponent<Collider>();
        }

        #region Event Subscription
        private void SubscribeEvents()
        {
            if (BoomerangSignals.Instance != null)
            {
                BoomerangSignals.Instance.onBoomerangHasReturned += ClearDeferredColliders;
            }
            if (CoreGameSignals.Instance != null)
            {
                CoreGameSignals.Instance.onReset += ClearDeferredColliders;
            }
        }

        private void UnsubscribeEvents()
        {
            if (BoomerangSignals.Instance != null)
            {
                BoomerangSignals.Instance.onBoomerangHasReturned -= ClearDeferredColliders;
            }
            if (CoreGameSignals.Instance != null)
            {
                CoreGameSignals.Instance.onReset -= ClearDeferredColliders;
            }
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }
        #endregion

        private void OnDisable()
        {
            ClearDeferredColliders();
        }

        private void ClearDeferredColliders()
        {
            _deferredShieldedColliders.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Missile"))
            {
                Vector3 touchPoint = GetTouchPointOnCollider(other);
                if (IsTouchPointShielded(touchPoint, other.transform))
                {
                    _deferredShieldedColliders.Add(other);
                    return;
                }

                _deferredShieldedColliders.Remove(other);
                BoomerangSignals.Instance.onBoomerangNextTarget?.Invoke();
            }
            else if (other.CompareTag("CatchArea"))
            {
                if (manager.IsRising)
                {
                    return;
                }
                BoomerangSignals.Instance.onBoomerangHasReturned?.Invoke();
                transform.parent.parent.position = other.transform.position;
            }
            else if (other.CompareTag("BoomerangHand"))
            {
                transform.parent.parent.parent = other.transform;
                transform.parent.parent.localPosition = manager != null
                    ? manager.EffectiveHandLocalPosition
                    : _data.BoomerangHandLocalPosition;
                transform.parent.parent.localEulerAngles = manager != null
                    ? manager.EffectiveHandLocalEulerAngles
                    : _data.BoomerangHandLocalEulerAngles;
                BoomerangSignals.Instance.onPlayerTouchedBoomerang?.Invoke();
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (_deferredShieldedColliders.Count == 0 || !_deferredShieldedColliders.Contains(other))
            {
                return;
            }

            if (other == null || !other.CompareTag("Missile"))
            {
                _deferredShieldedColliders.Remove(other);
                return;
            }

            Vector3 touchPoint = GetTouchPointOnCollider(other);
            if (!IsTouchPointShielded(touchPoint, other.transform))
            {
                _deferredShieldedColliders.Remove(other);
                BoomerangSignals.Instance.onBoomerangNextTarget?.Invoke();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_deferredShieldedColliders.Count > 0)
            {
                _deferredShieldedColliders.Remove(other);
            }
        }

        private Vector3 GetTouchPointOnCollider(Collider targetCollider)
        {
            Vector3 boomerangCenter = _selfCollider != null ? _selfCollider.bounds.center : transform.position;
            if (targetCollider is BoxCollider box)
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

            if (targetCollider.enabled && targetCollider.gameObject.activeInHierarchy)
            {
                return targetCollider.ClosestPoint(boomerangCenter);
            }

            return targetCollider.transform.position;
        }

        private static bool IsTouchPointShielded(Vector3 touchPoint, Transform targetTransform)
        {
            return MissileSignals.Instance != null && MissileSignals.Instance.onIsPointShielded(touchPoint, targetTransform);
        }
    }
}
