using System;
using Extensions;
using UnityEngine.Events;

namespace Signals
{
    public class EnergySignals : MonoSingleton<EnergySignals>
    {
        public Func<int, bool> onTryConsumeEnergy = delegate { return true; };
        public UnityAction<int> onAddEnergy = delegate { };

        public Func<int> onGetCurrentEnergy = delegate { return 5; };
        public Func<int> onGetMaxEnergy = delegate { return 5; };
        public Func<int> onGetRemainingSeconds = delegate { return 0; };

        public UnityAction<int, int> onEnergyChanged = delegate { };
        public UnityAction<int> onEnergyTimerUpdated = delegate { };
        public UnityAction onInsufficientEnergy = delegate { };
    }
}
