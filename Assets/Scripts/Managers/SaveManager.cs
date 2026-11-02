using System;
using System.Collections.Generic;
using Commands;
using Signals;
using UnityEngine;
using Enums;

namespace Managers
{
    public class SaveManager : MonoBehaviour
    {
        #region Self Variables
        #region Private Variables

        private LoadGameCommand _loadGameCommand;
        private SaveGameCommand _saveGameCommand;

        #endregion
        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }
        private void Init()
        {
            _loadGameCommand = new LoadGameCommand();
            _saveGameCommand = new SaveGameCommand();
        }

        #region Event Subscription

        private void SubscribeEvents()
        {
            SaveSignals.Instance.onSave += OnSaveData;
            SaveSignals.Instance.onSaveLong += OnSaveLong;
            SaveSignals.Instance.onChangeSoundState += OnSaveData;
            SaveSignals.Instance.onChangePitchState += OnSaveData;
            SaveSignals.Instance.onGetScore += OnGetData;
            SaveSignals.Instance.onGetIntWithDefault += OnGetIntWithDefault;
            SaveSignals.Instance.onGetLong += OnGetLong;
            SaveSignals.Instance.onGetSoundState += OnGetData;
            SaveSignals.Instance.onGetPitchState += OnGetData;
            SaveSignals.Instance.onBuyItem += OnSaveList;
        }

        private void UnsubscribeEvents()
        {
            if (SaveSignals.Instance != null)
            {
                SaveSignals.Instance.onSave -= OnSaveData;
                SaveSignals.Instance.onSaveLong -= OnSaveLong;
                SaveSignals.Instance.onChangeSoundState -= OnSaveData;
                SaveSignals.Instance.onChangePitchState -= OnSaveData;
                SaveSignals.Instance.onGetScore -= OnGetData;
                SaveSignals.Instance.onGetIntWithDefault -= OnGetIntWithDefault;
                SaveSignals.Instance.onGetLong -= OnGetLong;
                SaveSignals.Instance.onGetSoundState -= OnGetData;
                SaveSignals.Instance.onGetPitchState -= OnGetData;
                SaveSignals.Instance.onBuyItem -= OnSaveList;
            }
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        #endregion

        private void Start()
        {
            SendData();
        }

        private void OnSaveData(int value, SaveLoadStates saveType, SaveFiles saveFiles)
        {
            _saveGameCommand.OnSaveData(saveType, value, saveFiles.ToString());
        }

        private void OnSaveLong(long value, SaveLoadStates saveType, SaveFiles saveFiles)
        {
            _saveGameCommand.OnSaveLong(saveType, value, saveFiles.ToString());
        }

        private int OnGetData(SaveLoadStates state, SaveFiles file)
        {
            return _loadGameCommand.OnLoadGameData(state, file.ToString());
        }

        private int OnGetIntWithDefault(SaveLoadStates state, int defaultValue, SaveFiles file)
        {
            return _loadGameCommand.OnLoadGameDataWithDefault(state, defaultValue, file.ToString());
        }

        private long OnGetLong(SaveLoadStates state, SaveFiles file)
        {
            return _loadGameCommand.OnLoadLong(state, 0L, file.ToString());
        }

        private void OnSaveList(List<int> newList, SaveLoadStates saveType, SaveFiles saveFiles)
        {
            _saveGameCommand.OnSaveList(saveType, newList, saveFiles.ToString());
            SendData();
        }
        private List<int> OnGetList(SaveLoadStates saveType, SaveFiles saveFiles)
        {
            return _loadGameCommand.OnLoadGameList(saveType, saveFiles.ToString());
        }

        private void SendData()
        {
            SaveSignals.Instance.onInitializeBoughtItems?.Invoke(OnGetList(SaveLoadStates.BuyItem, SaveFiles.SaveFile));
        }
    }
}
