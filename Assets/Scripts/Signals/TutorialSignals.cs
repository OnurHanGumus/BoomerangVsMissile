using Enums;
using Extentions;
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