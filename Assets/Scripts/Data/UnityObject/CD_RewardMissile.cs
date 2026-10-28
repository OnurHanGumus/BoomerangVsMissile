using Data.ValueObject;
using UnityEngine;

namespace Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_RewardMissile", menuName = "Picker3D/CD_RewardMissile", order = 1)]
    public class CD_RewardMissile : ScriptableObject
    {
        public RewardMissileData Data;
    }
}
