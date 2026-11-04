using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Data.ValueObject
{
    [Serializable]
    public class WeeklyTaskProgressData
    {
        public WeeklyTaskType Type;
        public WeeklyTaskDifficulty Difficulty;
        public int ParamId = -1;
        public int Target = 1;
        public int Progress = 0;
        public int Reward = 100;
        public bool IsClaimed = false;
        public List<int> UniqueIds = new List<int>();

        public bool IsCompleted => Progress >= Target;
        public bool IsClaimable => IsCompleted && !IsClaimed;

        public float NormalizedProgress => Target > 0 ? Mathf.Clamp01((float)Progress / Target) : 1f;
    }
}
