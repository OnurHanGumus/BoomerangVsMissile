using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data.ValueObject
{
    [Serializable]
    public class MissileLevelData
    {
        public List<MissileData> MissileData;
        public List<float> PercentageList;

    }
    [Serializable]
    public class MissileData
    {
        public int MissileCount = 10;
        public List<GameObject> MissilePrefabList;
        public int MissileMaxVelocity = 10;
        public float MissileCreateOffset = 2;
    }
}