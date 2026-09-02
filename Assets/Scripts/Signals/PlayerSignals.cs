using Enums;
using Extensions;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Signals
{
    public class PlayerSignals : MonoSingleton<PlayerSignals>
    {
        public Func<Transform> onGetPlayer = delegate { return null; };
        public UnityAction<PlayerAnimationStates> onChangePlayerAnimation = delegate { };
        public UnityAction<PlayerAnimationStates> onResetAnimation = delegate { };
        public UnityAction<float> onSetAnimationSpeed = delegate { };
        public UnityAction onResetAnimator = delegate { };
    }
}