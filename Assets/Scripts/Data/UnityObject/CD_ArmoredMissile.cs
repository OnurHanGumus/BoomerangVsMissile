using Data.ValueObject;
using UnityEngine;

namespace Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_ArmoredMissile", menuName = "Picker3D/CD_ArmoredMissile", order = 1)]
    public class CD_ArmoredMissile : ScriptableObject
    {
        public ArmoredMissileData Data;
    }
}
