using System;
using UnityEngine;

namespace Data.ValueObject
{
    [Serializable]
    public class ZigzagMissileData
    {
        [Header("Horizontal Motion")]
        public float HorizontalSpeed = 1.6f;
        public float TravelHalfWidth = 1.8f;
        public float ScreenEdgeMargin = 0.75f;
        public bool StartTowardScreenCenter = true;

        [Header("Obstacle Detection")]
        public float LookAheadDistance = 0.75f;
        public float BodyHalfWidth = 0.3f;
        public float CheckBelowHeight = 0.9f;
        public float CheckAboveHeight = 2.2f;
        public float FlipCooldown = 0.2f;

        [Header("Visual Lean")]
        public float LeanAngle = 20f;
        public float LeanSmoothTime = 0.14f;
    }
}
