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
        public int PointIndex = 0;
        public bool IsRight = true;
        public bool IsThrown = false;
        public bool IsRising = false;
        public bool IsDisappeared = false;


        #endregion

        #region Serialized Variables
        [SerializeField] private BoomerangMeshController meshController;
        [SerializeField] private BoomerangPhysicsController physicsController;

        #endregion

        #region Private Variables
        private PlayerData _data;
        private BoomerangMovementController _movementController;

        #endregion

        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }

        private void Init()
        {
            _data = GetData();
            _movementController = GetComponent<BoomerangMovementController>();
        }

        public PlayerData GetData() => Resources.Load<CD_Player>("Data/CD_Player").Data;

        #region Event Subscription

        private void SubscribeEvents()
        {
            CoreGameSignals.Instance.onPlay += OnPlay;
            CoreGameSignals.Instance.onPlay += _movementController.OnPlay;
            CoreGameSignals.Instance.onPlay += meshController.OnPlay;
            CoreGameSignals.Instance.onLevelSuccessful += physicsController.OnLevelEnded;
            CoreGameSignals.Instance.onLevelFailed += physicsController.OnLevelEnded;
            CoreGameSignals.Instance.onLevelSuccessful += meshController.OnLevelSuccessful;
            CoreGameSignals.Instance.onLevelFailed += OnLevelFailed;
            CoreGameSignals.Instance.onRestartLevel += OnRestartLevel;
            CoreGameSignals.Instance.onRestartLevel += _movementController.OnRestartLevel;
            CoreGameSignals.Instance.onRestartLevel += physicsController.OnRestartLevel;
            InputSignals.Instance.onClicking += OnAddPoint;
            InputSignals.Instance.onInputReleased += OnInputRelease;
            BoomerangSignals.Instance.onBoomerangNextTarget += OnBoomerangNextTarget;
            BoomerangSignals.Instance.onBoomerangNextTarget += _movementController.OnBoomerangNextTarget;
            BoomerangSignals.Instance.onBoomerangReturning += OnBoomerangReturning;
            BoomerangSignals.Instance.onBoomerangHasReturned += OnBoomerangReturned;
            BoomerangSignals.Instance.onBoomerangHasReturned += _movementController.OnBoomerangHasReturned;
            BoomerangSignals.Instance.onBoomerangRebuilt += _movementController.OnBoomerangRebuilt;
            BoomerangSignals.Instance.onBoomerangRebuilt += OnBoomerangRebuilt;
            BoomerangSignals.Instance.onSelectBoomerang += meshController.OnSelectBoomerang;
        }

        #endregion

        private void OnLevelFailed()
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }

        private void OnPlay()
        {
            transform.GetChild(0).gameObject.SetActive(true);
            transform.position = new Vector3(_data.BoomerangInitPosX, _data.BoomerangInitPosY, 0);
        }

        public void OnBoomerangNextTarget()
        {
            ++PointIndex;
            IsRight = !IsRight;
        }

        private void OnBoomerangReturning()
        {
            if (PointIndex > 1)
            {
                BoomerangSignals.Instance.onCombo?.Invoke(PointIndex - 2);
            }
        }

        private void OnBoomerangReturned()
        {
            PointIndex = 0;
            MissilePoints.Clear();
        }

        private void OnAddPoint(Vector3 pos)
        {
            MissilePoints.Add(pos);
        }

        private void OnInputRelease()
        {
            if (MissilePoints.Count <= 0 || IsThrown)
            {
                return;
            }
            transform.parent = null;
            transform.localEulerAngles = Vector3.zero;
            BoomerangSignals.Instance.onBoomerangThrown?.Invoke();
            _movementController.Thrown();
        }
        
        private void OnBoomerangRebuilt()
        {
            IsDisappeared = false;
            IsRising = false;
            IsThrown = false;
            PointIndex = 0;
            MissilePoints.Clear();
        }

        private void OnRestartLevel()
        {
            transform.parent = null;
            IsRising = false;
            IsDisappeared = false;
            MissilePoints.Clear();
            PointIndex = 0;
        }
    }
}