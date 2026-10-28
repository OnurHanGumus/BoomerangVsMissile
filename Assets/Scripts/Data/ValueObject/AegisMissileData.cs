using System;
using UnityEngine;

namespace Data.ValueObject
{
    [Serializable]
    public class AegisMissileData
    {
        [Header("Shield Bubble")]
        [Min(0.5f)] public float ShieldRadius = 3.0f;
        public float PulseSpeed = 3.0f;
        public float PulseScaleAmount = 0.03f;
    }
}
