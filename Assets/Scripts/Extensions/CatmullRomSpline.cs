using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
    public class CatmullRomSpline
    {
        private List<Vector3> _controlPoints = new List<Vector3>();
        private List<float> _segmentLengths = new List<float>();
        private float _totalLength = 0f;

        public int SegmentCount => Mathf.Max(0, _controlPoints.Count - 1);
        public float TotalLength => _totalLength;

        public void SetControlPoints(List<Vector3> points)
        {
            _controlPoints.Clear();
            _segmentLengths.Clear();
            _totalLength = 0f;

            if (points == null || points.Count < 2)
            {
                if (points != null)
                {
                    _controlPoints.AddRange(points);
                }
                return;
            }

            _controlPoints.AddRange(points);
            CalculateSegmentLengths();
        }

        private void CalculateSegmentLengths()
        {
            int segments = SegmentCount;
            for (int i = 0; i < segments; i++)
            {
                float length = 0f;
                Vector3 prev = EvaluateSegment(i, 0f);
                const int samples = 10;
                for (int s = 1; s <= samples; s++)
                {
                    float u = (float)s / samples;
                    Vector3 curr = EvaluateSegment(i, u);
                    length += Vector3.Distance(prev, curr);
                    prev = curr;
                }
                _segmentLengths.Add(Mathf.Max(length, 0.001f));
                _totalLength += length;
            }
        }

        public float GetSegmentLength(int segmentIndex)
        {
            if (segmentIndex < 0 || segmentIndex >= _segmentLengths.Count)
            {
                return 0.001f;
            }
            return _segmentLengths[segmentIndex];
        }

        public Vector3 EvaluateSegment(int segmentIndex, float u)
        {
            if (_controlPoints.Count < 2)
            {
                return _controlPoints.Count > 0 ? _controlPoints[0] : Vector3.zero;
            }

            segmentIndex = Mathf.Clamp(segmentIndex, 0, SegmentCount - 1);
            u = Mathf.Clamp01(u);

            Vector3 p0 = GetControlPoint(segmentIndex - 1, segmentIndex, segmentIndex + 1);
            Vector3 p1 = _controlPoints[segmentIndex];
            Vector3 p2 = _controlPoints[segmentIndex + 1];
            Vector3 p3 = GetControlPoint(segmentIndex + 2, segmentIndex + 1, segmentIndex);

            return CalculateCatmullRom(p0, p1, p2, p3, u);
        }

        public Vector3 Evaluate(float t)
        {
            if (_controlPoints.Count < 2)
            {
                return _controlPoints.Count > 0 ? _controlPoints[0] : Vector3.zero;
            }

            int count = SegmentCount;
            if (count == 0)
            {
                return _controlPoints[0];
            }

            t = Mathf.Clamp01(t);
            float scaledT = t * count;
            int segmentIndex = Mathf.FloorToInt(scaledT);
            if (segmentIndex >= count)
            {
                segmentIndex = count - 1;
            }
            float u = scaledT - segmentIndex;

            return EvaluateSegment(segmentIndex, u);
        }

        public Vector3 EvaluateSegmentTangent(int segmentIndex, float u)
        {
            if (_controlPoints.Count < 2)
            {
                return Vector3.forward;
            }

            segmentIndex = Mathf.Clamp(segmentIndex, 0, SegmentCount - 1);
            u = Mathf.Clamp01(u);

            Vector3 p0 = GetControlPoint(segmentIndex - 1, segmentIndex, segmentIndex + 1);
            Vector3 p1 = _controlPoints[segmentIndex];
            Vector3 p2 = _controlPoints[segmentIndex + 1];
            Vector3 p3 = GetControlPoint(segmentIndex + 2, segmentIndex + 1, segmentIndex);

            return CalculateCatmullRomDerivative(p0, p1, p2, p3, u).normalized;
        }

        private Vector3 GetControlPoint(int targetIndex, int fallbackA, int fallbackB)
        {
            if (targetIndex >= 0 && targetIndex < _controlPoints.Count)
            {
                return _controlPoints[targetIndex];
            }

            if (targetIndex < 0)
            {
                // Extrapolate backwards before start
                Vector3 start = _controlPoints[0];
                Vector3 next = _controlPoints.Count > 1 ? _controlPoints[1] : start + Vector3.up;
                return start + (start - next);
            }
            else
            {
                // Extrapolate forwards after end
                int last = _controlPoints.Count - 1;
                Vector3 end = _controlPoints[last];
                Vector3 prev = last > 0 ? _controlPoints[last - 1] : end + Vector3.down;
                return end + (end - prev);
            }
        }

        public static Vector3 CalculateCatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            return 0.5f * (
                (2.0f * p1) +
                (-p0 + p2) * t +
                (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * t2 +
                (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * t3
            );
        }

        public static Vector3 CalculateCatmullRomDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t;

            return 0.5f * (
                (-p0 + p2) +
                2.0f * (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * t +
                3.0f * (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * t2
            );
        }
    }
}
