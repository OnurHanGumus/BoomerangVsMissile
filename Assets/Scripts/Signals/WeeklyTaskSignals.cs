using System;
using System.Collections.Generic;
using Data.ValueObject;
using Extensions;
using UnityEngine.Events;

namespace Signals
{
    public class WeeklyTaskSignals : MonoSingleton<WeeklyTaskSignals>
    {
        public UnityAction onWeeklyTasksRefreshed = delegate { };
        public UnityAction<int> onTaskProgressChanged = delegate { };
        public UnityAction<int> onTaskCompleted = delegate { };
        public UnityAction<int> onClaimTask = delegate { };
        public UnityAction onClaimGrandPrize = delegate { };

        public Func<List<WeeklyTaskProgressData>> onGetTasks = delegate { return new List<WeeklyTaskProgressData>(); };
        public Func<int, string> onGetTaskDescription = delegate { return string.Empty; };
        public Func<bool> onHasClaimable = delegate { return false; };
        public Func<bool> onIsGrandPrizeClaimable = delegate { return false; };
        public Func<bool> onIsGrandPrizeClaimed = delegate { return false; };
        public Func<int> onGetGrandPrizeReward = delegate { return 1500; };
        public Func<long> onGetRemainingSeconds = delegate { return 0L; };
    }
}
