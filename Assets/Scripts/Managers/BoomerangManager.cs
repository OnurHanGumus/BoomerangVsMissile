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
        public List<Vector3> MissilePoints;
        public int PointIndex = 0;
        public bool IsRight = true;
        public bool IsThrown = false;
        public bool IsRising = false;
        public bool IsFullyCharged = false;
        public bool IsEmergencyRecalling => _movementController != null && _movementController.IsEmergencyRecalling;

        public BoomerangItemData GetBoomerangData()
        {
            return BoomerangStoreManager.Instance.CurrentBoomerangData;
        }

        public Vector3 EffectiveHandLocalPosition
        {
            get
            {
                var current = BoomerangStoreManager.Instance != null ? BoomerangStoreManager.Instance.CurrentBoomerangData : null;
                if (current != null && (current.HandLocalPosition != Vector3.zero || current.HandLocalEulerAngles != Vector3.zero))
                {
                    return current.HandLocalPosition;
                }
                return _data != null ? _data.BoomerangHandLocalPosition : new Vector3(0.106f, 0.425f, -0.144f);
            }
        }

        public Vector3 EffectiveHandLocalEulerAngles
        {
            get
            {
                var current = BoomerangStoreManager.Instance != null ? BoomerangStoreManager.Instance.CurrentBoomerangData : null;
                if (current != null && (current.HandLocalPosition != Vector3.zero || current.HandLocalEulerAngles != Vector3.zero))
                {
                    return current.HandLocalEulerAngles;
                }
                return _data != null ? _data.BoomerangHandLocalEulerAngles : new Vector3(19.138f, 52.601f, -105.943f);
            }
        }

        [SerializeField] private BoomerangMeshController meshController;
        [SerializeField] private BoomerangPhysicsController physicsController;
        [SerializeField] private Controllers.Boomerang.TrajectoryPreviewController trajectoryPreviewController;


        private PlayerData _data;
        private BoomerangMovementController _movementController;

        private void Awake()
        {
            Init();
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
            ApplyStats();
        }

        #region Event Subscription
        private void OnEnable()
        {
            SubscribeEvents();

        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            CoreGameSignals.Instance.onPlay += OnPlay;
            CoreGameSignals.Instance.onPlay += _movementController.OnPlay;;
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
            BoomerangSignals.Instance.onBoomerangStatsChanged += ApplyStats;
        }

        private void UnsubscribeEvents()
        {
            if (CoreGameSignals.Instance != null)
            {
                CoreGameSignals.Instance.onPlay -= OnPlay;
                if (_movementController != null) CoreGameSignals.Instance.onPlay -= _movementController.OnPlay;
                CoreGameSignals.Instance.onLevelSuccessful -= OnLevelFailedOrSuccessful;
                CoreGameSignals.Instance.onLevelFailed -= OnLevelFailedOrSuccessful;
                CoreGameSignals.Instance.onRestartLevel -= OnRestartLevel;
                if (_movementController != null) CoreGameSignals.Instance.onRestartLevel -= _movementController.OnRestartLevel;
            }

            if (InputSignals.Instance != null)
            {
                InputSignals.Instance.onClicking -= OnAddPoint;
                InputSignals.Instance.onInputReleased -= OnInputRelease;
            }

            if (BoomerangSignals.Instance != null)
            {
                BoomerangSignals.Instance.onBoomerangNextTarget -= OnBoomerangNextTarget;
                if (_movementController != null) BoomerangSignals.Instance.onBoomerangNextTarget -= _movementController.OnBoomerangNextTarget;
                BoomerangSignals.Instance.onBoomerangReturning -= OnBoomerangReturning;
                BoomerangSignals.Instance.onBoomerangHasReturned -= OnBoomerangReturned;
                if (_movementController != null) BoomerangSignals.Instance.onBoomerangHasReturned -= _movementController.OnBoomerangHasReturned;
                BoomerangSignals.Instance.onBoomerangStatsChanged -= ApplyStats;
            }
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        #endregion

        public void ApplyStats()
        {
            Transform parentChild = transform.Find("Parent");
            if (parentChild != null)
            {
                var current = BoomerangStoreManager.Instance != null ? BoomerangStoreManager.Instance.CurrentBoomerangData : null;
                float sizeMultiplier = current != null ? current.SizeMultiplier : 1.0f;
                parentChild.localScale = Vector3.one * sizeMultiplier;
            }
        }

        public void ApplyUpgrades() => ApplyStats();

        private void OnLevelFailedOrSuccessful()
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }

        private void OnPlay()
        {
            ApplyStats();
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
        }

        private void OnBoomerangReturned()
        {
            PointIndex = 0;
            IsFullyCharged = false;
            MissilePoints.Clear();
        }

        private void OnAddPoint(Vector3 pos)
        {
            if (MissilePoints == null)
            {
                MissilePoints = new List<Vector3>();
            }

            if (MissilePoints.Count == 0)
            {
                MissilePoints.Add(pos);
            }
            else if (!IsThrown)
            {
                MissilePoints[0] = pos;
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