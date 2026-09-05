using System;
using Enums;
using UnityEngine;

namespace Data.ValueObject
{
    [Serializable]
    public class ClusterMissileData
    {
        public PoolEnums ClusterChildType = PoolEnums.Missile6;
        public int ClusterChildCount = 1;
        public float ClusterChildSpacing = 0.8f;
        public float ClusterSpawnDelay = 1f;
    }
}
