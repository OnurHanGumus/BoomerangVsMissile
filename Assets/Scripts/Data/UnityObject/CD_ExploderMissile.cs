using Data.ValueObject;
using UnityEngine;

namespace Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_ExploderMissile", menuName = "Picker3D/CD_ExploderMissile", order = 1)]
    public class CD_ExploderMissile : ScriptableObject
    {
        public ExploderMissileData Data;
    }
}
