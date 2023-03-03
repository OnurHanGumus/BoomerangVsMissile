using Data.ValueObject;
using Managers;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Signals;

namespace Controllers
{
    public class BoomerangMovementController : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables

        #endregion
        #region Private Variables
        private Rigidbody _rig;
        private BoomerangManager _manager;
        private PlayerData _data;

        private bool _isNotStarted = true;
        public bool _isPointMissed = false;
        private Vector3 _initializePos = new Vector3(0,-4,0);
        private Vector3 _currentDir;


        #endregion
        #endregion

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _rig = GetComponent<Rigidbody>();
            _manager = GetComponent<BoomerangManager>();
            _data = _manager.GetData();
        }


        private void FixedUpdate()
        {
            if (!_manager.IsThrowed)
            {
                return;
            }

            Move();
            Spin();
        }
        public void Move()
        {
            if (_isPointMissed)//Gecikmekli bir þekilde yeni boomerang gelir. O geldiðinde bu deðer tekrar false olur.
            {
                return;
            }
            if (_manager.MissilePoints.Count <= 0)
            {
                return;
            }
            if (!_manager.IsThrowed)
            {
                return;
            }

            _rig.velocity = _currentDir;

            if (Mathf.Abs((_manager.MissilePoints[_manager.PointIndeks] - transform.position).sqrMagnitude) <= new Vector3(0.1f, 0.1f, 0.1f).sqrMagnitude)
            {
                _isPointMissed = true;
            }

        }

        private Vector3 GetDirection()
        {
            Vector3 dir = (_manager.MissilePoints[_manager.PointIndeks] - transform.position).normalized * _data.Speed * (_manager.PointIndeks + 1);
            return dir;
        }
        private void Spin()
        {
            //_rig.AddRelativeTorque(new Vector3(0, 0, _data.AngularSpeed * (_manager.IsRight ? 1 : -1)), ForceMode.Force);
            _rig.angularVelocity = new Vector3(0, 0, _data.AngularSpeed * (_manager.IsRight ? 1 : -1));
            _rig.maxAngularVelocity = 50;
        }
        public void Throwed()
        {
            _manager.IsRising = true; ;

            _manager.MissilePoints.Add(_initializePos);
            _currentDir = GetDirection();
            _manager.IsThrowed = true;
        }


        public void OnPlay()
        {
            _rig.velocity = Vector3.zero;
            _rig.angularVelocity = Vector3.zero;
            transform.parent = null;
            //transform.position = _initializePos;
        }

        

        public void OnBoomerangHasReturned()
        {
            _manager.IsThrowed = false;
            _isPointMissed = false;
            _rig.velocity = Vector3.zero;
            _rig.angularVelocity = Vector3.zero;
        }

        public void OnBoomerangNextTarget()
        {
            _currentDir = GetDirection();

            if (_isPointMissed)
            {
                _manager.PointIndeks = _manager.MissilePoints.Count - 1;
            }
            _isPointMissed = false;

            if (_manager.PointIndeks == _manager.MissilePoints.Count - 1)
            {
                _manager.IsRising = false;
            }
            if (_manager.PointIndeks == _manager.MissilePoints.Count - 1)
            {
                BoomerangSignals.Instance.onBoomerangReturning?.Invoke();
            }
        }


        public void OnBoomerangRespawned()
        {
            _isPointMissed = false;
        }
        public void OnBoomerangRebuilded()
        {
            _rig.velocity = Vector3.zero;
            _rig.angularVelocity = Vector3.zero;
            transform.position = _initializePos;
            transform.eulerAngles = Vector3.zero;
            _isPointMissed = false;
        }

        public void OnLevelFailed()
        {


        }
        public void OnLevelSuccess()
        {

        }
        public void OnRestartLevel()
        {
            _manager.IsThrowed = false;
            _isNotStarted = true;
        }
    }
}