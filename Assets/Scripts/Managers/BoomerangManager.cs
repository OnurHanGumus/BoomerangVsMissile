using System;
using System.Collections.Generic;
using Commands;
using Controllers;
using Controllers.Boomerang;
using Data.UnityObject;
using Data.ValueObject;
using DG.Tweening;
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

        public int EffectiveDamage
        {
            get
            {
                var current = BoomerangStoreManager.Instance != null ? BoomerangStoreManager.Instance.CurrentBoomerangData : null;
                int baseDamage = current != null ? current.Damage : 1;
                return _buffController != null ? _buffController.ApplyDamageBuff(baseDamage) : baseDamage;
            }
        }

        public float EffectiveSpeed
        {
            get
            {
                var current = BoomerangStoreManager.Instance != null ? BoomerangStoreManager.Instance.CurrentBoomerangData : null;
                float baseSpeed = current != null ? current.Speed : 5f;
                return _buffController != null ? _buffController.ApplySpeedBuff(baseSpeed) : baseSpeed;
            }
        }

        public float EffectiveReturnSpeedMultiplier
        {
            get
            {
                var current = BoomerangStoreManager.Instance != null ? BoomerangStoreManager.Instance.CurrentBoomerangData : null;
                float baseReturnMultiplier = current != null ? current.ReturnSpeedMultiplier : 1f;
                return _buffController != null ? _buffController.ApplyReturnSpeedBuff(baseReturnMultiplier) : baseReturnMultiplier;
            }
        }

        public float EffectiveSizeMultiplier
        {
            get
            {
                var current = BoomerangStoreManager.Instance != null ? BoomerangStoreManager.Instance.CurrentBoomerangData : null;
                float baseSize = current != null ? current.SizeMultiplier : 1f;
                return _buffController != null ? _buffController.ApplySizeBuff(baseSize) : baseSize;
            }
        }

        public float BuffReturnSplineMultiplier =>
            _buffController != null ? _buffController.GetReturnSplineSpeedMultiplier() : 1f;

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
        private BoomerangBuffController _buffController;
        private Tween _sizeTween;

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _data = GetData();
            _movementController = GetComponent<BoomerangMovementController>();
            _buffController = GetComponent<BoomerangBuffController>();
            if (_buffController == null)
            {
                _buffController = gameObject.AddComponent<BoomerangBuffController>();
            }

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
            _sizeTween?.Kill();
            UnsubscribeEvents();
        }

        #endregion

        public void ApplyStats()
        {
            Transform parentChild = transform.Find("Parent");
            if (parentChild != null)
            {
                float sizeMultiplier = EffectiveSizeMultiplier;
                _sizeTween?.Kill();
                if (IsThrown && parentChild.gameObject.activeInHierarchy)
                {
                    _sizeTween = parentChild
                        .DOScale(Vector3.one * sizeMultiplier, 0.2f)
                        .SetEase(Ease.OutBack)
                        .SetUpdate(true)
                        .SetLink(gameObject);
                }
                else
                {
                    parentChild.localScale = Vector3.one * sizeMultiplier;
                }
            }
        }

        public void ApplyUpgrades() => ApplyStats();

        private void OnLevelFailedOrSuccessful()
        {
            _sizeTween?.Kill();
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
            _sizeTween?.Kill();
            transform.GetChild(0).gameObject.SetActive(false);
            transform.parent = null;
            IsRising = false;
            IsFullyCharged = false;
            MissilePoints.Clear();
            PointIndex = 0;
        }
    }
}
