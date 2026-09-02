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
    }
}