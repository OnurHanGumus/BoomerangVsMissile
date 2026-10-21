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
        private bool _isFailed = false;
        private float _lastDamageTime = -1f;
        #endregion
        #endregion

        private void Awake()
        {
            Init();
        }

        private void Init()
        {

        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Boomerang"))
            {
                var boomerangManager = other.GetComponentInParent<BoomerangManager>();
                bool isFullyCharged = boomerangManager != null && boomerangManager.IsFullyCharged;
                int baseDamage = boomerangManager != null ? boomerangManager.GetBoomerangData().Damage : 1;
                TakeDamage(isFullyCharged ? baseDamage * 2 : baseDamage);
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
        }

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

            CurrentHealth -= damage;
            if (CurrentHealth <= 0)
            {
                manager.Explode();
            }
            else
            {
                OnHitted?.Invoke();
            }
        }


    }
}