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
            Move();
        }



        private void Move()
        {
            if (_isNotStarted)
            {
                return;
            }

            _rig.velocity = new Vector3(_rig.velocity.x, _rig.velocity.y, _data.Speed);
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
            _rig.useGravity = true;


        }
        public void OnLevelFailed()
        {
            _rig.angularVelocity = Vector3.zero;
            _rig.velocity = Vector3.zero;
            _rig.useGravity = false;

        }
        public void OnLevelSuccess()
        {
            _rig.angularVelocity = Vector3.zero;
            _rig.velocity = Vector3.zero;
            _rig.useGravity = false;
        }
        public void OnRestartLevel()
        {
            _isNotStarted = true;
            _rig.angularVelocity = Vector3.zero;
            _rig.velocity = Vector3.zero;
            //_isNotStarted = true;
            transform.position = new Vector3(_data.InitializePosX, _data.InitializePosY);
            transform.eulerAngles = Vector3.zero;
        }
    }
}