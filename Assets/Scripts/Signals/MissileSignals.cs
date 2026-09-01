using Enums;
using Extensions;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Signals
{
    public class MissileSignals : MonoSingleton<MissileSignals>
    {
        public UnityAction onMissileDestroyed = delegate { };
        public UnityAction onPinkMissileDestroyed = delegate { };
    }
}