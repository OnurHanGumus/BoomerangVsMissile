using Data.ValueObject;
using Managers;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Controllers
{
    public class BoomerangMovementController : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables
        [SerializeField] private Vector3 initializePos;

        #endregion
        #region Private Variables
        private Rigidbody _rig;
        private BoomerangManager _manager;
        private PlayerData _data;

        private bool _isNotStarted = true;
        private bool _isThrowed = false;
        private bool _isPointMissed = false;

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
            initializePos = transform.position;
        }


        private void FixedUpdate()
        {
            if (!_isThrowed)
            {
                return;
            }
            Move();
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
            if (!_isThrowed)
            {
                return;
            }
            Vector3 dir = (_manager.MissilePoints[_manager.PointIndeks] - transform.position).normalized * 10;
            _rig.velocity = dir;

        }

        public void Throwed()
        {
            _manager.MissilePoints.Add(initializePos);
            _isThrowed = true;
        }


        public void OnPlay()
        {


        }

        

        public void OnBoomerangHasReturned()
        {
            _isThrowed = false;
            _rig.velocity = Vector3.zero;
        }
        public void OnLevelFailed()
        {


        }
        public void OnLevelSuccess()
        {

        }
        public void OnRestartLevel()
        {

        }
    }
}