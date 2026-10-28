using Data.ValueObject;
using Enums;
using Extensions;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Signals
{
    public class BoomerangSignals : MonoSingleton<BoomerangSignals>
    {
        public UnityAction onBoomerangHasReturned = delegate { };
        public UnityAction onBoomerangNextTarget = delegate { };
        public UnityAction onBoomerangThrown = delegate { };
        public UnityAction onBoomerangReturning = delegate { }; //when all missile points has reached, only one point that return to player remain.
        public UnityAction<int> onCombo = delegate { }; 
        public UnityAction<int> onSelectBoomerang = delegate { }; 
        public UnityAction<float, float, float, float> onSetChargedArc = delegate { }; 
        public UnityAction onBoomerangStatsChanged = delegate { };
        public UnityAction onEmergencyRecall = delegate { };
        public UnityAction onPlayerTouchedBoomerang = delegate { };
        public UnityAction<BoomerangStatEnums, RewardMissileData, Vector3> onRewardBuffTriggered = delegate { };
        public UnityAction<BoomerangStatEnums, int, float, float> onBuffTimerUpdated = delegate { };
    }
}
