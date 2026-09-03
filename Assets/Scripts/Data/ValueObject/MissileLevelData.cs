using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data.ValueObject
{
    [Serializable]
    public class MissileLevelData
    {
        public List<MissileSpawnData> MissileData;
    }
    [Serializable]
    public class MissileSpawnData
    {
        public int MissileCount = 10;
        public List<GameObject> MissilePrefabList;
        public List<float> PercentageList;
        public int MissileMaxVelocity = 10;
        public float MissileCreateOffset = 2;
    }
}