using System;
using UnityEngine;

namespace Data.ValueObject
{
    [Serializable]
    public class BossMissileData
    {
        public float ExplosionRadius = 4f;
        public float FallSpeed = 3.5f;
        public float PathCheckWidth = 0.8f;
    }
}
