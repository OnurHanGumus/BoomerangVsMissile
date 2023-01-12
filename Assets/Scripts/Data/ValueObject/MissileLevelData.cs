using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data.ValueObject
{
    [Serializable]
    public class MissileLevelData
    {
        public List<MissileData> MissileData;
    }
    [Serializable]
    public class MissileData
    {
        public int MissileCount = 10;
        public List<GameObject> MissilePrefabList;
        public int MissileMaxVelocity = 10;
        public int MissileCreateOffset = 2;
    }
}