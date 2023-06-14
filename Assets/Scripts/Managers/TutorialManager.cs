using System;
using System.Collections.Generic;
using Commands;
using Controllers;
using Data.UnityObject;
using Data.ValueObject;
using Enums;
using Signals;
using UnityEngine;
using TMPro;
using DG.Tweening;

namespace Managers
{
    public class TutorialManager : MonoBehaviour
    {
        #region Self Variables

        #region Public Variables

        #endregion

        #region Serialized Variables
        [SerializeField] private TextMeshProUGUI tutorialText;
        #endregion

        #region Private Variables
        private TutorialData _data;
        private int _textIndeks = 0;
        private bool _isFirstTime = true;
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

        public TutorialData GetData() => Resources.Load<CD_Tutorial>("Data/CD_Tutorial").Data;

        #region Event Subscription

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            CoreGameSignals.Instance.onPlay += OnPlay;
            CoreGameSignals.Instance.onRestartLevel += OnRestartLevel;
            BoomerangSignals.Instance.onBoomerangNextTarget += OnHitMissile;
            TutorialSignals.Instance.onTutorialSatisfied += OnTutorialSatisfied;
            TutorialSignals.Instance.onTutorialActive += OnTutorialActive;
        }

        private void UnsubscribeEvents()
        {
            CoreGameSignals.Instance.onPlay -= OnPlay;
            CoreGameSignals.Instance.onRestartLevel -= OnRestartLevel;
            BoomerangSignals.Instance.onBoomerangNextTarget -= OnHitMissile;
            TutorialSignals.Instance.onTutorialSatisfied -= OnTutorialSatisfied;
            TutorialSignals.Instance.onTutorialActive -= OnTutorialActive;
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        #endregion
        private void OnPlay()
        {
            tutorialText.text = _data.TextList[_textIndeks++];
            tutorialText.DOFade(1, 0.3f);
        }

        private void OnHitMissile()
        {
            tutorialText.DOFade(0, 0.3f).OnComplete(()=> 
            { 
                tutorialText.DOFade(1, 0.3f);
                tutorialText.text = _data.TextList[_textIndeks];
            });

            if (_isFirstTime)
            {
                AudioSignals.Instance.onPlaySound(AudioSoundEnums.Win);
                _isFirstTime = false;
            }
        }

        private void OnTutorialActive(bool isTrue)
        {
            if (!isTrue)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnTutorialSatisfied()
        {
            tutorialText.gameObject.SetActive(false);
        }

        private void OnRestartLevel()
        {
            _textIndeks = 0;
        }
    }
}