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
            InputSignals.Instance.onClicking += OnAddPoint;
            InputSignals.Instance.onInputReleased += OnInputRelease;
            PlayerSignals.Instance.onBoomerangNextTarget += OnBoomerangNextTarget;
            PlayerSignals.Instance.onBoomerangNextTarget += _movementController.OnBoomerangNextTarget;
            PlayerSignals.Instance.onBoomerangHasReturned += OnBoomerangReturned;
            PlayerSignals.Instance.onBoomerangHasReturned += _movementController.OnBoomerangHasReturned;
            CoreGameSignals.Instance.onRestartLevel += OnResetLevel;
        }

        private void UnsubscribeEvents()
        {
            CoreGameSignals.Instance.onPlay -= OnPlay;
            InputSignals.Instance.onClicking -= OnAddPoint;
            InputSignals.Instance.onInputReleased -= OnInputRelease;
            PlayerSignals.Instance.onBoomerangNextTarget -= OnBoomerangNextTarget;
            PlayerSignals.Instance.onBoomerangNextTarget -= _movementController.OnBoomerangNextTarget;
            PlayerSignals.Instance.onBoomerangHasReturned -= OnBoomerangReturned;
            PlayerSignals.Instance.onBoomerangHasReturned -= _movementController.OnBoomerangHasReturned;
            CoreGameSignals.Instance.onRestartLevel -= OnResetLevel;
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
            PlayerSignals.Instance.onBoomerangThrowed?.Invoke();
            _movementController.Throwed();
        }
        private void OnBecameInvisible()
        {
            PlayerSignals.Instance.onBoomerangBecomeInvisible?.Invoke();
            Debug.Log("invisible");
        }
        private void OnResetLevel()
        {

        }
    }
}