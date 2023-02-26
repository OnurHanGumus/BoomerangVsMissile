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
        }
        private void Init()
        {
            _loadGameCommand = new LoadGameCommand();
            _saveGameCommand = new SaveGameCommand();
        }

        #region Event Subscription

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            SaveSignals.Instance.onSaveScore += OnSaveData;
            SaveSignals.Instance.onChangeSoundState += OnSaveData;
            SaveSignals.Instance.onGetScore += OnGetData;
            SaveSignals.Instance.onGetSoundState += OnGetData;
            SaveSignals.Instance.onBuyItem += OnSaveList;
        }

        private void UnsubscribeEvents()
        {
            SaveSignals.Instance.onSaveScore -= OnSaveData;
            SaveSignals.Instance.onChangeSoundState -= OnSaveData;
            SaveSignals.Instance.onGetScore -= OnGetData;
            SaveSignals.Instance.onGetSoundState -= OnGetData;
            SaveSignals.Instance.onBuyItem -= OnSaveList;
        }

        private void OnDisable()
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

        private int OnGetData(SaveLoadStates state, SaveFiles file)
        {
            return _loadGameCommand.OnLoadGameData(state, file.ToString());
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
            SaveSignals.Instance.onInitializeBuyedItems?.Invoke(OnGetList(SaveLoadStates.BuyItem, SaveFiles.SaveFile));
        }
    }
}