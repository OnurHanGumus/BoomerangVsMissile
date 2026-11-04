using System;
using Enums;
using UnityEngine;

namespace Data.ValueObject
{
    [Serializable]
    public class WeeklyTaskDefinitionData
    {
        public WeeklyTaskType Type;
        public WeeklyTaskCategory Category;
        [Min(1)] public int Weight = 10;
        [Tooltip("Format string. {0} = Target count, {1} = Dynamic parameter name (boomerang, missile, or combo tier).")]
        public string DescriptionFormat = "Complete {0} tasks";

        [Header("Target Ranges per Difficulty")]
        public int EasyTargetMin = 3;
        public int EasyTargetMax = 5;
        public int MediumTargetMin = 8;
        public int MediumTargetMax = 15;
        public int HardTargetMin = 20;
        public int HardTargetMax = 35;

        public WeeklyTaskDefinitionData()
        {
        }

        public WeeklyTaskDefinitionData(
            WeeklyTaskType type,
            WeeklyTaskCategory category,
            int weight,
            string descriptionFormat,
            int easyMin, int easyMax,
            int medMin, int medMax,
            int hardMin, int hardMax)
        {
            Type = type;
            Category = category;
            Weight = weight;
            DescriptionFormat = descriptionFormat;
            EasyTargetMin = easyMin;
            EasyTargetMax = easyMax;
            MediumTargetMin = medMin;
            MediumTargetMax = medMax;
            HardTargetMin = hardMin;
            HardTargetMax = hardMax;
        }

        public void GetTargetRange(WeeklyTaskDifficulty difficulty, out int min, out int max)
        {
            switch (difficulty)
            {
                case WeeklyTaskDifficulty.Easy:
                    min = EasyTargetMin;
                    max = EasyTargetMax;
                    break;
                case WeeklyTaskDifficulty.Medium:
                    min = MediumTargetMin;
                    max = MediumTargetMax;
                    break;
                case WeeklyTaskDifficulty.Hard:
                    min = HardTargetMin;
                    max = HardTargetMax;
                    break;
                default:
                    min = EasyTargetMin;
                    max = EasyTargetMax;
                    break;
            }

            if (max < min)
            {
                max = min;
            }
        }
    }
}
