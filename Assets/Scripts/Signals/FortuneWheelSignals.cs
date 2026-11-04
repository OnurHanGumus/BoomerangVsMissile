using System;
using Extensions;
using UnityEngine.Events;

namespace Signals
{
    public class FortuneWheelSignals : MonoSingleton<FortuneWheelSignals>
    {
        public UnityAction onOpenFortuneWheel = delegate { };
        public UnityAction onCloseFortuneWheel = delegate { };
        public UnityAction onWheelSpinStarted = delegate { };
        public UnityAction<int> onWheelSpinCompleted = delegate { };
        public Func<bool> onGetIsDailyWheelAvailable = delegate { return true; };
        public Func<long> onGetRemainingCooldownSeconds = delegate { return 0L; };
        public UnityAction onDailyWheelCooldownChanged = delegate { };
    }
}

