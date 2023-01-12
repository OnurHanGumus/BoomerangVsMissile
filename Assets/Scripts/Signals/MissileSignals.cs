using Enums;
using Extentions;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Signals
{
    public class MissileSignals : MonoSingleton<MissileSignals>
    {
        public UnityAction onMissileDestroyed = delegate { };
    }
}