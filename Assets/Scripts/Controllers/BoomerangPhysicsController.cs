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
            if (other.CompareTag("Missile"))
            {

                PlayerSignals.Instance.onBoomerangNextTarget?.Invoke();
            }
            else if (other.CompareTag("BoomerangHand"))
            {
                PlayerSignals.Instance.onBoomerangHasReturned?.Invoke();
            }
        }
    }
}