using System;
using System.Collections.Generic;
using Commands;
using Controllers;
using Data.UnityObject;
using Data.ValueObject;
using Enums;
using Signals;
using UnityEngine;

using DG.Tweening;

namespace Managers
{
    public class TimeManager : MonoBehaviour
    {
        #region Self Variables

        #region Private Variables
        private TimeData _data;
        private bool _isLost = false;
        private bool _isClicking = false;
        private Tween _hitStopTween;
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
            MissileSignals.Instance.onMissileDestroyed += OnTriggerHitStop;

            InputSignals.Instance.onClicking += OnClicking;
            InputSignals.Instance.onInputReleased += OnInputReleased;
        }

        #endregion

        private void OnTriggerHitStop()
        {
            if (_isLost)
            {
                return;
            }

            Time.timeScale = _data.HitStopTimeScale;
            _hitStopTween?.Kill();
            _hitStopTween = DOVirtual.DelayedCall(_data.HitStopDuration, RestoreTimeScale, true);
        }

        private void RestoreTimeScale()
        {
            if (_isLost)
            {
                return;
            }

            Time.timeScale = _isClicking ? _data.ClickingTimeScale : _data.NormalTimeScale;
        }

        private void OnPlay()
        {
            _isLost = false;
            _isClicking = false;
            _hitStopTween?.Kill();
            Time.timeScale = _data.NormalTimeScale;
        }

        private void OnLevelFailed()
        {
            _hitStopTween?.Kill();
            _isLost = true;
        }

        private void OnClicking(Vector3 empty)
        {
            _isClicking = true;
            if (_hitStopTween == null || !_hitStopTween.IsActive() || _hitStopTween.IsComplete())
            {
                Time.timeScale = _data.ClickingTimeScale;
            }
        }

        private void OnInputReleased()
        {
            _isClicking = false;
            if (_hitStopTween == null || !_hitStopTween.IsActive() || _hitStopTween.IsComplete())
            {
                Time.timeScale = _data.NormalTimeScale;
            }
        }

        private void OnRestartLevel()
        {
            _hitStopTween?.Kill();
            _isLost = false;
            _isClicking = false;
            Time.timeScale = _data.NormalTimeScale;
        }
    }
}