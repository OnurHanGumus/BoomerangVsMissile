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
    }
}