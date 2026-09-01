using Enums;
using Extensions;
using System;
using UnityEngine.Events;

namespace Signals
{
    public class BoomerangSignals : MonoSingleton<BoomerangSignals>
    {
        public UnityAction onBoomerangHasReturned = delegate { };
        public UnityAction onBoomerangNextTarget = delegate { };
        public UnityAction onBoomerangThrown = delegate { };
        public UnityAction onBoomerangDisappeared = delegate { };
        public UnityAction onBoomerangRebuilt = delegate { };
        public UnityAction onBoomerangReturning = delegate { }; //when all missile points has reached, only one point that return to player remain.
        public UnityAction<int> onCombo = delegate { }; 
        public UnityAction<int> onSelectBoomerang = delegate { }; 
    }
}