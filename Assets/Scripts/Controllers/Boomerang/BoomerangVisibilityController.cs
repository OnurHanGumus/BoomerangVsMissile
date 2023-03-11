using Data.ValueObject;
using Managers;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Signals;

namespace Controllers
{
    public class BoomerangVisibilityController : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables
        [SerializeField] private BoomerangManager manager;
        #endregion
        #region Private Variables
        private bool _isLevelSuccessful = false;
        #endregion
        #endregion

        private void Awake()
        {
            Init();
        }

        private void Init()
        {

        }
        
        private void OnBecameInvisible()
        {
            if (_isLevelSuccessful)
            {
                return;
            }

            manager.IsDisapeared = true;
            BoomerangSignals.Instance.onBoomerangDisapeared?.Invoke();
        }
        public void OnLevelSuccessful()
        {
            _isLevelSuccessful = true;
        }
        public void OnRestartLevel()
        {
            _isLevelSuccessful = false;
        }
    }
}