using System;
using System.Collections.Generic;

namespace Data.ValueObject
{
    [Serializable]
    public class WeeklyTaskStateData
    {
        public long WeekStartTicks = 0L;
        public long LastLoginDayTicks = 0L;
        public List<WeeklyTaskProgressData> Tasks = new List<WeeklyTaskProgressData>();
        public bool IsGrandPrizeClaimed = false;
        public int CurrentWinStreak = 0;
    }
}
