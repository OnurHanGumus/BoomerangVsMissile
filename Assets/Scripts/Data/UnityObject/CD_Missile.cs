using Data.ValueObject;
using System.Collections.Generic;
using UnityEngine;

namespace Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_Missile", menuName = "Picker3D/CD_Missile", order = 0)]
    public class CD_Missile : ScriptableObject
    {
        public MissileLevelData Data;
    }
}