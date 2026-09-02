using Data.ValueObject;
using Managers;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Signals;

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
        #endregion
        #endregion

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _data = manager.GetData();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Missile"))
            {
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
                transform.parent.parent.localPosition = _data.BoomerangHandLocalPosition;
                transform.parent.parent.localEulerAngles = _data.BoomerangHandLocalEulerAngles;
            }
        }
    }
}