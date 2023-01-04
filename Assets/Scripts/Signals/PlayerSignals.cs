using Enums;
using Extentions;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Signals
{
    public class PlayerSignals : MonoSingleton<PlayerSignals>
    {
        public Func<Transform> onGetPlayer = delegate { return null; };
        public UnityAction onBoomerangHasReturned = delegate { };
        public UnityAction onBoomerangNextTarget = delegate { };
        public UnityAction onBoomerangBecomeInvisible = delegate { };
        public UnityAction onBoomerangThrowed = delegate { };
        public UnityAction<PlayerAnimationStates> onChangePlayerAnimation = delegate { };
    }
}