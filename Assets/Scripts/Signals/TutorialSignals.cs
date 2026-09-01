using Enums;
using Extensions;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Signals
{
    public class TutorialSignals : MonoSingleton<TutorialSignals>
    {
        public UnityAction<bool> onTutorialActive = delegate { };
        public UnityAction onTutorialSatisfied = delegate { };
    }
}