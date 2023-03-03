using Data.ValueObject;
using Managers;
using UnityEngine;

namespace Controllers
{
    public class PlayerMovementController : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables
        #endregion
        #region Private Variables
        private Rigidbody _rig;
        private PlayerManager _manager;
        private PlayerData _data;

        private bool _isNotStarted = true;

        #endregion
        #endregion

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _rig = GetComponent<Rigidbody>();
            _manager = GetComponent<PlayerManager>();
            _data = _manager.GetData();
        }


        private void FixedUpdate()
        {

        }

        public void OnClicked()
        {

        }

        public void OnReleased()
        {

        }

        public void OnPlay()
        {
            _isNotStarted = false;
        }

        public void OnLevelFailed()
        {

        }

        public void OnLevelSuccess()
        {

        }
        public void OnRestartLevel()
        {
            _isNotStarted = true;
        }
    }
}