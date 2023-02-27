using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;
using UnityEngine.Events;
using Extentions;

namespace Signals
{
    public class SaveSignals : MonoSingleton<SaveSignals>
    {
        public UnityAction<int, SaveLoadStates, SaveFiles> onSave = delegate { };
        public UnityAction<int, SaveLoadStates, SaveFiles> onChangeSoundState = delegate { };
        public UnityAction<List<int>, SaveLoadStates, SaveFiles> onBuyItem = delegate { };
        public UnityAction<List<int>> onInitializeBuyedItems = delegate { };

        public Func<SaveLoadStates, SaveFiles, int> onGetScore = delegate { return 0; };
        public Func<SaveLoadStates, SaveFiles, int> onGetSoundState = delegate { return 0; };
        //public Func<SaveLoadStates, SaveFiles, List<int>> onGetItem = delegate { return null; };




    }
}