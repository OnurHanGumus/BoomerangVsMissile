using Data.ValueObject;
using UnityEngine;

namespace Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_BossMissile", menuName = "Picker3D/CD_BossMissile", order = 1)]
    public class CD_BossMissile : ScriptableObject
    {
        public BossMissileData Data;
    }
}
