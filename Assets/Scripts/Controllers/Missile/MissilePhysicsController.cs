using Data.ValueObject;
using Managers;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Signals;
using Enums;

namespace Controllers
{
    public class MissilePhysicsController : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables
        [SerializeField] private MissileManager manager;
        #endregion
        #region Private Variables

        private bool _isFailed = false;
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
                manager.Explode();

                transform.parent.gameObject.SetActive(false);
            }
            else if (other.CompareTag("SafeArea"))
            {
                manager.Explode();
                transform.parent.gameObject.SetActive(false);
                if (!_isFailed)
                {
                    CoreGameSignals.Instance.onLevelFailed?.Invoke();
                    AudioSignals.Instance.onPlaySound(AudioSoundEnums.Loose);
                }
                _isFailed = true;
            }
            else if (other.CompareTag("Missile"))
            {
                GameObject particle = PoolSignals.Instance.onGetObject?.Invoke(manager.ParticleType);
                particle.transform.position = transform.position;
                particle.gameObject.SetActive(true);

                transform.parent.gameObject.SetActive(false);
                MissileSignals.Instance.onMissileDestroyed?.Invoke();
            }
        }
        public void OnLevelFailed()
        {
            _isFailed = true;
        }

        public void OnRestartLevel()
        {
            _isFailed = false;
        }
    }
}