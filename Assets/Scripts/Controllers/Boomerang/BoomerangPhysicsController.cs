using Data.ValueObject;
using Managers;
using Signals;
using UnityEngine;

namespace Controllers
{
    public class BoomerangPhysicsController : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables
        [SerializeField] private BoomerangManager manager;
        #endregion

        #region Private Variables
        private struct SweptMissileHit
        {
            public MissilePhysicsController MissilePhysics;
            public float HitTime;
            public Vector3 ContactReferencePoint;
        }

        private const int MaxOverlapResults = 32;
        private const float BroadphaseMotionPadding = 0.5f;
        private const float MinSweepDistanceSqr = 0.000001f;

        private PlayerData _data;
        private BoxCollider _boxCollider;
        private readonly Collider[] _overlapBuffer = new Collider[MaxOverlapResults];
        private readonly SweptMissileHit[] _sweptHitsBuffer = new SweptMissileHit[MaxOverlapResults];

        private bool _hasPreviousSweepState;
        private Vector3 _previousCenter;
        private Quaternion _previousRotation;
        #endregion

        #endregion

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _data = manager.GetData();
            _boxCollider = GetComponent<BoxCollider>();
        }

        private void OnEnable()
        {
            _hasPreviousSweepState = false;
        }

        private void OnDisable()
        {
            _hasPreviousSweepState = false;
        }

        private void FixedUpdate()
        {
            if (!manager.IsThrown)
            {
                _hasPreviousSweepState = false;
                return;
            }

            Vector3 currentCenter = GetColliderWorldCenter();
            Quaternion currentRotation = transform.rotation;

            if (!_hasPreviousSweepState)
            {
                _previousCenter = currentCenter;
                _previousRotation = currentRotation;
                _hasPreviousSweepState = true;
                return;
            }

            SweepForMissiles(_previousCenter, _previousRotation, currentCenter, currentRotation);

            if (manager.IsThrown)
            {
                _previousCenter = currentCenter;
                _previousRotation = currentRotation;
            }
            else
            {
                _hasPreviousSweepState = false;
            }
        }

        private Vector3 GetColliderWorldCenter()
        {
            return transform.TransformPoint(_boxCollider.center);
        }

        private void SweepForMissiles(Vector3 startCenter, Quaternion startRotation, Vector3 endCenter, Quaternion endRotation)
        {
            Vector3 boomDelta = endCenter - startCenter;
            if (boomDelta.sqrMagnitude < MinSweepDistanceSqr)
            {
                return;
            }

            Vector3 lossyScale = transform.lossyScale;
            Vector3 boomHalf = new Vector3(
                Mathf.Abs(_boxCollider.size.x * lossyScale.x) * 0.5f,
                Mathf.Abs(_boxCollider.size.y * lossyScale.y) * 0.5f,
                Mathf.Abs(_boxCollider.size.z * lossyScale.z) * 0.5f);

            float distance = boomDelta.magnitude;
            float boomMaxRadiusXY = Mathf.Sqrt((boomHalf.x * boomHalf.x) + (boomHalf.y * boomHalf.y));
            Vector3 sweepCenter = (startCenter + endCenter) * 0.5f;
            float angleDeg = Mathf.Atan2(boomDelta.y, boomDelta.x) * Mathf.Rad2Deg;
            Quaternion sweepOrientation = Quaternion.Euler(0f, 0f, angleDeg);
            Vector3 sweepHalfExtents = new Vector3(
                (distance * 0.5f) + boomMaxRadiusXY + BroadphaseMotionPadding,
                boomMaxRadiusXY + BroadphaseMotionPadding,
                boomHalf.z + BroadphaseMotionPadding);

            int overlapCount = Physics.OverlapBoxNonAlloc(
                sweepCenter,
                sweepHalfExtents,
                _overlapBuffer,
                sweepOrientation,
                ~0,
                QueryTriggerInteraction.Collide);

            int sweptHitCount = 0;
            for (int i = 0; i < overlapCount; i++)
            {
                Collider other = _overlapBuffer[i];
                if (!other.CompareTag("Missile"))
                {
                    continue;
                }

                MissilePhysicsController missilePhysics = other.GetComponent<MissilePhysicsController>();
                if (IsAlreadyCollected(missilePhysics, sweptHitCount))
                {
                    continue;
                }

                BoxCollider missileBox = (BoxCollider)other;
                Rigidbody missileRigidbody = other.attachedRigidbody;

                if (TryComputeRelativeSweepHit(
                    startCenter,
                    startRotation,
                    endCenter,
                    endRotation,
                    boomHalf,
                    missileBox,
                    missileRigidbody,
                    out float hitTime,
                    out Vector3 contactReferencePoint))
                {
                    _sweptHitsBuffer[sweptHitCount] = new SweptMissileHit
                    {
                        MissilePhysics = missilePhysics,
                        HitTime = hitTime,
                        ContactReferencePoint = contactReferencePoint
                    };
                    sweptHitCount++;
                }
            }

            for (int i = 1; i < sweptHitCount; i++)
            {
                SweptMissileHit key = _sweptHitsBuffer[i];
                int j = i - 1;
                while (j >= 0 && _sweptHitsBuffer[j].HitTime > key.HitTime)
                {
                    _sweptHitsBuffer[j + 1] = _sweptHitsBuffer[j];
                    j--;
                }
                _sweptHitsBuffer[j + 1] = key;
            }

            for (int i = 0; i < sweptHitCount; i++)
            {
                if (!manager.IsThrown)
                {
                    break;
                }

                _sweptHitsBuffer[i].MissilePhysics.HandleSweptBoomerangHit(
                    _boxCollider,
                    _sweptHitsBuffer[i].ContactReferencePoint);
            }
        }

        private bool IsAlreadyCollected(MissilePhysicsController missilePhysics, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (_sweptHitsBuffer[i].MissilePhysics == missilePhysics)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryComputeRelativeSweepHit(
            Vector3 boomStartCenter,
            Quaternion boomStartRotation,
            Vector3 boomEndCenter,
            Quaternion boomEndRotation,
            Vector3 boomHalf,
            BoxCollider missileBox,
            Rigidbody missileRigidbody,
            out float hitTime,
            out Vector3 contactReferencePoint)
        {
            hitTime = 0f;
            contactReferencePoint = Vector3.zero;

            Transform missileTransform = missileBox.transform;
            Vector3 missileScale = missileTransform.lossyScale;
            Vector3 missileHalf = new Vector3(
                Mathf.Abs(missileBox.size.x * missileScale.x) * 0.5f,
                Mathf.Abs(missileBox.size.y * missileScale.y) * 0.5f,
                Mathf.Abs(missileBox.size.z * missileScale.z) * 0.5f);

            Vector3 missileEndCenter = missileTransform.TransformPoint(missileBox.center);
            Vector3 missileDelta = missileRigidbody.linearVelocity * Time.fixedDeltaTime;
            Vector3 missileStartCenter = missileEndCenter - missileDelta;

            Vector3 boomDelta = boomEndCenter - boomStartCenter;
            Vector3 relStart = boomStartCenter - missileStartCenter;
            Vector3 relDelta = boomDelta - missileDelta;

            Quaternion missileRot = missileTransform.rotation;
            Vector3 mRight = missileRot * Vector3.right;
            Vector3 mUp = missileRot * Vector3.up;
            Vector3 mForward = missileRot * Vector3.forward;

            // Step 1: Bounding envelope across boomerang spin on the missile's 3 axes to locate the crossing window
            Quaternion midRot = Quaternion.Slerp(boomStartRotation, boomEndRotation, 0.5f);
            Vector3 r0Right = boomStartRotation * Vector3.right;
            Vector3 r0Up = boomStartRotation * Vector3.up;
            Vector3 r0Forward = boomStartRotation * Vector3.forward;
            Vector3 rMidRight = midRot * Vector3.right;
            Vector3 rMidUp = midRot * Vector3.up;
            Vector3 rMidForward = midRot * Vector3.forward;
            Vector3 r1Right = boomEndRotation * Vector3.right;
            Vector3 r1Up = boomEndRotation * Vector3.up;
            Vector3 r1Forward = boomEndRotation * Vector3.forward;

            float envMin = 0f;
            float envMax = 1f;

            float envBoomX = MaxProjectedHalfExtent(mRight, r0Right, r0Up, r0Forward, rMidRight, rMidUp, rMidForward, r1Right, r1Up, r1Forward, boomHalf);
            if (!ClipAxisInterval(mRight, relStart, relDelta, missileHalf.x + envBoomX, ref envMin, ref envMax))
            {
                return false;
            }

            float envBoomY = MaxProjectedHalfExtent(mUp, r0Right, r0Up, r0Forward, rMidRight, rMidUp, rMidForward, r1Right, r1Up, r1Forward, boomHalf);
            if (!ClipAxisInterval(mUp, relStart, relDelta, missileHalf.y + envBoomY, ref envMin, ref envMax))
            {
                return false;
            }

            float envBoomZ = MaxProjectedHalfExtent(mForward, r0Right, r0Up, r0Forward, rMidRight, rMidUp, rMidForward, r1Right, r1Up, r1Forward, boomHalf);
            if (!ClipAxisInterval(mForward, relStart, relDelta, missileHalf.z + envBoomZ, ref envMin, ref envMax))
            {
                return false;
            }

            // Step 2: Exact 5-axis relative-motion SAT at the interpolated crossing orientation
            float evalTime = (envMin + envMax) * 0.5f;
            Quaternion evalRot = Quaternion.Slerp(boomStartRotation, boomEndRotation, evalTime);
            Vector3 bRight = evalRot * Vector3.right;
            Vector3 bUp = evalRot * Vector3.up;
            Vector3 bForward = evalRot * Vector3.forward;

            float uMin = 0f;
            float uMax = 1f;

            if (!ClipAxisInterval(mRight, relStart, relDelta, missileHalf.x + ProjectBoxHalfExtent(mRight, bRight, bUp, bForward, boomHalf), ref uMin, ref uMax))
            {
                return false;
            }

            if (!ClipAxisInterval(mUp, relStart, relDelta, missileHalf.y + ProjectBoxHalfExtent(mUp, bRight, bUp, bForward, boomHalf), ref uMin, ref uMax))
            {
                return false;
            }

            if (!ClipAxisInterval(mForward, relStart, relDelta, missileHalf.z + ProjectBoxHalfExtent(mForward, bRight, bUp, bForward, boomHalf), ref uMin, ref uMax))
            {
                return false;
            }

            if (!ClipAxisInterval(bRight, relStart, relDelta, ProjectBoxHalfExtent(bRight, mRight, mUp, mForward, missileHalf) + boomHalf.x, ref uMin, ref uMax))
            {
                return false;
            }

            if (!ClipAxisInterval(bUp, relStart, relDelta, ProjectBoxHalfExtent(bUp, mRight, mUp, mForward, missileHalf) + boomHalf.y, ref uMin, ref uMax))
            {
                return false;
            }

            hitTime = uMin;
            Vector3 relativeContactOffset = relStart + (relDelta * hitTime);
            contactReferencePoint = missileEndCenter + relativeContactOffset;
            return true;
        }

        private static float MaxProjectedHalfExtent(
            Vector3 axis,
            Vector3 r0Right, Vector3 r0Up, Vector3 r0Forward,
            Vector3 rMidRight, Vector3 rMidUp, Vector3 rMidForward,
            Vector3 r1Right, Vector3 r1Up, Vector3 r1Forward,
            Vector3 halfExtents)
        {
            float p0 = ProjectBoxHalfExtent(axis, r0Right, r0Up, r0Forward, halfExtents);
            float pMid = ProjectBoxHalfExtent(axis, rMidRight, rMidUp, rMidForward, halfExtents);
            float p1 = ProjectBoxHalfExtent(axis, r1Right, r1Up, r1Forward, halfExtents);
            return Mathf.Max(p0, Mathf.Max(pMid, p1));
        }

        private static float ProjectBoxHalfExtent(Vector3 axis, Vector3 boxRight, Vector3 boxUp, Vector3 boxForward, Vector3 halfExtents)
        {
            return (Mathf.Abs(Vector3.Dot(axis, boxRight)) * halfExtents.x)
                 + (Mathf.Abs(Vector3.Dot(axis, boxUp)) * halfExtents.y)
                 + (Mathf.Abs(Vector3.Dot(axis, boxForward)) * halfExtents.z);
        }

        private static bool ClipAxisInterval(Vector3 axis, Vector3 relStart, Vector3 relDelta, float combinedHalfExtent, ref float uMin, ref float uMax)
        {
            float p0 = Vector3.Dot(axis, relStart);
            float d = Vector3.Dot(axis, relDelta);

            if (Mathf.Abs(d) < 1e-6f)
            {
                return Mathf.Abs(p0) <= combinedHalfExtent;
            }

            float invD = 1f / d;
            float t1 = (-combinedHalfExtent - p0) * invD;
            float t2 = (combinedHalfExtent - p0) * invD;

            if (t1 > t2)
            {
                float tmp = t1;
                t1 = t2;
                t2 = tmp;
            }

            if (t1 > uMin)
            {
                uMin = t1;
            }

            if (t2 < uMax)
            {
                uMax = t2;
            }

            return uMin <= uMax;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("CatchArea"))
            {
                if (manager.IsRising)
                {
                    return;
                }
                _hasPreviousSweepState = false;
                BoomerangSignals.Instance.onBoomerangHasReturned?.Invoke();
                transform.parent.parent.position = other.transform.position;
            }
            else if (other.CompareTag("BoomerangHand"))
            {
                transform.parent.parent.parent = other.transform;
                transform.parent.parent.localPosition = manager != null
                    ? manager.EffectiveHandLocalPosition
                    : _data.BoomerangHandLocalPosition;
                transform.parent.parent.localEulerAngles = manager != null
                    ? manager.EffectiveHandLocalEulerAngles
                    : _data.BoomerangHandLocalEulerAngles;
                BoomerangSignals.Instance.onPlayerTouchedBoomerang?.Invoke();
            }
        }
    }
}
