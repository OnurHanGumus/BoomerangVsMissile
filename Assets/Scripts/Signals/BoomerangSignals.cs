using Enums;
using Extentions;
using System;
using UnityEngine.Events;

namespace Signals
{
    public class BoomerangSignals : MonoSingleton<BoomerangSignals>
    {
        public UnityAction onBoomerangHasReturned = delegate { };
        public UnityAction onBoomerangNextTarget = delegate { };
        public UnityAction onBoomerangThrowed = delegate { };
        public UnityAction onBoomerangDisapeared = delegate { };
        public UnityAction onBoomerangRebuilded = delegate { };
        public UnityAction onBoomerangReturning = delegate { }; //when all missile points has reached, only one point that return to player remain.
        public UnityAction<int> onCombo = delegate { }; 
    }
}