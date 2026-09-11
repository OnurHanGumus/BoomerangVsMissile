using Data.ValueObject;
using Enums;
using Extensions;
using Managers;
using Signals;
using System.Collections.Generic;
using UnityEngine;

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
        private CatmullRomSpline _returnSpline;

        private Vector3 _initializePos = new Vector3(0, -4, 0);
        private Vector3 _currentDir;
        public bool _isPointMissed = false;

        private bool _isReturning = false;
        private int _returnSegment = 0;
        private float _returnProgress = 0f;
        private float _currentReturnSwingDir = 1f;

        private float _chargedArcWidth = -1f;
        private float _chargedArcHeight = -1f;
        private bool _firstHitHandled = false;
        private bool _hasSelectedReturnSwingDir = false;
        private float _selectedReturnSwingDir = 1f;
        private bool _isEmergencyRecalling = false;

        #endregion

        #endregion

        public bool IsEmergencyRecalling => _isEmergencyRecalling;

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _rig = GetComponent<Rigidbody>();
            _manager = GetComponent<BoomerangManager>();
            _data = _manager.GetData();
            _returnSpline = new CatmullRomSpline();
            BoomerangSignals.Instance.onSetChargedArc += OnSetChargedArc;
            BoomerangSignals.Instance.onEmergencyRecall += OnEmergencyRecall;
        }

        private void OnDestroy()
        {
            if (BoomerangSignals.Instance != null)
            {
                BoomerangSignals.Instance.onSetChargedArc -= OnSetChargedArc;
                BoomerangSignals.Instance.onEmergencyRecall -= OnEmergencyRecall;
            }
        }

        private void OnEmergencyRecall()
        {
            if (!_manager.IsThrown || _isEmergencyRecalling)
            {
                return;
            }

            _isEmergencyRecalling = true;
            _isReturning = false;
            _manager.IsRising = false;
            _firstHitHandled = true;

            BoomerangSignals.Instance.onBoomerangReturning?.Invoke();

            AudioSignals.Instance.onPlaySound(AudioSoundEnums.Pitch);
        }

        private void OnSetChargedArc(float width, float height, bool isFullyCharged, float returnSwingDir)
        {
            _chargedArcWidth = width;
            _chargedArcHeight = height;
            _manager.IsFullyCharged = isFullyCharged;
            _selectedReturnSwingDir = returnSwingDir;
            _hasSelectedReturnSwingDir = true;
        }

        private void FixedUpdate()
        {
            if (!_manager.IsThrown)
            {
                return;
            }

            if (_isEmergencyRecalling)
            {
                MoveDirectToRecall();
            }
            else if (_isReturning)
            {
                MoveAlongReturnSpline();
            }
            else
            {
                MoveDirectToTarget();
            }

            Spin();
        }

        #region Emergency Recall Phase

        private void MoveDirectToRecall()
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

            Vector3 toTarget = _initializePos - transform.position;
            toTarget.z = 0f;

            float speed = _manager.EffectiveSpeed + (_manager.EffectiveSpeed * _manager.EffectiveReturnSpeedMultiplier);
            Vector3 recallVelocity = toTarget.normalized * speed;
            _rig.linearVelocity = recallVelocity;

            if (toTarget.sqrMagnitude <= 0.25f || Vector3.Dot(recallVelocity, toTarget) <= 0f)
            {
                transform.position = _initializePos;
                _isEmergencyRecalling = false;
                _isReturning = false;
                _manager.IsThrown = false;
                _rig.linearVelocity = Vector3.zero;
                _rig.angularVelocity = Vector3.zero;
                BoomerangSignals.Instance.onBoomerangHasReturned?.Invoke();
            }
        }

        #endregion

        #region Sharp Direct Phase

        private void MoveDirectToTarget()
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

            if (_isPointMissed || _manager.MissilePoints == null || _manager.MissilePoints.Count == 0)
            {
                return;
            }

            _rig.linearVelocity = _currentDir;

            // Check if boomerang reached the target point proximity threshold or passed it
            Vector3 currentTarget = _manager.MissilePoints[_manager.PointIndex];
            Vector3 toTarget = currentTarget - transform.position;
            toTarget.z = 0f;

            if (toTarget.sqrMagnitude <= 0.09f || Vector3.Dot(_currentDir, toTarget) <= 0f)
            {
                OnTargetReached();
            }
        }

        private Vector3 GetDirection()
        {
            if (_manager.MissilePoints == null || _manager.MissilePoints.Count == 0)
            {
                return Vector3.zero;
            }

            int index = Mathf.Clamp(_manager.PointIndex, 0, _manager.MissilePoints.Count - 1);
            Vector3 target = _manager.MissilePoints[index];
            Vector3 dir = (target - transform.position).normalized * _manager.EffectiveSpeed;
            return new Vector3(dir.x, dir.y, 0f);
        }

        private void OnTargetReached()
        {
            StartReturnArc();
        }

        #endregion

        #region Smooth Return Arc Phase

        private void StartReturnArc()
        {
            if (_isReturning)
            {
                return;
            }

            _isReturning = true;
            _manager.IsRising = false;
            BoomerangSignals.Instance.onBoomerangReturning?.Invoke();

            // Build smooth Catmull-Rom return trajectory
            Vector3 currentPos = new Vector3(transform.position.x, transform.position.y, 0f);
            Vector3 target = _manager.MissilePoints.Count > 0 ? _manager.MissilePoints[0] : currentPos;

            float arcWidth = _chargedArcWidth > 0 ? _chargedArcWidth : _data.ReturnArcWidth;
            float arcHeight = _chargedArcHeight > 0 ? _chargedArcHeight : _data.ReturnArcHeight;

            // Inward direction bias if target is near screen borders, or player-selected drag direction
            _currentReturnSwingDir = _hasSelectedReturnSwingDir ? _selectedReturnSwingDir : CalculateSwingDirection(target.x);

            // Apex loop point beyond the target
            Vector3 apexPoint = new Vector3(
                target.x + (_currentReturnSwingDir * arcWidth),
                target.y + arcHeight,
                0f
            );

            // Mid descent swoop point towards the return position
            Vector3 midDescentPoint = new Vector3(
                (apexPoint.x + _initializePos.x) * 0.5f + (_currentReturnSwingDir * arcWidth * 0.4f),
                (apexPoint.y + _initializePos.y) * 0.5f,
                0f
            );

            List<Vector3> returnPoints = new List<Vector3>
            {
                currentPos,
                target,
                apexPoint,
                midDescentPoint,
                _initializePos
            };

            _returnSpline.SetControlPoints(returnPoints);
            _returnSegment = 0;
            _returnProgress = 0f;
        }

        private void MoveAlongReturnSpline()
        {
            if (_returnSpline == null || _returnSpline.SegmentCount == 0)
            {
                return;
            }

            float currentSpeed = _manager.EffectiveSpeed + (_manager.EffectiveSpeed * _manager.EffectiveReturnSpeedMultiplier); ;
            float segmentLength = _returnSpline.GetSegmentLength(_returnSegment);
            if (segmentLength <= 0.0001f)
            {
                segmentLength = 0.001f;
            }

            _returnProgress += (currentSpeed * Time.fixedDeltaTime) / segmentLength;

            while (_returnProgress >= 1f && _returnSegment < _returnSpline.SegmentCount)
            {
                _returnProgress -= 1f;
                _returnSegment++;

                if (_returnSegment >= _returnSpline.SegmentCount)
                {
                    // Completed return to player
                    _isReturning = false;
                    _manager.IsThrown = false;
                    _rig.linearVelocity = Vector3.zero;
                    _rig.angularVelocity = Vector3.zero;
                    BoomerangSignals.Instance.onBoomerangHasReturned?.Invoke();
                    return;
                }

                segmentLength = _returnSpline.GetSegmentLength(_returnSegment);
                if (segmentLength <= 0.0001f)
                {
                    segmentLength = 0.001f;
                }
            }

            // Sample curved position on the return spline
            Vector3 targetPos = _returnSpline.EvaluateSegment(_returnSegment, Mathf.Clamp01(_returnProgress));
            targetPos.z = 0f;

            Vector3 delta = targetPos - transform.position;
            delta.z = 0f;
            _rig.linearVelocity = delta / Time.fixedDeltaTime;
            transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
        }

        #endregion

        #region Extra Return Arc Swing (Dynamic Hit in Return Phase)

        private void TriggerExtraReturnSwing()
        {
            Vector3 currentPos = new Vector3(transform.position.x, transform.position.y, 0f);

            float arcWidth = _chargedArcWidth > 0 ? _chargedArcWidth : _data.ReturnArcWidth;
            float arcHeight = _chargedArcHeight > 0 ? _chargedArcHeight : _data.ReturnArcHeight;

            // Swap swing direction on X axis each time an extra swing is triggered
            _currentReturnSwingDir = -_currentReturnSwingDir;

            // Apex loop point from the current hit position
            Vector3 apexPoint = new Vector3(
                currentPos.x + (_currentReturnSwingDir * arcWidth),
                currentPos.y + (arcHeight * 0.75f),
                0f
            );

            // Mid descent swoop point towards the return position
            Vector3 midDescentPoint = new Vector3(
                (apexPoint.x + _initializePos.x) * 0.5f + (_currentReturnSwingDir * arcWidth * 0.35f),
                (apexPoint.y + _initializePos.y) * 0.5f,
                0f
            );

            List<Vector3> newReturnPoints = new List<Vector3>
            {
                currentPos,
                apexPoint,
                midDescentPoint,
                _initializePos
            };

            _returnSpline.SetControlPoints(newReturnPoints);
            _returnSegment = 0;
            _returnProgress = 0f;

            BoomerangSignals.Instance.onCombo?.Invoke(Mathf.Max(0, _manager.PointIndex - 1));
        }

        #endregion

        private void Spin()
        {
            _rig.angularVelocity = new Vector3(0, 0, _data.AngularSpeed * (_manager.IsRight ? 1 : -1));
            _rig.maxAngularVelocity = 50;
        }

        public void Thrown()
        {
            _manager.IsRising = true;
            _isReturning = false;
            _isPointMissed = false;
            _manager.PointIndex = 0;

            // Ensure the list maintains the exact structure: only first selected target + return point
            if (_manager.MissilePoints.Count > 1)
            {
                Vector3 firstTarget = _manager.MissilePoints[0];
                _manager.MissilePoints.Clear();
                _manager.MissilePoints.Add(firstTarget);
            }
            if (_manager.MissilePoints.Count == 1)
            {
                _manager.MissilePoints.Add(_initializePos);
            }

            _firstHitHandled = false;
            _currentDir = GetDirection();
            _manager.IsThrown = true;
        }

        public static float CalculateSwingDirection(float targetX)
        {
            if (targetX >= 0.5f)
            {
                return -1f;
            }
            if (targetX <= -0.5f)
            {
                return 1f;
            }
            return 1f;
        }

        public void OnBoomerangNextTarget()
        {
            if (_isEmergencyRecalling)
            {
                _manager.PointIndex++;
                BoomerangSignals.Instance.onCombo?.Invoke(Mathf.Max(0, _manager.PointIndex - 1));
                return;
            }

            if (!_firstHitHandled)
            {
                _firstHitHandled = true;
                if (!_isReturning)
                {
                    StartReturnArc();
                }
                return;
            }

            if (_isReturning)
            {
                TriggerExtraReturnSwing();
                return;
            }

            StartReturnArc();
        }

        public void OnPlay()
        {
            ResetFlight();
        }

        public void OnBoomerangHasReturned()
        {
            _manager.IsThrown = false;
            ResetFlight();
        }

        public void OnLevelFailed()
        {
            _manager.IsThrown = false;
            _isReturning = false;
            _isEmergencyRecalling = false;
            _rig.linearVelocity = Vector3.zero;
            _rig.angularVelocity = Vector3.zero;
        }

        public void OnLevelSuccess()
        {
            _manager.IsThrown = false;
            _isReturning = false;
            _isEmergencyRecalling = false;
            _rig.linearVelocity = Vector3.zero;
            _rig.angularVelocity = Vector3.zero;
        }

        public void OnRestartLevel()
        {
            _manager.MissilePoints.Clear();
            _manager.IsThrown = false;
            ResetFlight();
        }

        private void ResetFlight()
        {
            _isReturning = false;
            _isEmergencyRecalling = false;
            _isPointMissed = false;
            _rig.linearVelocity = Vector3.zero;
            _rig.angularVelocity = Vector3.zero;
            _returnSegment = 0;
            _returnProgress = 0f;
            _currentReturnSwingDir = 1f;
            _chargedArcWidth = -1f;
            _chargedArcHeight = -1f;
            _firstHitHandled = false;
            _hasSelectedReturnSwingDir = false;
            _selectedReturnSwingDir = 1f;
            _manager.IsFullyCharged = false;
        }
    }
}