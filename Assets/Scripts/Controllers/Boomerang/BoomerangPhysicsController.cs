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
                BoomerangSignals.Instance.onBoomerangNextTarget?.Invoke();
            }
            else if (other.CompareTag("CatchArea"))
            {
                if (manager.IsRising)
                {
                    return;
                }
                BoomerangSignals.Instance.onBoomerangHasReturned?.Invoke();
                transform.parent.position = other.transform.position;
                
            }
            else if (other.CompareTag("BoomerangHand"))
            {
                transform.parent.parent = other.transform;
                transform.parent.localPosition = new Vector3(0.1360204f, 0.2610005f, -0.04199352f);
                transform.parent.localEulerAngles = new Vector3(-9.304f, -9.275f, -107.476f);
            }
        }
    }
}