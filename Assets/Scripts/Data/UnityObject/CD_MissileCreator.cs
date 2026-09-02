using Data.ValueObject;
using System.Collections.Generic;
using UnityEngine;

namespace Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_MissileCreator", menuName = "Picker3D/CD_MissileCreator", order = 0)]
    public class CD_MissileCreator : ScriptableObject
    {
        public MissileLevelData Data;
    }
}