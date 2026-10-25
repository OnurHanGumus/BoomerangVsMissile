using Data.ValueObject;
using UnityEngine;

namespace Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_ZigzagMissile", menuName = "Picker3D/CD_ZigzagMissile", order = 1)]
    public class CD_ZigzagMissile : ScriptableObject
    {
        public ZigzagMissileData Data;
    }
}
