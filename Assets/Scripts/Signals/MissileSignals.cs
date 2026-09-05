using Enums;
using Extensions;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Signals
{
    public class MissileSignals : MonoSingleton<MissileSignals>
    {
        public UnityAction<float> onMissileDestroyed = delegate { };
        public UnityAction onBossMissileDestroyed = delegate { };
        public UnityAction onMissileArmorHit = delegate { };
        public UnityAction<int> onClusterSplit = delegate { };
    }
}