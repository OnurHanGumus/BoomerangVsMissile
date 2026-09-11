using Enums;
using Extensions;
using UnityEngine.Events;

namespace Signals
{
    public class PitchSignals : MonoSingleton<PitchSignals>
    {
        public UnityAction<PitchEnums> onPlayPitch = delegate { };
    }
}
