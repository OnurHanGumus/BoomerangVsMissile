using System;
using System.Collections.Generic;
using Commands;
using Controllers;
using Data.UnityObject;
using Data.ValueObject;
using Enums;
using Signals;
using UnityEngine;

namespace Managers
{
    public class BoomerangManager : MonoBehaviour
    {
        #region Self Variables

        #region Public Variables
        public List<Vector3> MissilePoints;
        public int PointIndeks = 0;
        public bool IsRight = true;
        public bool IsThrowed = false;
        public bool IsRising = false;

        #endregion

        #region Serialized Variables
        #endregion

        #region Private Variables
        private PlayerData _data;
        private BoomerangMovementController _movementController;

        #endregion

        #endregion

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _data = GetData();
            _movementController = GetComponent<BoomerangMovementController>();
        }
        public PlayerData GetData() => Resources.Load<CD_Player>("Data/CD_Player").Data;

        #region Event Subscription

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            CoreGameSignals.Instance.onPlay += OnPlay;
            CoreGameSignals.Instance.onPlay += _movementController.OnPlay;
            InputSignals.Instance.onClicking += OnAddPoint;
            InputSignals.Instance.onInputReleased += OnInputRelease;
            BoomerangSignals.Instance.onBoomerangNextTarget += OnBoomerangNextTarget;
            BoomerangSignals.Instance.onBoomerangNextTarget += _movementController.OnBoomerangNextTarget;
            BoomerangSignals.Instance.onBoomerangHasReturned += OnBoomerangReturned;
            BoomerangSignals.Instance.onBoomerangHasReturned += _movementController.OnBoomerangHasReturned;
            BoomerangSignals.Instance.onBoomerangRebuilded += _movementController.OnBoomerangRebuilded;
            CoreGameSignals.Instance.onRestartLevel += OnRestartLevel;
            CoreGameSignals.Instance.onRestartLevel += _movementController.OnRestartLevel;
        }

        private void UnsubscribeEvents()
        {
            CoreGameSignals.Instance.onPlay -= OnPlay;
            CoreGameSignals.Instance.onPlay -= _movementController.OnPlay;
            InputSignals.Instance.onClicking -= OnAddPoint;
            InputSignals.Instance.onInputReleased -= OnInputRelease;
            BoomerangSignals.Instance.onBoomerangNextTarget -= OnBoomerangNextTarget;
            BoomerangSignals.Instance.onBoomerangNextTarget -= _movementController.OnBoomerangNextTarget;
            BoomerangSignals.Instance.onBoomerangHasReturned -= OnBoomerangReturned;
            BoomerangSignals.Instance.onBoomerangHasReturned -= _movementController.OnBoomerangHasReturned;
            BoomerangSignals.Instance.onBoomerangRebuilded -= _movementController.OnBoomerangRebuilded;
            CoreGameSignals.Instance.onRestartLevel -= OnRestartLevel;
            CoreGameSignals.Instance.onRestartLevel -= _movementController.OnRestartLevel;
        }


        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        #endregion

        private void OnPlay()
        {

        }
        public void OnBoomerangNextTarget()
        {
            if (MissilePoints.Count == (PointIndeks + 1))
            {
                return;
            }
            ++PointIndeks;
            IsRight = !IsRight;
        }

        private void OnBoomerangReturned()
        {
            PointIndeks = 0;
            MissilePoints.Clear();
        }
        private void OnAddPoint(Vector3 pos)
        {
            MissilePoints.Add(pos);
        }
        private void OnInputRelease()
        {
            if (MissilePoints.Count <= 0 || IsThrowed)
            {
                return;
            }
            transform.parent = null;
            transform.localEulerAngles = Vector3.zero;
            BoomerangSignals.Instance.onBoomerangThrowed?.Invoke();
            _movementController.Throwed();
        }
        private void OnBecameInvisible()
        {
            BoomerangSignals.Instance.onBoomerangDisapeared?.Invoke();
        }

        private void OnRestartLevel()
        {
            transform.parent = null;
            IsRising = false;
            MissilePoints.Clear();
        }
    }
}