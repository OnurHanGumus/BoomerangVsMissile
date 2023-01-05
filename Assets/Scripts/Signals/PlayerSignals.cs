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
        public UnityAction<PlayerAnimationStates> onChangePlayerAnimation = delegate { };
        public UnityAction onAnimationSpeedIncreased = delegate { };
    }
}