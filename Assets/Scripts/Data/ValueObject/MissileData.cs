using System;
using UnityEngine;

namespace Data.ValueObject
{
    [Serializable]
    public class MissileData
    {
        [Header("Impact Feedback Settings")]
        public float ShakeStrength = 0.25f;
        public float ShakeDuration = 0.15f;
        public float HitStopDuration = 0.05f;

        [Header("Armor & Health Settings")]
        public int MaxHealth = 1;
        public bool IsArmored = false;

        [Header("Movement Settings")]
        public float Velocity = 2f;
    }
}
