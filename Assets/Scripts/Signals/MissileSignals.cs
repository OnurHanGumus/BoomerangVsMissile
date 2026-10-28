using Enums;
using Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Signals
{
    public class MissileSignals : MonoSingleton<MissileSignals>
    {
        public UnityAction<float> onMissileDestroyed = delegate { };
        public UnityAction onExploderMissileDestroyed = delegate { };
        public UnityAction onMissileArmorHit = delegate { };
        public UnityAction<int> onClusterSplit = delegate { };
        public UnityAction onExploderMissileCreated = delegate { };
        public UnityAction onAegisStateChanged = delegate { };
        public Func<Vector3, Transform, bool> onIsPointShielded = delegate { return false; };
    }
}
