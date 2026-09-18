using System;
using System.Collections.Generic;
using Commands;
using Controllers;
using Data.UnityObject;
using Data.ValueObject;
using Extensions;
using Keys;
using Signals;
using UnityEngine;
using Enums;

namespace Managers
{
    public class ScoreManager : MonoBehaviour
    {
        #region Self Variables

        #region Public Variables


        #endregion

        #region Serialized Variables


        #endregion

        #region Private Variables
        private ScoreData _data;
        private int _money;
        public int Money
        {
            get { return _money; }
            set { _money = value;
            UISignals.Instance.onSetChangedText?.Invoke(ScoreTypeEnums.Money, Money);
            }
        }


        #endregion

        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }
        private void Init()
        {
            Money = SaveSignals.Instance.onGetScore(SaveLoadStates.Money, SaveFiles.SaveFile);
        }
        #region Event Subscription

        private void SubscribeEvents()
        {
            ScoreSignals.Instance.onScoreIncrease += OnScoreIncrease;
            ScoreSignals.Instance.onScoreDecrease += OnScoreDecrease;
            ScoreSignals.Instance.onGetMoney += OnGetMoney;
            CoreGameSignals.Instance.onNextLevel += OnNextLevel;
            CoreGameSignals.Instance.onRestartLevel += OnRestartLevel;
        }

        #endregion

        private void OnScoreIncrease(ScoreTypeEnums type, int amount)
        {
            Money += amount;
            SaveSignals.Instance.onSave(Money, SaveLoadStates.Money, SaveFiles.SaveFile);
        }

        private void OnScoreDecrease(ScoreTypeEnums type, int amount)
        {
            Money -= amount;
            SaveSignals.Instance.onSave(Money, SaveLoadStates.Money, SaveFiles.SaveFile);
        }

        private void OnNextLevel()
        {
            SaveSignals.Instance.onSave(Money, SaveLoadStates.Money, SaveFiles.SaveFile);
        }
        private int OnGetMoney()
        {
            return Money;
        }

        private void OnRestartLevel()
        {
        }
    }
}