using Enums;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

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
        public List<MissileEnums> MissileTypeList;
        public List<float> PercentageList;
        public int MissileMaxVelocity = 10;
        public float MissileCreateOffset = 2;
    }
}