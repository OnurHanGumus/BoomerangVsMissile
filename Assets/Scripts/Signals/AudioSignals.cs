using Enums;
using Extentions;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Signals
{
    public class AudioSignals : MonoSingleton<AudioSignals>
    {
        public UnityAction<AudioSoundEnums> onPlaySound = delegate { };
    }
}