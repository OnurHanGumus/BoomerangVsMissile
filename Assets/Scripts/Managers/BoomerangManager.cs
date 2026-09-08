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
        public bool IsFullyCharged = false;

        public float EffectiveSpeed
        {
            get
            {
                float bonus = BoomerangUpgradeManager.Instance != null 
                    ? BoomerangUpgradeManager.Instance.GetTotalBonus(BoomerangUpgradeType.Speed) 
                    : 0f;
                return (_data != null ? _data.Speed : 5f) + bonus;
            }
        }

        public float EffectiveReturnSpeedMultiplier
        {
            get
            {
                float bonus = BoomerangUpgradeManager.Instance != null 
                    ? BoomerangUpgradeManager.Instance.GetTotalBonus(BoomerangUpgradeType.ReturnSpeed) 
                    : 0f;
                return (_data != null ? _data.ReturnSpeedMultiplier : 1.25f) + bonus;
            }
        }
        #endregion

        #region Serialized Variables
        [SerializeField] private BoomerangMeshController meshController;
        [SerializeField] private BoomerangPhysicsController physicsController;
        [SerializeField] private Controllers.Boomerang.TrajectoryPreviewController trajectoryPreviewController;

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
            if (trajectoryPreviewController == null)
            {
                trajectoryPreviewController = FindFirstObjectByType<Controllers.Boomerang.TrajectoryPreviewController>();
                if (trajectoryPreviewController == null)
                {
                    GameObject previewObj = new GameObject("TrajectoryPreviewController");
                    trajectoryPreviewController = previewObj.AddComponent<Controllers.Boomerang.TrajectoryPreviewController>();
                }
            }
        }

        public PlayerData GetData() => Resources.Load<CD_Player>("Data/CD_Player").Data;

        private void Start()
        {
            ApplyUpgrades();
        }

        #region Event Subscription

        private void SubscribeEvents()
        {
            CoreGameSignals.Instance.onPlay += OnPlay;
            CoreGameSignals.Instance.onPlay += _movementController.OnPlay;
            CoreGameSignals.Instance.onPlay += meshController.OnPlay;
            CoreGameSignals.Instance.onLevelSuccessful += meshController.OnLevelSuccessful;
            CoreGameSignals.Instance.onLevelSuccessful += OnLevelFailedOrSuccessful;
            CoreGameSignals.Instance.onLevelFailed += OnLevelFailedOrSuccessful;
            CoreGameSignals.Instance.onRestartLevel += OnRestartLevel;
            CoreGameSignals.Instance.onRestartLevel += _movementController.OnRestartLevel;
            InputSignals.Instance.onClicking += OnAddPoint;
            InputSignals.Instance.onInputReleased += OnInputRelease;
            BoomerangSignals.Instance.onBoomerangNextTarget += OnBoomerangNextTarget;
            BoomerangSignals.Instance.onBoomerangNextTarget += _movementController.OnBoomerangNextTarget;
            BoomerangSignals.Instance.onBoomerangReturning += OnBoomerangReturning;
            BoomerangSignals.Instance.onBoomerangHasReturned += OnBoomerangReturned;
            BoomerangSignals.Instance.onBoomerangHasReturned += _movementController.OnBoomerangHasReturned;
            BoomerangSignals.Instance.onSelectBoomerang += meshController.OnSelectBoomerang;
            BoomerangSignals.Instance.onBoomerangStatsChanged += ApplyUpgrades;
        }

        private void OnDestroy()
        {
            if (BoomerangSignals.Instance != null)
            {
                BoomerangSignals.Instance.onBoomerangStatsChanged -= ApplyUpgrades;
            }
        }

        #endregion

        public void ApplyUpgrades()
        {
            Transform parentChild = transform.Find("Parent");
            if (parentChild != null)
            {
                float sizeBonus = BoomerangUpgradeManager.Instance != null 
                    ? BoomerangUpgradeManager.Instance.GetTotalBonus(BoomerangUpgradeType.Size) 
                    : 0f;
                parentChild.localScale = Vector3.one * (1.0f + sizeBonus);
            }
        }

        private void OnLevelFailedOrSuccessful()
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }

        private void OnPlay()
        {
            ApplyUpgrades();
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
            IsFullyCharged = false;
            MissilePoints.Clear();
        }

        private void OnAddPoint(Vector3 pos)
        {
            if (MissilePoints.Count == 0)
            {
                MissilePoints.Add(pos);
            }
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

        private void OnRestartLevel()
        {
            transform.GetChild(0).gameObject.SetActive(false);
            transform.parent = null;
            IsRising = false;
            IsFullyCharged = false;
            MissilePoints.Clear();
            PointIndex = 0;
        }
    }
}