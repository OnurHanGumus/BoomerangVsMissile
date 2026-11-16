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
        public int Money
        {
            get { return _money; }
            set
            {
                _money = value;
                UISignals.Instance.onSetChangedText?.Invoke(ScoreTypeEnums.Money, Money);
            }
        }
        #endregion

        #region Serialized Variables

        #endregion

        #region Private Variables
        private ScoreData _data;
        private LevelRewardData _levelRewardData;
        private int _money;
        private int _lastLevelReward;
        #endregion

        #endregion

        private void Awake()
        {
            Init();
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void Init()
        {
            _levelRewardData = GetLevelRewardData();
            Money = SaveSignals.Instance.onGetScore(SaveLoadStates.Money, SaveFiles.SaveFile);
        }

        private LevelRewardData GetLevelRewardData() => Resources.Load<CD_LevelReward>("Data/CD_LevelReward").Data;

        #region Event Subscription

        private void SubscribeEvents()
        {
            ScoreSignals.Instance.onScoreIncrease += OnScoreIncrease;
            ScoreSignals.Instance.onScoreDecrease += OnScoreDecrease;
            ScoreSignals.Instance.onGetMoney += OnGetMoney;
            ScoreSignals.Instance.onGetLastLevelReward += OnGetLastLevelReward;
            CoreGameSignals.Instance.onLevelSuccessful += OnLevelSuccessful;
            CoreGameSignals.Instance.onNextLevel += OnNextLevel;
            CoreGameSignals.Instance.onRestartLevel += OnRestartLevel;
        }

        private void UnsubscribeEvents()
        {
            ScoreSignals.Instance.onScoreIncrease -= OnScoreIncrease;
            ScoreSignals.Instance.onScoreDecrease -= OnScoreDecrease;
            ScoreSignals.Instance.onGetMoney -= OnGetMoney;
            ScoreSignals.Instance.onGetLastLevelReward -= OnGetLastLevelReward;
            CoreGameSignals.Instance.onLevelSuccessful -= OnLevelSuccessful;
            CoreGameSignals.Instance.onNextLevel -= OnNextLevel;
            CoreGameSignals.Instance.onRestartLevel -= OnRestartLevel;
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        #endregion

        private void OnLevelSuccessful()
        {
            int currentLevel = LevelSignals.Instance.onGetLevelId();
            _lastLevelReward = _levelRewardData.GetRewardForLevel(currentLevel);
            OnScoreIncrease(ScoreTypeEnums.Money, _lastLevelReward);
            ScoreSignals.Instance.onLevelRewardGranted(_lastLevelReward);
        }

        private int OnGetLastLevelReward()
        {
            return _lastLevelReward;
        }

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
