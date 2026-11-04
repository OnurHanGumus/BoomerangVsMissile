using System;
using System.Collections.Generic;
using Data.UnityObject;
using Data.ValueObject;
using Enums;
using Signals;
using UnityEngine;

namespace Commands
{
    public class GenerateWeeklyTasksCommand
    {
        public static int ComputeDeterministicSeed(string installId, long weekStartTicks)
        {
            unchecked
            {
                uint hash = 2166136261u;
                string combined = (installId ?? "default") + "_" + weekStartTicks.ToString();
                for (int i = 0; i < combined.Length; i++)
                {
                    hash ^= combined[i];
                    hash *= 16777619u;
                }
                return (int)(hash & 0x7FFFFFFF);
            }
        }

        public List<WeeklyTaskProgressData> Generate(
            int seed,
            int playerLevel,
            int daysRemainingInWeek,
            WeeklyTasksData config,
            CD_Boomerangs boomerangsConfig,
            MissileLevelData missileLevelData,
            ComboCommentsData comboCommentsData)
        {
            if (config == null)
            {
                config = new WeeklyTasksData();
            }

            var rng = new System.Random(seed);
            var tasks = new List<WeeklyTaskProgressData>(6);
            var usedTypes = new HashSet<WeeklyTaskType>();

            List<BoomerangItemData> unlockedBoomerangs = GetUnlockedBoomerangs(playerLevel, boomerangsConfig);
            List<MissileEnums> unlockedMissiles = GetUnlockedMissileTypes(playerLevel, missileLevelData);
            bool hasArmoredUnlocked = HasAnyArmoredUnlocked(unlockedMissiles);
            bool hasRewardMissileUnlocked = unlockedMissiles.Contains(MissileEnums.Missile_17_Reward);
            int maxComboIndex = GetMaxComboIndex(comboCommentsData);

            int clampedDaysRemaining = Mathf.Clamp(daysRemainingInWeek, 1, 7);
            float lateJoinScale = Mathf.Max(config.MinLateJoinScale, clampedDaysRemaining / 7f);
            float levelScale = 1f;
            if (config.LevelScaleDivisor > 0f)
            {
                levelScale = Mathf.Clamp(1f + (playerLevel / config.LevelScaleDivisor), 1f, config.MaxLevelScaleMultiplier);
            }

            // Slot 1: Easy LoginDays (always present)
            var slot1 = BuildTask(
                WeeklyTaskType.LoginDays,
                WeeklyTaskDifficulty.Easy,
                rng,
                config,
                unlockedBoomerangs,
                unlockedMissiles,
                maxComboIndex,
                clampedDaysRemaining,
                lateJoinScale,
                levelScale);
            tasks.Add(slot1);
            usedTypes.Add(WeeklyTaskType.LoginDays);

            // Slot 2: Easy (Engagement / Combat)
            var slot2Pool = FilterEligibleDefinitions(
                config,
                usedTypes,
                unlockedBoomerangs,
                unlockedMissiles,
                hasArmoredUnlocked,
                hasRewardMissileUnlocked,
                WeeklyTaskCategory.Engagement,
                WeeklyTaskCategory.Combat);
            WeeklyTaskType slot2Type = PickWeightedType(slot2Pool, rng, WeeklyTaskType.CompleteLevels);
            tasks.Add(BuildTask(slot2Type, WeeklyTaskDifficulty.Easy, rng, config, unlockedBoomerangs, unlockedMissiles, maxComboIndex, clampedDaysRemaining, lateJoinScale, levelScale));
            usedTypes.Add(slot2Type);

            // Slot 3: Medium (Combat)
            var slot3Pool = FilterEligibleDefinitions(
                config,
                usedTypes,
                unlockedBoomerangs,
                unlockedMissiles,
                hasArmoredUnlocked,
                hasRewardMissileUnlocked,
                WeeklyTaskCategory.Combat);
            WeeklyTaskType slot3Type = PickWeightedType(slot3Pool, rng, WeeklyTaskType.DestroyMissiles);
            tasks.Add(BuildTask(slot3Type, WeeklyTaskDifficulty.Medium, rng, config, unlockedBoomerangs, unlockedMissiles, maxComboIndex, clampedDaysRemaining, lateJoinScale, levelScale));
            usedTypes.Add(slot3Type);

            // Slot 4: Medium (Boomerang)
            var slot4Pool = FilterEligibleDefinitions(
                config,
                usedTypes,
                unlockedBoomerangs,
                unlockedMissiles,
                hasArmoredUnlocked,
                hasRewardMissileUnlocked,
                WeeklyTaskCategory.Boomerang);
            WeeklyTaskType slot4Type = PickWeightedType(slot4Pool, rng, WeeklyTaskType.CompleteLevelsWithBoomerang);
            tasks.Add(BuildTask(slot4Type, WeeklyTaskDifficulty.Medium, rng, config, unlockedBoomerangs, unlockedMissiles, maxComboIndex, clampedDaysRemaining, lateJoinScale, levelScale));
            usedTypes.Add(slot4Type);

            // Slot 5: Medium (Skill)
            var slot5Pool = FilterEligibleDefinitions(
                config,
                usedTypes,
                unlockedBoomerangs,
                unlockedMissiles,
                hasArmoredUnlocked,
                hasRewardMissileUnlocked,
                WeeklyTaskCategory.Skill);
            WeeklyTaskType slot5Type = PickWeightedType(slot5Pool, rng, WeeklyTaskType.ReachCombo);
            tasks.Add(BuildTask(slot5Type, WeeklyTaskDifficulty.Medium, rng, config, unlockedBoomerangs, unlockedMissiles, maxComboIndex, clampedDaysRemaining, lateJoinScale, levelScale));
            usedTypes.Add(slot5Type);

            // Slot 6: Hard (Any unused eligible category)
            var slot6Pool = FilterEligibleDefinitions(
                config,
                usedTypes,
                unlockedBoomerangs,
                unlockedMissiles,
                hasArmoredUnlocked,
                hasRewardMissileUnlocked,
                WeeklyTaskCategory.Engagement,
                WeeklyTaskCategory.Combat,
                WeeklyTaskCategory.Boomerang,
                WeeklyTaskCategory.Skill);
            WeeklyTaskType slot6Type = PickWeightedType(slot6Pool, rng, WeeklyTaskType.CompleteLevels);
            tasks.Add(BuildTask(slot6Type, WeeklyTaskDifficulty.Hard, rng, config, unlockedBoomerangs, unlockedMissiles, maxComboIndex, clampedDaysRemaining, lateJoinScale, levelScale));
            usedTypes.Add(slot6Type);

            return tasks;
        }

        private List<WeeklyTaskDefinitionData> FilterEligibleDefinitions(
            WeeklyTasksData config,
            HashSet<WeeklyTaskType> usedTypes,
            List<BoomerangItemData> unlockedBoomerangs,
            List<MissileEnums> unlockedMissiles,
            bool hasArmoredUnlocked,
            bool hasRewardMissileUnlocked,
            params WeeklyTaskCategory[] allowedCategories)
        {
            var result = new List<WeeklyTaskDefinitionData>();
            if (config.Definitions == null)
            {
                return result;
            }

            for (int i = 0; i < config.Definitions.Count; i++)
            {
                var def = config.Definitions[i];
                if (def == null || usedTypes.Contains(def.Type))
                {
                    continue;
                }

                bool categoryMatch = false;
                for (int c = 0; c < allowedCategories.Length; c++)
                {
                    if (def.Category == allowedCategories[c])
                    {
                        categoryMatch = true;
                        break;
                    }
                }

                if (!categoryMatch)
                {
                    continue;
                }

                if (!IsTypeEligible(def.Type, unlockedBoomerangs, unlockedMissiles, hasArmoredUnlocked, hasRewardMissileUnlocked))
                {
                    continue;
                }

                result.Add(def);
            }

            return result;
        }

        private bool IsTypeEligible(
            WeeklyTaskType type,
            List<BoomerangItemData> unlockedBoomerangs,
            List<MissileEnums> unlockedMissiles,
            bool hasArmoredUnlocked,
            bool hasRewardMissileUnlocked)
        {
            switch (type)
            {
                case WeeklyTaskType.LoginDays:
                    return false; // Reserved for slot 1
                case WeeklyTaskType.DestroyMissileType:
                    return unlockedMissiles != null && unlockedMissiles.Count > 0;
                case WeeklyTaskType.BreakArmor:
                    return hasArmoredUnlocked;
                case WeeklyTaskType.CollectRewardBuff:
                    return hasRewardMissileUnlocked;
                case WeeklyTaskType.CompleteLevelsWithBoomerang:
                    return unlockedBoomerangs != null && unlockedBoomerangs.Count > 0;
                case WeeklyTaskType.CompleteLevelsWithDifferentBoomerangs:
                    return unlockedBoomerangs != null && unlockedBoomerangs.Count >= 2;
                default:
                    return true;
            }
        }

        private WeeklyTaskType PickWeightedType(List<WeeklyTaskDefinitionData> pool, System.Random rng, WeeklyTaskType fallback)
        {
            if (pool == null || pool.Count == 0)
            {
                return fallback;
            }

            int totalWeight = 0;
            for (int i = 0; i < pool.Count; i++)
            {
                totalWeight += Mathf.Max(1, pool[i].Weight);
            }

            int roll = rng.Next(0, totalWeight);
            int cumulative = 0;
            for (int i = 0; i < pool.Count; i++)
            {
                cumulative += Mathf.Max(1, pool[i].Weight);
                if (roll < cumulative)
                {
                    return pool[i].Type;
                }
            }

            return pool[pool.Count - 1].Type;
        }

        private WeeklyTaskProgressData BuildTask(
            WeeklyTaskType type,
            WeeklyTaskDifficulty difficulty,
            System.Random rng,
            WeeklyTasksData config,
            List<BoomerangItemData> unlockedBoomerangs,
            List<MissileEnums> unlockedMissiles,
            int maxComboIndex,
            int daysRemainingInWeek,
            float lateJoinScale,
            float levelScale)
        {
            var def = config.GetDefinition(type);
            int targetMin = 3;
            int targetMax = 5;
            if (def != null)
            {
                def.GetTargetRange(difficulty, out targetMin, out targetMax);
            }

            int rawTarget = rng.Next(targetMin, targetMax + 1);
            int finalTarget = rawTarget;
            int paramId = -1;

            switch (type)
            {
                case WeeklyTaskType.LoginDays:
                    finalTarget = Mathf.Clamp(rawTarget, 1, daysRemainingInWeek);
                    break;

                case WeeklyTaskType.DestroyMissiles:
                case WeeklyTaskType.DestroyMissileType:
                case WeeklyTaskType.BreakArmor:
                case WeeklyTaskType.CompleteLevels:
                    finalTarget = Mathf.Max(1, Mathf.RoundToInt(rawTarget * levelScale * lateJoinScale));
                    if (type == WeeklyTaskType.DestroyMissiles || type == WeeklyTaskType.DestroyMissileType)
                    {
                        finalTarget = RoundToStep(finalTarget, 5);
                    }
                    break;

                case WeeklyTaskType.CompleteLevelsWithDifferentBoomerangs:
                    int maxPossibleBoomerangs = unlockedBoomerangs != null ? unlockedBoomerangs.Count : 1;
                    finalTarget = Mathf.Clamp(rawTarget, 1, Mathf.Max(1, maxPossibleBoomerangs));
                    break;

                default:
                    finalTarget = Mathf.Max(1, Mathf.RoundToInt(rawTarget * lateJoinScale));
                    break;
            }

            if (type == WeeklyTaskType.DestroyMissileType)
            {
                if (unlockedMissiles != null && unlockedMissiles.Count > 0)
                {
                    MissileEnums chosenMissile = unlockedMissiles[rng.Next(0, unlockedMissiles.Count)];
                    paramId = (int)chosenMissile;
                }
                else
                {
                    paramId = (int)MissileEnums.Missile_1_Default;
                }
            }
            else if (type == WeeklyTaskType.CompleteLevelsWithBoomerang)
            {
                if (unlockedBoomerangs != null && unlockedBoomerangs.Count > 0)
                {
                    BoomerangItemData chosenBoomerang = unlockedBoomerangs[rng.Next(0, unlockedBoomerangs.Count)];
                    paramId = chosenBoomerang.Id;
                }
                else
                {
                    paramId = 0;
                }
            }
            else if (type == WeeklyTaskType.ReachCombo)
            {
                config.GetComboIndexRange(difficulty, out int comboMin, out int comboMax);
                comboMin = Mathf.Clamp(comboMin, 0, maxComboIndex);
                comboMax = Mathf.Clamp(comboMax, comboMin, maxComboIndex);
                paramId = rng.Next(comboMin, comboMax + 1);
            }

            config.GetRewardRange(difficulty, out int rewardMin, out int rewardMax);
            int rawReward = rng.Next(rewardMin, rewardMax + 1);
            int step = Mathf.Max(1, config.RewardRoundStep);
            int reward = Mathf.Max(step, RoundToStep(rawReward, step));

            return new WeeklyTaskProgressData
            {
                Type = type,
                Difficulty = difficulty,
                ParamId = paramId,
                Target = Mathf.Max(1, finalTarget),
                Progress = 0,
                Reward = reward,
                IsClaimed = false,
                UniqueIds = new List<int>()
            };
        }

        private static int RoundToStep(int value, int step)
        {
            if (step <= 1)
            {
                return Mathf.Max(1, value);
            }
            return Mathf.Max(step, Mathf.RoundToInt((float)value / step) * step);
        }

        private List<BoomerangItemData> GetUnlockedBoomerangs(int playerLevel, CD_Boomerangs boomerangsConfig)
        {
            var result = new List<BoomerangItemData>();
            if (boomerangsConfig == null || boomerangsConfig.Boomerangs == null)
            {
                return result;
            }

            for (int i = 0; i < boomerangsConfig.Boomerangs.Count; i++)
            {
                var item = boomerangsConfig.Boomerangs[i];
                if (item == null)
                {
                    continue;
                }

                bool isTierUnlocked = false;
                if (BoomerangSignals.Instance != null)
                {
                    isTierUnlocked = BoomerangSignals.Instance.onIsTierUnlocked(item.Tier);
                }
                else
                {
                    isTierUnlocked = playerLevel >= boomerangsConfig.GetRequiredUnlockLevel(item.Tier);
                }

                int unlockLevel = item.GetUnlockLevel(boomerangsConfig);
                if (isTierUnlocked && playerLevel >= unlockLevel)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        private List<MissileEnums> GetUnlockedMissileTypes(int playerLevel, MissileLevelData missileLevelData)
        {
            var set = new HashSet<MissileEnums>();
            var list = new List<MissileEnums>();

            if (missileLevelData == null || missileLevelData.MissileData == null || missileLevelData.MissileData.Count == 0)
            {
                list.Add(MissileEnums.Missile_1_Default);
                return list;
            }

            int maxIndex = Mathf.Clamp(playerLevel, 0, missileLevelData.MissileData.Count - 1);
            for (int i = 0; i <= maxIndex; i++)
            {
                var spawnData = missileLevelData.MissileData[i];
                if (spawnData == null || spawnData.MissileTypeList == null)
                {
                    continue;
                }

                for (int m = 0; m < spawnData.MissileTypeList.Count; m++)
                {
                    MissileEnums mType = spawnData.MissileTypeList[m];
                    if (mType == MissileEnums.Missile_6_Cluster_Small)
                    {
                        continue;
                    }

                    if (set.Add(mType))
                    {
                        list.Add(mType);
                    }
                }
            }

            if (list.Count == 0)
            {
                list.Add(MissileEnums.Missile_1_Default);
            }

            return list;
        }

        private static bool HasAnyArmoredUnlocked(List<MissileEnums> unlockedMissiles)
        {
            if (unlockedMissiles == null)
            {
                return false;
            }

            for (int i = 0; i < unlockedMissiles.Count; i++)
            {
                switch (unlockedMissiles[i])
                {
                    case MissileEnums.Missile_4_Armored:
                    case MissileEnums.Missile_7_Armored_v2:
                    case MissileEnums.Missile_9_Armored_v3:
                    case MissileEnums.Missile_10_Armored_v4:
                        return true;
                }
            }
            return false;
        }

        private static int GetMaxComboIndex(ComboCommentsData commentsData)
        {
            if (commentsData != null && commentsData.CommentsList != null && commentsData.CommentsList.Count > 0)
            {
                return Mathf.Min(5, commentsData.CommentsList.Count - 1);
            }
            return 4;
        }
    }
}
