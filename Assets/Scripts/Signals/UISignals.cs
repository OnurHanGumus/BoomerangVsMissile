using Enums;
using Extensions;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Signals
{
    public class UISignals : MonoSingleton<UISignals>
    {
        public UnityAction<UIPanels> onOpenPanel;
        public UnityAction<UIPanels> onClosePanel;
        public UnityAction<bool> onCloseSuccessfulPanel;

        public UnityAction<ScoreTypeEnums, int> onSetChangedText;
    }
}