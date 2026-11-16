using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Data.ValueObject
{
    [Serializable]
    public class LevelRewardTierData
    {
        public BoomerangTierEnums Tier;
        [Min(0)] public int MinLevel;
        [Min(0)] public int CoinReward;
    }

    [Serializable]
    public class LevelRewardData
    {
        public List<LevelRewardTierData> RewardTiers = new List<LevelRewardTierData>
        {
            new LevelRewardTierData { Tier = BoomerangTierEnums.Tier1, MinLevel = 0, CoinReward = 10 },
            new LevelRewardTierData { Tier = BoomerangTierEnums.Tier2, MinLevel = 35, CoinReward = 25 },
            new LevelRewardTierData { Tier = BoomerangTierEnums.Tier3, MinLevel = 160, CoinReward = 50 },
            new LevelRewardTierData { Tier = BoomerangTierEnums.Tier4, MinLevel = 400, CoinReward = 100 }
        };

        public int GetRewardForLevel(int level)
        {
            int selectedReward = 0;
            int highestMatchedMinLevel = -1;

            for (int i = 0; i < RewardTiers.Count; i++)
            {
                LevelRewardTierData tierData = RewardTiers[i];
                if (level >= tierData.MinLevel && tierData.MinLevel >= highestMatchedMinLevel)
                {
                    highestMatchedMinLevel = tierData.MinLevel;
                    selectedReward = tierData.CoinReward;
                }
            }

            return selectedReward;
        }
    }
}
