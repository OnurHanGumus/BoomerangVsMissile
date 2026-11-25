using System;
using Controllers;
using Data.UnityObject;
using Data.ValueObject;
using Enums;
using Extensions;
using Keys;
using Signals;
using UnityEngine;

namespace Managers
{
    public class LevelManager : MonoBehaviour
    {
        #region Self Variables

        #region Public Variables


        #endregion

        #region Serialized Variables

        [Space] [SerializeField] private GameObject levelHolder;
        [SerializeField] private LevelLoaderCommand levelLoader;
        [SerializeField] private ClearActiveLevelCommand levelClearer;

        #endregion

        #region Private Variables

        private int _maxReachedLevelId;
        private int _currentLevelId;
        private LevelData _data;
        private int _currentModdedLevel = 0;
        private bool _isLevelSuccessful = false;
        #endregion

        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }

        private void Init()
        {
            _maxReachedLevelId = GetActiveLevel();
            _currentLevelId = _maxReachedLevelId;
            _data = GetData();
        }

        private LevelData GetData() => Resources.Load<CD_Level>("Data/CD_Level").Data;
        private int GetActiveLevel()
        {
            if (!ES3.FileExists()) return 0;
            return ES3.KeyExists("Level") ? ES3.Load<int>("Level") : 0;
        }

        #region Event Subscription

        private void SubscribeEvents()
        {
            CoreGameSignals.Instance.onLevelInitialize += OnInitializeLevel;
            CoreGameSignals.Instance.onClearActiveLevel += OnClearActiveLevel;
            CoreGameSignals.Instance.onNextLevel += OnNextLevel;
            CoreGameSignals.Instance.onLevelSuccessful += OnLevelSuccessful;
            CoreGameSignals.Instance.onRestartLevel += OnRestartLevel;
            LevelSignals.Instance.onGetLevelId += OnGetLevelId;
            LevelSignals.Instance.onGetMaxReachedLevelId += OnGetMaxReachedLevelId;
            LevelSignals.Instance.onGetCurrentModdedLevel += OnGetModdedLevel;
            LevelSignals.Instance.onPreviousLevelOpened += OnPreviousLevelOpened;
        }

        private void UnsubscribeEvents()
        {
            CoreGameSignals.Instance.onLevelInitialize -= OnInitializeLevel;
            CoreGameSignals.Instance.onClearActiveLevel -= OnClearActiveLevel;
            CoreGameSignals.Instance.onNextLevel -= OnNextLevel;
            CoreGameSignals.Instance.onLevelSuccessful -= OnLevelSuccessful;
            CoreGameSignals.Instance.onRestartLevel -= OnRestartLevel;
            LevelSignals.Instance.onGetLevelId -= OnGetLevelId;
            LevelSignals.Instance.onGetMaxReachedLevelId -= OnGetMaxReachedLevelId;
            LevelSignals.Instance.onGetCurrentModdedLevel -= OnGetModdedLevel;
            LevelSignals.Instance.onPreviousLevelOpened -= OnPreviousLevelOpened;
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        private void OnLevelSuccessful()
        {
            _isLevelSuccessful = true;
        }

        #endregion

        private void Start()
        {
            OnInitializeLevel();
            TutorialSignals.Instance.onTutorialActive(_maxReachedLevelId == 0);
        }

        private void OnEnable()
        {
            Debug.Log(_currentLevelId);
        }

        private void OnNextLevel()
        {
            if (_currentLevelId != _maxReachedLevelId)
            {
                _currentLevelId = _maxReachedLevelId;
            }
            else
            {
                _maxReachedLevelId++;
                _currentLevelId = _maxReachedLevelId;
                SaveSignals.Instance.onSave(_maxReachedLevelId, SaveLoadStates.Level, SaveFiles.SaveFile);
            }

            CoreGameSignals.Instance.onRestartLevel();
        }

        private void OnPreviousLevelOpened(int levelId)
        {
            _currentLevelId = levelId;
            CoreGameSignals.Instance.onRestartLevel();
        }

        private void OnRestartLevel()
        {
            CoreGameSignals.Instance.onClearActiveLevel();
            CoreGameSignals.Instance.onReset();
            CoreGameSignals.Instance.onLevelInitialize();
            _isLevelSuccessful = false;
        }

        private int OnGetLevelId()
        {
            return _currentLevelId;
        }

        private int OnGetMaxReachedLevelId()
        {
            return _maxReachedLevelId;
        }

        private void OnInitializeLevel()
        {
            UnityEngine.Object[] Levels = Resources.LoadAll("Levels");
            int newLevelId = _currentLevelId % Levels.Length;
            _currentModdedLevel = newLevelId;
            levelLoader.InitializeLevel((GameObject)Levels[newLevelId], levelHolder.transform);
            LevelSignals.Instance.onLevelLoaded(newLevelId);
        }

        private int OnGetModdedLevel()
        {
            return _currentModdedLevel;
        }

        private void OnClearActiveLevel()
        {
            levelClearer.ClearActiveLevel(levelHolder.transform);
        }
    }
}