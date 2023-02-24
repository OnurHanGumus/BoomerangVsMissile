using System;
using System.Collections.Generic;
using Commands;
using Controllers;
using Data.UnityObject;
using Data.ValueObject;
using Extentions;
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
        private int _gem;
        public int Gem
        {
            get { return _gem; }
            set { _gem = value;
            UISignals.Instance.onSetChangedText?.Invoke(ScoreTypeEnums.Gem, Gem);
            }
        }


        #endregion

        #endregion

        private void Awake()
        {
            Init();
        }
        private void Init()
        {
            Gem = SaveSignals.Instance.onGetScore(SaveLoadStates.Gem, SaveFiles.SaveFile);
        }
        #region Event Subscription

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            ScoreSignals.Instance.onScoreIncrease += OnScoreIncrease;
            ScoreSignals.Instance.onScoreDecrease += OnScoreDecrease;
            ScoreSignals.Instance.onGetScore += OnGetScore;
            CoreGameSignals.Instance.onNextLevel += OnNextLevel;
            CoreGameSignals.Instance.onRestartLevel += OnRestartLevel;
        }

        private void UnsubscribeEvents()
        {
            ScoreSignals.Instance.onScoreIncrease -= OnScoreIncrease;
            ScoreSignals.Instance.onScoreDecrease -= OnScoreDecrease;
            ScoreSignals.Instance.onGetScore -= OnGetScore;
            CoreGameSignals.Instance.onNextLevel -= OnNextLevel;
            CoreGameSignals.Instance.onRestartLevel -= OnRestartLevel;
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        #endregion

        private void OnScoreIncrease(ScoreTypeEnums type, int amount)
        {
            Gem += amount;
        }

        private void OnScoreDecrease(ScoreTypeEnums type, int amount)
        {
            Gem -= amount;
            SaveSignals.Instance.onSaveScore(Gem, SaveLoadStates.Gem, SaveFiles.SaveFile);
        }

        private void OnNextLevel()
        {
            SaveSignals.Instance.onSaveScore(Gem, SaveLoadStates.Gem, SaveFiles.SaveFile);
        }
        private int OnGetScore()
        {
            return Gem;
        }

        private void OnRestartLevel()
        {
        }
    }
}