using System;
using UnityEngine;

namespace Data.ValueObject
{
    [Serializable]
    public class PlayerData
    {
        public float Speed = 5, AngularSpeed = 10;
        public int InitializePosX, InitializePosY;
        public float BoomerangInitPosX, BoomerangInitPosY;

        [Header("Boomerang Hand Grip Transform")]
        public Vector3 BoomerangHandLocalPosition = new Vector3(0.1360204f, 0.2610005f, -0.04199352f);
        public Vector3 BoomerangHandLocalEulerAngles = new Vector3(-9.304f, -9.275f, -107.476f);

        [Header("Boomerang Return Arc")]
        public float ReturnArcWidth = 1.2f;
        public float ReturnArcHeight = 0.8f;
        public float ReturnSpeedMultiplier = 1.25f;

        [Header("Charge Swing Settings")]
        public float MinReturnArcWidth = 0.2f;
        public float MaxReturnArcWidth = 5.0f;
        public float MinReturnArcHeight = 0.2f;
        public float MaxReturnArcHeight = 5.0f;
        public float ChargeDuration = 1.0f;

        [Header("Trajectory Preview Settings")]
        public int DirectSegments = 10;
        public int SplineSegments = 30;
        public float StartLineWidth = 0.08f;
        public float EndLineWidth = 0.03f;
        public Color PreviewInitialColor = new Color(0.2f, 0.9f, 1f, 0.85f);
        public Color PreviewMidColor = new Color(1f, 0.85f, 0.2f, 0.9f);
        public Color PreviewMaxColor = new Color(1f, 0.25f, 0.35f, 0.95f);

        [Header("Impact Camera Shake Settings")]
        public float ShakeDuration = 0.15f;
        public float ShakeStrength = 0.25f;
        public int ShakeVibrato = 14;
        public float ShakeRandomness = 90f;
    }
}