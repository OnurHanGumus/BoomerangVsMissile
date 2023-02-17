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
        private PlayerData _data;
        private bool _isLoosed = false;
        #endregion

        #endregion

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _data = GetData();
        }
        public PlayerData GetData() => Resources.Load<CD_Player>("Data/CD_Player").Data;

        #region Event Subscription

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            CoreGameSignals.Instance.onPlay += OnPlay;
            CoreGameSignals.Instance.onLevelFailed += OnLevelFailed;
            CoreGameSignals.Instance.onRestartLevel += OnRestartLevel;
            BoomerangSignals.Instance.onBoomerangDisapeared += OnBoomerangDisapeared;
            BoomerangSignals.Instance.onBoomerangRebuilded += OnBoomerangRebuilded;
        }

        private void UnsubscribeEvents()
        {
            CoreGameSignals.Instance.onPlay -= OnPlay;
            CoreGameSignals.Instance.onLevelFailed -= OnLevelFailed;
            CoreGameSignals.Instance.onRestartLevel -= OnRestartLevel;
            BoomerangSignals.Instance.onBoomerangDisapeared -= OnBoomerangDisapeared;
            BoomerangSignals.Instance.onBoomerangRebuilded -= OnBoomerangRebuilded;
        }


        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        #endregion
        private void OnPlay()
        {
            Time.timeScale = 1f;
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.BoomerangPanel);

        }

        private void OnBoomerangDisapeared()
        {
            if (_isLoosed == true)
            {
                return;
            }
            UISignals.Instance.onOpenPanel?.Invoke(UIPanels.BoomerangPanel);
            Time.timeScale = 0.05f;
        }
        private void OnBoomerangRebuilded()
        {
            Time.timeScale = 1f;
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.BoomerangPanel);

        }
        private void OnLevelFailed()
        {
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.BoomerangPanel);
            _isLoosed = true;
        }
        private void OnRestartLevel()
        {
            Time.timeScale = 1f;
            _isLoosed = false;

        }
    }
}