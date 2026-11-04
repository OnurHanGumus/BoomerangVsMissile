using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Data.ValueObject
{
    [Serializable]
    public class WeeklyTasksData
    {
        [Header("Global Settings")]
        [Range(0.2f, 1.0f)]
        [Tooltip("Minimum target multiplier when a player opens the game late in the week.")]
        public float MinLateJoinScale = 0.4f;

        [Tooltip("Level divisor for target scaling: 1 + (playerLevel / LevelScaleDivisor).")]
        [Min(50f)] public float LevelScaleDivisor = 250f;
        [Range(1.0f, 3.0f)] public float MaxLevelScaleMultiplier = 1.75f;

        [Header("Reward Settings (Coins)")]
        public int EasyRewardMin = 50;
        public int EasyRewardMax = 100;
        public int MediumRewardMin = 150;
        public int MediumRewardMax = 300;
        public int HardRewardMin = 400;
        public int HardRewardMax = 600;
        [Min(5)] public int RewardRoundStep = 10;
        public int GrandPrizeReward = 1500;

        [Header("Combo Tier Index Ranges (CD_Comments index)")]
        public int EasyComboIndexMin = 1;
        public int EasyComboIndexMax = 2;
        public int MediumComboIndexMin = 2;
        public int MediumComboIndexMax = 3;
        public int HardComboIndexMin = 3;
        public int HardComboIndexMax = 4;

        [Header("Task Definitions")]
        public List<WeeklyTaskDefinitionData> Definitions = CreateDefaultDefinitions();

        [Header("Missile Display Names")]
        public List<MissileDisplayData> MissileDisplayList = CreateDefaultMissileDisplays();

        public WeeklyTaskDefinitionData GetDefinition(WeeklyTaskType type)
        {
            if (Definitions != null)
            {
                for (int i = 0; i < Definitions.Count; i++)
                {
                    if (Definitions[i] != null && Definitions[i].Type == type)
                    {
                        return Definitions[i];
                    }
                }
            }
            return null;
        }

        public string GetMissileDisplayName(MissileEnums type)
        {
            if (MissileDisplayList != null)
            {
                for (int i = 0; i < MissileDisplayList.Count; i++)
                {
                    if (MissileDisplayList[i] != null && MissileDisplayList[i].Type == type && !string.IsNullOrEmpty(MissileDisplayList[i].DisplayName))
                    {
                        return MissileDisplayList[i].DisplayName;
                    }
                }
            }

            return GetFallbackMissileDisplayName(type);
        }

        public static string GetFallbackMissileDisplayName(MissileEnums type)
        {
            switch (type)
            {
                case MissileEnums.Missile_1_Default: return "Standard Missiles";
                case MissileEnums.Missile_2_Small: return "Fast Small Missiles";
                case MissileEnums.Missile_3_Exploder: return "Exploder Missiles";
                case MissileEnums.Missile_4_Armored: return "Armored Missiles";
                case MissileEnums.Missile_5_Cluster: return "Cluster Missiles";
                case MissileEnums.Missile_6_Cluster_Small: return "Mini Cluster Missiles";
                case MissileEnums.Missile_7_Armored_v2: return "Heavy Armored Mk.II Missiles";
                case MissileEnums.Missile_8_Cluster_v2: return "Cluster Mk.II Missiles";
                case MissileEnums.Missile_9_Armored_v3: return "Armored Mk.III Missiles";
                case MissileEnums.Missile_10_Armored_v4: return "Juggernaut Armored Missiles";
                case MissileEnums.Missile_11_Zigzag: return "Zigzag Missiles";
                case MissileEnums.Missile_12_Default_v2: return "Ballistic Mk.II Missiles";
                case MissileEnums.Missile_13_Default_v3: return "Ballistic Mk.III Missiles";
                case MissileEnums.Missile_14_Default_v4: return "Apex Ballistic Missiles";
                case MissileEnums.Missile_15_Aegis: return "Aegis Shield Missiles";
                case MissileEnums.Missile_16_Aegis_v2: return "Aegis Mk.II Missiles";
                case MissileEnums.Missile_17_Reward: return "Reward Supply Missiles";
                default: return type.ToString().Replace("_", " ");
            }
        }

        public void GetRewardRange(WeeklyTaskDifficulty difficulty, out int min, out int max)
        {
            switch (difficulty)
            {
                case WeeklyTaskDifficulty.Easy:
                    min = EasyRewardMin;
                    max = EasyRewardMax;
                    break;
                case WeeklyTaskDifficulty.Medium:
                    min = MediumRewardMin;
                    max = MediumRewardMax;
                    break;
                case WeeklyTaskDifficulty.Hard:
                    min = HardRewardMin;
                    max = HardRewardMax;
                    break;
                default:
                    min = EasyRewardMin;
                    max = EasyRewardMax;
                    break;
            }

            if (max < min)
            {
                max = min;
            }
        }

        public void GetComboIndexRange(WeeklyTaskDifficulty difficulty, out int min, out int max)
        {
            switch (difficulty)
            {
                case WeeklyTaskDifficulty.Easy:
                    min = EasyComboIndexMin;
                    max = EasyComboIndexMax;
                    break;
                case WeeklyTaskDifficulty.Medium:
                    min = MediumComboIndexMin;
                    max = MediumComboIndexMax;
                    break;
                case WeeklyTaskDifficulty.Hard:
                    min = HardComboIndexMin;
                    max = HardComboIndexMax;
                    break;
                default:
                    min = EasyComboIndexMin;
                    max = EasyComboIndexMax;
                    break;
            }

            if (max < min)
            {
                max = min;
            }
        }

        public static List<WeeklyTaskDefinitionData> CreateDefaultDefinitions()
        {
            return new List<WeeklyTaskDefinitionData>
            {
                new WeeklyTaskDefinitionData(
                    WeeklyTaskType.LoginDays, WeeklyTaskCategory.Engagement, 10,
                    "Enter the game on {0} days",
                    3, 5, 5, 7, 7, 7),
                new WeeklyTaskDefinitionData(
                    WeeklyTaskType.CompleteLevels, WeeklyTaskCategory.Engagement, 14,
                    "Complete {0} levels",
                    4, 7, 10, 16, 20, 30),
                new WeeklyTaskDefinitionData(
                    WeeklyTaskType.WinStreak, WeeklyTaskCategory.Skill, 10,
                    "Win {0} levels in a row",
                    2, 3, 4, 6, 7, 10),
                new WeeklyTaskDefinitionData(
                    WeeklyTaskType.SpinFortuneWheel, WeeklyTaskCategory.Engagement, 10,
                    "Spin the Fortune Wheel {0} times",
                    1, 2, 3, 4, 5, 6),
                new WeeklyTaskDefinitionData(
                    WeeklyTaskType.DestroyMissiles, WeeklyTaskCategory.Combat, 15,
                    "Destroy {0} missiles",
                    60, 120, 180, 300, 350, 550),
                new WeeklyTaskDefinitionData(
                    WeeklyTaskType.DestroyMissileType, WeeklyTaskCategory.Combat, 18,
                    "Explode {0} {1}",
                    15, 30, 35, 65, 75, 120),
                new WeeklyTaskDefinitionData(
                    WeeklyTaskType.BreakArmor, WeeklyTaskCategory.Combat, 12,
                    "Break {0} missile armor plates",
                    10, 20, 25, 45, 55, 90),
                new WeeklyTaskDefinitionData(
                    WeeklyTaskType.CollectRewardBuff, WeeklyTaskCategory.Combat, 10,
                    "Collect {0} Reward Missile buffs",
                    2, 4, 5, 9, 10, 15),
                new WeeklyTaskDefinitionData(
                    WeeklyTaskType.ReachCombo, WeeklyTaskCategory.Skill, 14,
                    "Reach \"{1}\" combo {0} times",
                    4, 7, 4, 7, 3, 5),
                new WeeklyTaskDefinitionData(
                    WeeklyTaskType.NoRecallLevels, WeeklyTaskCategory.Skill, 11,
                    "Complete {0} levels without Emergency Recall",
                    2, 4, 5, 8, 9, 14),
                new WeeklyTaskDefinitionData(
                    WeeklyTaskType.CompleteLevelsWithBoomerang, WeeklyTaskCategory.Boomerang, 18,
                    "Pass {0} levels with \"{1}\"",
                    2, 4, 4, 7, 8, 12),
                new WeeklyTaskDefinitionData(
                    WeeklyTaskType.CompleteLevelsWithDifferentBoomerangs, WeeklyTaskCategory.Boomerang, 10,
                    "Pass levels using {0} different boomerangs",
                    2, 2, 2, 3, 3, 4),
            };
        }

        public static List<MissileDisplayData> CreateDefaultMissileDisplays()
        {
            var list = new List<MissileDisplayData>();
            var values = (MissileEnums[])Enum.GetValues(typeof(MissileEnums));
            for (int i = 0; i < values.Length; i++)
            {
                list.Add(new MissileDisplayData(values[i], GetFallbackMissileDisplayName(values[i])));
            }
            return list;
        }
    }
}
