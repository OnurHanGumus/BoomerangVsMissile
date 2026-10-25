using Data.UnityObject;
using Data.ValueObject;
using Interfaces;
using Managers;
using UnityEngine;

namespace Controllers.Missile.Abilities
{
    public class ZigzagMissileAbility : MonoBehaviour, IMissileMovementModifier
    {
        #region Self Variables

        #region Public Variables
        public ZigzagMissileData Data
        {
            get
            {
                if (_data == null)
                {
                    LoadData();
                }
                return _data;
            }
        }
        #endregion

        #region Serialized Variables
        [Header("Zigzag Data")]
        [SerializeField] private CD_ZigzagMissile cdZigzagMissile;
        [SerializeField] private Transform leanTarget;
        #endregion

        #region Private Variables
        private const float DefaultHalfScreenSpan = 5.0f;
        private const float InnerScanOffset = 0.05f;

        private MissileManager _manager;
        private ZigzagMissileData _data;
        private readonly Collider[] _overlapBuffer = new Collider[16];

        private float _spawnX;
        private float _minX;
        private float _maxX;
        private int _direction = 1;
        private bool _isHolding = false;
        private bool _isSpawnStateReady = false;
        private float _lastFlipTime = -10f;
        private float _lastEvaluatedFixedTime = -1f;
        private float _currentLeanAngle = 0f;
        private float _leanVelocity = 0f;
        #endregion

        #endregion

        private void Awake()
        {
            EnsureInitialized();
        }

        public void Initialize(MissileManager manager)
        {
            _manager = manager;
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_manager == null)
            {
                _manager = GetComponent<MissileManager>();
            }
            if (leanTarget == null)
            {
                leanTarget = transform.Find("Parent");
            }
            LoadData();
        }

        private void LoadData()
        {
            if (cdZigzagMissile == null)
            {
                cdZigzagMissile = Resources.Load<CD_ZigzagMissile>("Data/Missiles/CD_ZigzagMissile");
            }
            _data = cdZigzagMissile != null && cdZigzagMissile.Data != null
                ? cdZigzagMissile.Data
                : new ZigzagMissileData();
        }

        private void OnEnable()
        {
            EnsureInitialized();
            ResetSpawnState();
        }

        private void OnDisable()
        {
            _isSpawnStateReady = false;
            _isHolding = false;
            _lastEvaluatedFixedTime = -1f;
            ResetLeanImmediate();
        }

        private void ResetSpawnState()
        {
            _spawnX = transform.position.x;
            _isHolding = false;
            _lastFlipTime = -10f;
            _lastEvaluatedFixedTime = -1f;
            ResetLeanImmediate();

            ComputeHorizontalBounds(out _minX, out _maxX);

            if (Data.StartTowardScreenCenter && Mathf.Abs(_spawnX) > 0.15f)
            {
                _direction = _spawnX > 0f ? -1 : 1;
            }
            else
            {
                _direction = Random.value < 0.5f ? -1 : 1;
            }

            _isSpawnStateReady = true;
        }

        private void ResetLeanImmediate()
        {
            _currentLeanAngle = 0f;
            _leanVelocity = 0f;
            if (leanTarget != null)
            {
                leanTarget.localRotation = Quaternion.identity;
            }
        }

        private void Update()
        {
            if (leanTarget == null || (_manager != null && _manager.IsDead))
            {
                return;
            }

            if (!_isSpawnStateReady)
            {
                ResetSpawnState();
            }

            float targetAngle = _isHolding ? 0f : (_direction * Data.LeanAngle);
            float smoothTime = Mathf.Max(0.01f, Data.LeanSmoothTime);
            _currentLeanAngle = Mathf.SmoothDampAngle(_currentLeanAngle, targetAngle, ref _leanVelocity, smoothTime);
            leanTarget.localRotation = Quaternion.Euler(0f, 0f, _currentLeanAngle);
        }

        private void ComputeHorizontalBounds(out float minX, out float maxX)
        {
            float leftBound = -DefaultHalfScreenSpan;
            float rightBound = DefaultHalfScreenSpan;

            UnityEngine.Camera cam = UnityEngine.Camera.main;
            if (cam != null)
            {
                float depth = Vector3.Dot(transform.position - cam.transform.position, cam.transform.forward);
                if (depth <= 0.01f)
                {
                    depth = Mathf.Abs(transform.position.z - cam.transform.position.z);
                }

                if (depth > 0.01f)
                {
                    float viewLeft = cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, depth)).x;
                    float viewRight = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, depth)).x;
                    leftBound = viewLeft + Data.ScreenEdgeMargin;
                    rightBound = viewRight - Data.ScreenEdgeMargin;
                }
            }

            if (rightBound <= leftBound + 0.2f)
            {
                leftBound = -DefaultHalfScreenSpan;
                rightBound = DefaultHalfScreenSpan;
            }

            float maxAllowedHalfSpan = (rightBound - leftBound) * 0.5f;
            float halfSpan = Mathf.Min(Mathf.Max(0.1f, Data.TravelHalfWidth), maxAllowedHalfSpan);
            float corridorCenter = Mathf.Clamp(_spawnX, leftBound + halfSpan, rightBound - halfSpan);

            minX = corridorCenter - halfSpan;
            maxX = corridorCenter + halfSpan;
        }

        private void EvaluateHorizontalState()
        {
            if (_manager != null && _manager.IsDead)
            {
                return;
            }

            float currentX = transform.position.x;

            if (_isHolding)
            {
                if (!IsAtEdge(_direction, currentX) && !IsSideBlocked(_direction))
                {
                    _isHolding = false;
                }
                else if (!IsAtEdge(-_direction, currentX) && !IsSideBlocked(-_direction))
                {
                    _direction = -_direction;
                    _isHolding = false;
                    _lastFlipTime = Time.time;
                }
                return;
            }

            if (IsAtEdge(_direction, currentX))
            {
                int opposite = -_direction;
                _direction = opposite;
                _lastFlipTime = Time.time;

                if (IsSideBlocked(_direction))
                {
                    _isHolding = true;
                }
                return;
            }

            if (IsSideBlocked(_direction))
            {
                int opposite = -_direction;
                bool canFlip = Time.time - _lastFlipTime >= Data.FlipCooldown
                               && !IsAtEdge(opposite, currentX)
                               && !IsSideBlocked(opposite);

                if (canFlip)
                {
                    _direction = opposite;
                    _lastFlipTime = Time.time;
                }
                else
                {
                    _isHolding = true;
                }
            }
        }

        private bool IsAtEdge(int dir, float currentX)
        {
            return (dir > 0 && currentX >= _maxX) || (dir < 0 && currentX <= _minX);
        }

        private bool IsSideBlocked(int dir)
        {
            GetScanBox(dir, out Vector3 boxCenter, out Vector3 halfExtents);

            int hitCount = Physics.OverlapBoxNonAlloc(
                boxCenter,
                halfExtents,
                _overlapBuffer,
                Quaternion.identity,
                ~0,
                QueryTriggerInteraction.Collide);

            float myX = transform.position.x;

            for (int i = 0; i < hitCount; i++)
            {
                Collider col = _overlapBuffer[i];
                if (col == null || !col.enabled || !col.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (col.transform == transform || col.transform.IsChildOf(transform))
                {
                    continue;
                }

                MissileManager otherMissile = col.GetComponentInParent<MissileManager>();
                if (otherMissile == null || otherMissile == _manager)
                {
                    continue;
                }

                if (!otherMissile.gameObject.activeInHierarchy || !otherMissile.gameObject.activeSelf || otherMissile.IsDead)
                {
                    continue;
                }

                float deltaX = (otherMissile.transform.position.x - myX) * dir;
                if (deltaX >= -InnerScanOffset)
                {
                    return true;
                }
            }

            return false;
        }

        private void GetScanBox(int dir, out Vector3 center, out Vector3 halfExtents)
        {
            Vector3 pos = transform.position;
            float outerOffset = Mathf.Max(InnerScanOffset + 0.1f, Data.BodyHalfWidth + Data.LookAheadDistance);
            float spanX = outerOffset - InnerScanOffset;
            float halfX = spanX * 0.5f;
            float centerX = pos.x + dir * (InnerScanOffset + halfX);

            float above = Mathf.Max(0.1f, Data.CheckAboveHeight);
            float below = Mathf.Max(0.1f, Data.CheckBelowHeight);
            float halfY = (above + below) * 0.5f;
            float centerY = pos.y + (above - below) * 0.5f;

            center = new Vector3(centerX, centerY, pos.z);
            halfExtents = new Vector3(halfX, halfY, 1.0f);
        }

        public Vector3 GetVelocityOffset()
        {
            if (_manager != null && _manager.IsDead)
            {
                return Vector3.zero;
            }

            if (!_isSpawnStateReady)
            {
                ResetSpawnState();
            }

            if (!Mathf.Approximately(_lastEvaluatedFixedTime, Time.fixedTime))
            {
                _lastEvaluatedFixedTime = Time.fixedTime;
                EvaluateHorizontalState();
            }

            if (_isHolding)
            {
                return Vector3.zero;
            }

            return new Vector3(_direction * Data.HorizontalSpeed, 0f, 0f);
        }

        private void OnDrawGizmosSelected()
        {
            ZigzagMissileData data = Data;
            if (data == null)
            {
                return;
            }

            int dir = Application.isPlaying ? _direction : 1;
            GetScanBox(dir, out Vector3 boxCenter, out Vector3 halfExtents);

            Gizmos.color = _isHolding ? Color.red : Color.yellow;
            Gizmos.DrawWireCube(boxCenter, halfExtents * 2f);

            float minX = Application.isPlaying ? _minX : transform.position.x - data.TravelHalfWidth;
            float maxX = Application.isPlaying ? _maxX : transform.position.x + data.TravelHalfWidth;
            float y = transform.position.y;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(new Vector3(minX, y - 1f, transform.position.z), new Vector3(minX, y + 1f, transform.position.z));
            Gizmos.DrawLine(new Vector3(maxX, y - 1f, transform.position.z), new Vector3(maxX, y + 1f, transform.position.z));
        }
    }
}
