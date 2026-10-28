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
        public float LookAheadDistance = 0.45f;
        public float BodyHalfWidth = 0.3f;
        public float CheckBelowHeight = 1.0f;
        public float CheckAboveHeight = 0.5f;
        public float FlipCooldown = 0.2f;
        public float HysteresisMargin = 0.15f;

        [Header("Visual Lean")]
        public float LeanAngle = 20f;
        public float LeanSmoothTime = 0.14f;
    }
}
