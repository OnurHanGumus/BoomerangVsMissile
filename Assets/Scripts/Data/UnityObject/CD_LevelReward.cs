using Data.ValueObject;
using UnityEngine;

namespace Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_LevelReward", menuName = "Boomerang/CD_LevelReward", order = 3)]
    public class CD_LevelReward : ScriptableObject
    {
        public LevelRewardData Data = new LevelRewardData();

        public int GetRewardForLevel(int level)
        {
            return Data.GetRewardForLevel(level);
        }
    }
}
