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
    public class PlayerManager : MonoBehaviour
    {
        #region Self Variables

        #region Public Variables

        #endregion

        #region Serialized Variables
        [SerializeField] private PlayerAnimationController animationController;
        [SerializeField] private GameObject catchObject, boomerangHand;
        #endregion

        #region Private Variables
        private PlayerData _data;
        private PlayerMovementController _movementController;
        private List<int> _playerUpgradeList;
        #endregion

        #endregion

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _data = GetData();
            _movementController = GetComponent<PlayerMovementController>();
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
            CoreGameSignals.Instance.onPlay += _movementController.OnPlay;
            CoreGameSignals.Instance.onLevelSuccessful += _movementController.OnLevelSuccess;
            CoreGameSignals.Instance.onLevelFailed += _movementController.OnLevelFailed;
            CoreGameSignals.Instance.onRestartLevel += _movementController.OnRestartLevel;
            CoreGameSignals.Instance.onRestartLevel += OnResetLevel;
            BoomerangSignals.Instance.onBoomerangDisapeared += OnBoomerangBecomeInvisible;
            BoomerangSignals.Instance.onBoomerangThrowed += OnBoomerangThrowed;
            BoomerangSignals.Instance.onBoomerangHasReturned += OnBoomerangHasReturned;
            BoomerangSignals.Instance.onBoomerangRebuilded += OnBoomerangRebuilded;
            BoomerangSignals.Instance.onBoomerangReturning += OnBoomerangReturning;
            PlayerSignals.Instance.onChangePlayerAnimation += animationController.OnChangeAnimation;
            PlayerSignals.Instance.onAnimationSpeedIncreased += IncreaseAnimationSpeed;

        }

        private void UnsubscribeEvents()
        {
            CoreGameSignals.Instance.onPlay -= OnPlay;
            CoreGameSignals.Instance.onPlay -= _movementController.OnPlay;
            CoreGameSignals.Instance.onLevelSuccessful -= _movementController.OnLevelSuccess;
            CoreGameSignals.Instance.onLevelFailed -= _movementController.OnLevelFailed;
            CoreGameSignals.Instance.onRestartLevel -= _movementController.OnRestartLevel;
            CoreGameSignals.Instance.onRestartLevel -= OnResetLevel;
            BoomerangSignals.Instance.onBoomerangDisapeared -= OnBoomerangBecomeInvisible;
            BoomerangSignals.Instance.onBoomerangThrowed -= OnBoomerangThrowed;
            BoomerangSignals.Instance.onBoomerangHasReturned -= OnBoomerangHasReturned;
            BoomerangSignals.Instance.onBoomerangRebuilded -= OnBoomerangRebuilded;
            BoomerangSignals.Instance.onBoomerangReturning -= OnBoomerangReturning;
            PlayerSignals.Instance.onChangePlayerAnimation -= animationController.OnChangeAnimation;
            PlayerSignals.Instance.onAnimationSpeedIncreased -= IncreaseAnimationSpeed;
        }


        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        #endregion


        private void OnPlay()
        {
        }

        private void OnInitializePlayerUpgrades(List<int> upgradeList)
        {
            _playerUpgradeList = upgradeList;
        }

        private void OnBoomerangThrowed()
        {
            PlayerSignals.Instance.onChangePlayerAnimation?.Invoke(PlayerAnimationStates.Throw);
            catchObject.SetActive(false);
            boomerangHand.SetActive(false);
            
        }
        private void OnBoomerangHasReturned()
        {
            PlayerSignals.Instance.onChangePlayerAnimation?.Invoke(PlayerAnimationStates.Catch);
            Debug.Log("tetiklendi");
        }
        private void OnBoomerangRebuilded()
        {
            PlayerSignals.Instance.onChangePlayerAnimation?.Invoke(PlayerAnimationStates.Idle);
        }

        private void OnBoomerangBecomeInvisible()
        {
            PlayerSignals.Instance.onChangePlayerAnimation?.Invoke(PlayerAnimationStates.BuildBoomerang);
        }

        private void IncreaseAnimationSpeed()
        {
            animationController.OnChangeAnimationSpeed();
        }
        private void OnBoomerangReturning()
        {
            catchObject.SetActive(true);
            boomerangHand.SetActive(true);


        }
        private void OnResetLevel()
        {
            PlayerSignals.Instance.onChangePlayerAnimation?.Invoke(PlayerAnimationStates.Idle);
        }
    }
}