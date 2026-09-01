using System;
using System.Collections.Generic;
using Commands;
using Controllers;
using Data.UnityObject;
using Data.ValueObject;
using Enums;
using Signals;
using UnityEngine;

namespace Managers
{
    public class TimeManager : MonoBehaviour
    {
        #region Self Variables

        #region Public Variables

        #endregion

        #region Serialized Variables
        #endregion

        #region Private Variables
        private TimeData _data;
        private bool _isLost = false;
        #endregion

        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }

        private void Init()
        {
            _data = GetData();
        }

        public TimeData GetData() => Resources.Load<CD_Time>("Data/CD_Time").Data;

        #region Event Subscription

        private void SubscribeEvents()
        {
            CoreGameSignals.Instance.onPlay += OnPlay;
            CoreGameSignals.Instance.onLevelFailed += OnLevelFailed;
            CoreGameSignals.Instance.onRestartLevel += OnRestartLevel;
            BoomerangSignals.Instance.onBoomerangDisappeared += OnBoomerangDisappeared;
            BoomerangSignals.Instance.onBoomerangRebuilt += OnBoomerangRebuilt;

            InputSignals.Instance.onClicking += OnClicking;
            InputSignals.Instance.onInputReleased += OnInputReleased;
        }

        #endregion
        private void OnPlay()
        {
            Time.timeScale = _data.NormalTimeScale;
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.BoomerangPanel);
        }

        private void OnBoomerangDisappeared()
        {
            if (_isLost == true)
            {
                return;
            }
            UISignals.Instance.onOpenPanel?.Invoke(UIPanels.BoomerangPanel);
            Time.timeScale = _data.MissingBoomerangTimeScale;
        }

        private void OnBoomerangRebuilt()
        {
            Time.timeScale = _data.NormalTimeScale;
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.BoomerangPanel);
        }

        private void OnLevelFailed()
        {
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.BoomerangPanel);
            _isLost = true;
        }

        private void OnClicking(Vector3 empty)
        {
            Time.timeScale = _data.ClickingTimeScale;
        }

        private void OnInputReleased()
        {
            Time.timeScale = _data.NormalTimeScale;
        }

        private void OnRestartLevel()
        {
            Time.timeScale = _data.NormalTimeScale;
            _isLost = false;
        }
    }
}