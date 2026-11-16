using Extensions;
using System;
using UnityEngine.Events;

namespace Signals
{
    public class AdSignals : MonoSingleton<AdSignals>
    {
        #region Self Variables

        #region Public Variables
        public UnityAction<UnityAction, UnityAction> onShowRewardedAd = delegate { };
        public Func<bool> onIsRewardedAdReady = delegate { return false; };
        public UnityAction onRewardedAdCompleted = delegate { };
        #endregion

        #endregion
    }
}
