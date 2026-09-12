using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;
using UnityEngine.Events;
using Extensions;

namespace Signals
{
    public class SaveSignals : MonoSingleton<SaveSignals>
    {
        public UnityAction<int, SaveLoadStates, SaveFiles> onSave = delegate { };
        public UnityAction<int, SaveLoadStates, SaveFiles> onChangeSoundState = delegate { };
        public UnityAction<int, SaveLoadStates, SaveFiles> onChangePitchState = delegate { };
        public UnityAction<List<int>, SaveLoadStates, SaveFiles> onBuyItem = delegate { };
        public UnityAction<List<int>> onInitializeBoughtItems = delegate { };

        public Func<SaveLoadStates, SaveFiles, int> onGetScore = delegate { return 0; };
        public Func<SaveLoadStates, SaveFiles, int> onGetSoundState = delegate { return 1; };
        public Func<SaveLoadStates, SaveFiles, int> onGetPitchState = delegate { return 1; };
        //public Func<SaveLoadStates, SaveFiles, List<int>> onGetItem = delegate { return null; };




    }
}