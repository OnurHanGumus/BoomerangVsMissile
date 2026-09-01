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

        [Header("Boomerang Return Arc")]
        public float ReturnArcWidth = 1.2f;
        public float ReturnArcHeight = 0.8f;
        public float ReturnSpeedMultiplier = 1.25f;
    }
}