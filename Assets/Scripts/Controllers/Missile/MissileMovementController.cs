using Data.ValueObject;
using Managers;
using UnityEngine;

namespace Controllers.Missile
{
    [RequireComponent(typeof(Rigidbody))]
    public class MissileMovementController : MonoBehaviour
    {
        #region Serialized Variables
        [SerializeField] private MissileManager manager;
        #endregion

        #region Private Variables
        private Rigidbody _rigidbody;
        private bool _hasSpeedOverride = false;
        private float _speedOverride = 0f;
        #endregion

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }
            if (manager == null)
            {
                manager = GetComponent<MissileManager>();
            }
        }

        private void OnEnable()
        {
            Init();
            _hasSpeedOverride = false;
            ApplyMovement();
        }

        private void FixedUpdate()
        {
            if (manager != null && manager.IsDead)
            {
                return;
            }

            ApplyMovement();
        }

        private void ApplyMovement()
        {
            if (_rigidbody == null)
            {
                return;
            }

            float speed = GetCurrentSpeed();
            _rigidbody.linearVelocity = new Vector3(0, -speed, 0);
        }

        private void OnDisable()
        {
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
            }
        }

        public float GetCurrentSpeed()
        {
            if (_hasSpeedOverride)
            {
                return _speedOverride;
            }

            if (manager != null && manager.MissileData != null)
            {
                return manager.MissileData.Velocity;
            }

            return 2f;
        }

        public void SetSpeedOverride(float speed)
        {
            _hasSpeedOverride = true;
            _speedOverride = speed;
            ApplyMovement();
        }

        public void ClearSpeedOverride()
        {
            _hasSpeedOverride = false;
            ApplyMovement();
        }
    }
}
