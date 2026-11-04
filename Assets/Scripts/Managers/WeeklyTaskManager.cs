using System;
using System.Collections.Generic;
using Commands;
using Data.UnityObject;
using Data.ValueObject;
using Enums;
using Signals;
using UnityEngine;

namespace Managers
{
    public class WeeklyTaskManager : MonoBehaviour
    {
        #region Self Variables

        #region Public Variables
        public WeeklyTasksData ConfigData => _configData;
        #endregion

        #region Serialized Variables
        [SerializeField] private CD_WeeklyTasks cdWeeklyTasks;
        [SerializeField] private CD_Boomerangs cdBoomerangs;
        [SerializeField] private CD_MissileCreator cdMissileCreator;
        [SerializeField] private CD_Comments cdComments;
        #endregion

        #region Private Variables
        private WeeklyTasksData _configData;
        private WeeklyTaskStateData _state;
        private WeeklyTaskSaveCommand _saveCommand;
        private GenerateWeeklyTasksCommand _generateCommand;

        private string _installId;
        private int _equippedBoomerangId = 0;
        private bool _usedEmergencyRecallThisAttempt = false;
        private bool _isDirty = false;
        #endregion

        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }

        private void Start()
        {
            SyncEquippedBoomerangFromStore();
            ValidateWeekAndLogin(DateTime.Now);
        }

        private void Init()
        {
            if (cdWeeklyTasks == null)
            {
                cdWeeklyTasks = Resources.Load<CD_WeeklyTasks>("Data/CD_WeeklyTasks");
            }
            _configData = cdWeeklyTasks != null && cdWeeklyTasks.Data != null
                ? cdWeeklyTasks.Data
                : new WeeklyTasksData();

            if (cdBoomerangs == null)
            {
                cdBoomerangs = Resources.Load<CD_Boomerangs>("Data/CD_Boomerangs");
            }

            if (cdMissileCreator == null)
            {
                cdMissileCreator = Resources.Load<CD_MissileCreator>("Data/CD_MissileCreator");
            }

            if (cdComments == null)
            {
                cdComments = Resources.Load<CD_Comments>("Data/CD_Comments");
            }

            _saveCommand = new WeeklyTaskSaveCommand();
            _generateCommand = new GenerateWeeklyTasksCommand();

            _installId = _saveCommand.GetOrCreateInstallId();
            _state = _saveCommand.LoadState();
        }

        #region Event Subscription
        private void SubscribeEvents()
        {
            CoreGameSignals.Instance.onPlay += OnPlay;
            CoreGameSignals.Instance.onLevelSuccessful += OnLevelSuccessful;
            CoreGameSignals.Instance.onLevelFailed += OnLevelFailed;

            BoomerangSignals.Instance.onSelectBoomerang += OnSelectBoomerang;
            BoomerangSignals.Instance.onEmergencyRecall += OnEmergencyRecall;
            BoomerangSignals.Instance.onCombo += OnCombo;
            BoomerangSignals.Instance.onRewardBuffTriggered += OnRewardBuffTriggered;

            MissileSignals.Instance.onMissileDestroyed += OnMissileDestroyed;
            MissileSignals.Instance.onMissileKilled += OnMissileKilled;
            MissileSignals.Instance.onMissileArmorHit += OnMissileArmorHit;

            if (FortuneWheelSignals.Instance != null)
            {
                FortuneWheelSignals.Instance.onWheelSpinCompleted += OnWheelSpinCompleted;
            }

            if (WeeklyTaskSignals.Instance != null)
            {
                WeeklyTaskSignals.Instance.onClaimTask += OnClaimTask;
                WeeklyTaskSignals.Instance.onClaimGrandPrize += OnClaimGrandPrize;
                WeeklyTaskSignals.Instance.onGetTasks += OnGetTasks;
                WeeklyTaskSignals.Instance.onGetTaskDescription += OnGetTaskDescription;
                WeeklyTaskSignals.Instance.onHasClaimable += OnHasClaimable;
                WeeklyTaskSignals.Instance.onIsGrandPrizeClaimable += OnIsGrandPrizeClaimable;
                WeeklyTaskSignals.Instance.onIsGrandPrizeClaimed += OnIsGrandPrizeClaimed;
                WeeklyTaskSignals.Instance.onGetGrandPrizeReward += OnGetGrandPrizeReward;
                WeeklyTaskSignals.Instance.onGetRemainingSeconds += OnGetRemainingSeconds;
            }
        }

        private void UnsubscribeEvents()
        {
            if (CoreGameSignals.Instance != null)
            {
                CoreGameSignals.Instance.onPlay -= OnPlay;
                CoreGameSignals.Instance.onLevelSuccessful -= OnLevelSuccessful;
                CoreGameSignals.Instance.onLevelFailed -= OnLevelFailed;
            }

            if (BoomerangSignals.Instance != null)
            {
                BoomerangSignals.Instance.onSelectBoomerang -= OnSelectBoomerang;
                BoomerangSignals.Instance.onEmergencyRecall -= OnEmergencyRecall;
                BoomerangSignals.Instance.onCombo -= OnCombo;
                BoomerangSignals.Instance.onRewardBuffTriggered -= OnRewardBuffTriggered;
            }

            if (MissileSignals.Instance != null)
            {
                MissileSignals.Instance.onMissileDestroyed -= OnMissileDestroyed;
                MissileSignals.Instance.onMissileKilled -= OnMissileKilled;
                MissileSignals.Instance.onMissileArmorHit -= OnMissileArmorHit;
            }

            if (FortuneWheelSignals.Instance != null)
            {
                FortuneWheelSignals.Instance.onWheelSpinCompleted -= OnWheelSpinCompleted;
            }

            if (WeeklyTaskSignals.Instance != null)
            {
                WeeklyTaskSignals.Instance.onClaimTask -= OnClaimTask;
                WeeklyTaskSignals.Instance.onClaimGrandPrize -= OnClaimGrandPrize;
                WeeklyTaskSignals.Instance.onGetTasks -= OnGetTasks;
                WeeklyTaskSignals.Instance.onGetTaskDescription -= OnGetTaskDescription;
                WeeklyTaskSignals.Instance.onHasClaimable -= OnHasClaimable;
                WeeklyTaskSignals.Instance.onIsGrandPrizeClaimable -= OnIsGrandPrizeClaimable;
                WeeklyTaskSignals.Instance.onIsGrandPrizeClaimed -= OnIsGrandPrizeClaimed;
                WeeklyTaskSignals.Instance.onGetGrandPrizeReward -= OnGetGrandPrizeReward;
                WeeklyTaskSignals.Instance.onGetRemainingSeconds -= OnGetRemainingSeconds;
            }
        }

        private void OnDestroy()
        {
            FlushSaveIfDirty();
            UnsubscribeEvents();
        }
        #endregion

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                FlushSaveIfDirty();
            }
            else
            {
                ValidateWeekAndLogin(DateTime.Now);
            }
        }

        private void OnApplicationQuit()
        {
            FlushSaveIfDirty();
        }

        private void SyncEquippedBoomerangFromStore()
        {
            if (BoomerangStoreManager.Instance != null)
            {
                _equippedBoomerangId = BoomerangStoreManager.Instance.SelectedItemId;
            }
        }

        #region Weekly Cycle & Login Validation
        public static DateTime GetLocalMondayStart(DateTime localNow)
        {
            int diff = (7 + ((int)localNow.DayOfWeek - (int)DayOfWeek.Monday)) % 7;
            return localNow.Date.AddDays(-diff);
        }

        private void ValidateWeekAndLogin(DateTime localNow)
        {
            DateTime mondayStart = GetLocalMondayStart(localNow);
            long currentWeekStartTicks = mondayStart.Ticks;
            bool weekGenerated = false;

            if (_state.Tasks == null || _state.Tasks.Count == 0 || _state.WeekStartTicks == 0L)
            {
                GenerateNewWeek(mondayStart, localNow);
                weekGenerated = true;
            }
            else if (currentWeekStartTicks > _state.WeekStartTicks)
            {
                GrantUnclaimedRewardsSilently();
                GenerateNewWeek(mondayStart, localNow);
                weekGenerated = true;
            }
            // Note: If currentWeekStartTicks < _state.WeekStartTicks (clock moved backwards), keep current tasks untouched.

            long todayTicks = localNow.Date.Ticks;
            if (todayTicks > _state.LastLoginDayTicks)
            {
                _state.LastLoginDayTicks = todayTicks;
                IncrementTasksOfType(WeeklyTaskType.LoginDays, 1);
                _isDirty = true;
            }

            if (weekGenerated || _isDirty)
            {
                SaveNow();
            }

            WeeklyTaskSignals.Instance.onWeeklyTasksRefreshed?.Invoke();
        }

        private void GrantUnclaimedRewardsSilently()
        {
            if (_state == null || _state.Tasks == null)
            {
                return;
            }

            int totalUnclaimedCoins = 0;
            bool allCompleted = _state.Tasks.Count > 0;

            for (int i = 0; i < _state.Tasks.Count; i++)
            {
                var task = _state.Tasks[i];
                if (task == null)
                {
                    continue;
                }

                if (!task.IsCompleted)
                {
                    allCompleted = false;
                }
                else if (!task.IsClaimed)
                {
                    totalUnclaimedCoins += Mathf.Max(0, task.Reward);
                    task.IsClaimed = true;
                }
            }

            if (allCompleted && !_state.IsGrandPrizeClaimed)
            {
                totalUnclaimedCoins += Mathf.Max(0, OnGetGrandPrizeReward());
                _state.IsGrandPrizeClaimed = true;
            }

            if (totalUnclaimedCoins > 0 && ScoreSignals.Instance != null)
            {
                ScoreSignals.Instance.onScoreIncrease?.Invoke(ScoreTypeEnums.Money, totalUnclaimedCoins);
            }
        }

        private void GenerateNewWeek(DateTime mondayStart, DateTime localNow)
        {
            DateTime nextMonday = mondayStart.AddDays(7);
            int daysRemaining = Mathf.Clamp(Mathf.CeilToInt((float)(nextMonday - localNow.Date).TotalDays), 1, 7);
            int playerLevel = GetCurrentPlayerLevel();
            int seed = GenerateWeeklyTasksCommand.ComputeDeterministicSeed(_installId, mondayStart.Ticks);

            MissileLevelData missileData = cdMissileCreator != null ? cdMissileCreator.Data : null;
            ComboCommentsData commentsData = cdComments != null ? cdComments.Data : null;

            _state.WeekStartTicks = mondayStart.Ticks;
            _state.LastLoginDayTicks = 0L;
            _state.IsGrandPrizeClaimed = false;
            _state.CurrentWinStreak = 0;
            _state.Tasks = _generateCommand.Generate(
                seed,
                playerLevel,
                daysRemaining,
                _configData,
                cdBoomerangs,
                missileData,
                commentsData);
            _isDirty = true;
        }
        #endregion

        #region Gameplay Event Handlers
        private bool IsTutorialLevel()
        {
            return LevelSignals.Instance != null && LevelSignals.Instance.onGetLevelId() == 0;
        }

        private int GetCurrentPlayerLevel()
        {
            if (LevelSignals.Instance != null)
            {
                return Mathf.Max(0, LevelSignals.Instance.onGetLevelId());
            }
            return 0;
        }

        private void OnPlay()
        {
            _usedEmergencyRecallThisAttempt = false;
            SyncEquippedBoomerangFromStore();
        }

        private void OnSelectBoomerang(int boomerangId)
        {
            _equippedBoomerangId = boomerangId;
        }

        private void OnEmergencyRecall()
        {
            _usedEmergencyRecallThisAttempt = true;
        }

        private void OnLevelSuccessful()
        {
            if (IsTutorialLevel())
            {
                return;
            }

            IncrementTasksOfType(WeeklyTaskType.CompleteLevels, 1);

            _state.CurrentWinStreak++;
            UpdateWinStreakTasks(_state.CurrentWinStreak);

            if (!_usedEmergencyRecallThisAttempt)
            {
                IncrementTasksOfType(WeeklyTaskType.NoRecallLevels, 1);
            }

            IncrementTasksOfTypeWithParam(WeeklyTaskType.CompleteLevelsWithBoomerang, _equippedBoomerangId, 1);
            UpdateDifferentBoomerangsTask(_equippedBoomerangId);

            SaveNow();
        }

        private void OnLevelFailed()
        {
            if (IsTutorialLevel())
            {
                return;
            }

            if (_state.CurrentWinStreak != 0)
            {
                _state.CurrentWinStreak = 0;
                _isDirty = true;
            }

            FlushSaveIfDirty();
        }

        private void OnMissileDestroyed(float shakeStrength)
        {
            if (IsTutorialLevel())
            {
                return;
            }

            IncrementTasksOfType(WeeklyTaskType.DestroyMissiles, 1);
        }

        private void OnMissileKilled(MissileEnums missileType)
        {
            if (IsTutorialLevel())
            {
                return;
            }

            IncrementTasksOfTypeWithParam(WeeklyTaskType.DestroyMissileType, (int)missileType, 1);
        }

        private void OnMissileArmorHit()
        {
            if (IsTutorialLevel())
            {
                return;
            }

            IncrementTasksOfType(WeeklyTaskType.BreakArmor, 1);
        }

        private void OnRewardBuffTriggered(BoomerangStatEnums statType, RewardMissileData rewardData, Vector3 position)
        {
            if (IsTutorialLevel())
            {
                return;
            }

            IncrementTasksOfType(WeeklyTaskType.CollectRewardBuff, 1);
        }

        private void OnCombo(int comboValue)
        {
            if (IsTutorialLevel())
            {
                return;
            }

            // Count once per combo chain when the exact required combo index is reached
            IncrementTasksOfTypeWithParam(WeeklyTaskType.ReachCombo, comboValue, 1);
        }

        private void OnWheelSpinCompleted(int wonSectorIndex)
        {
            IncrementTasksOfType(WeeklyTaskType.SpinFortuneWheel, 1);
            SaveNow();
        }
        #endregion

        #region Task Progress Updates
        private void IncrementTasksOfType(WeeklyTaskType type, int amount)
        {
            if (_state == null || _state.Tasks == null || amount <= 0)
            {
                return;
            }

            for (int i = 0; i < _state.Tasks.Count; i++)
            {
                var task = _state.Tasks[i];
                if (task == null || task.Type != type || task.IsCompleted)
                {
                    continue;
                }

                SetTaskProgress(i, task, task.Progress + amount);
            }
        }

        private void IncrementTasksOfTypeWithParam(WeeklyTaskType type, int paramId, int amount)
        {
            if (_state == null || _state.Tasks == null || amount <= 0)
            {
                return;
            }

            for (int i = 0; i < _state.Tasks.Count; i++)
            {
                var task = _state.Tasks[i];
                if (task == null || task.Type != type || task.ParamId != paramId || task.IsCompleted)
                {
                    continue;
                }

                SetTaskProgress(i, task, task.Progress + amount);
            }
        }

        private void UpdateWinStreakTasks(int currentStreak)
        {
            if (_state == null || _state.Tasks == null)
            {
                return;
            }

            for (int i = 0; i < _state.Tasks.Count; i++)
            {
                var task = _state.Tasks[i];
                if (task == null || task.Type != WeeklyTaskType.WinStreak || task.IsCompleted)
                {
                    continue;
                }

                if (currentStreak > task.Progress)
                {
                    SetTaskProgress(i, task, currentStreak);
                }
            }
        }

        private void UpdateDifferentBoomerangsTask(int boomerangId)
        {
            if (_state == null || _state.Tasks == null)
            {
                return;
            }

            for (int i = 0; i < _state.Tasks.Count; i++)
            {
                var task = _state.Tasks[i];
                if (task == null || task.Type != WeeklyTaskType.CompleteLevelsWithDifferentBoomerangs || task.IsCompleted)
                {
                    continue;
                }

                if (task.UniqueIds == null)
                {
                    task.UniqueIds = new List<int>();
                }

                if (!task.UniqueIds.Contains(boomerangId))
                {
                    task.UniqueIds.Add(boomerangId);
                    SetTaskProgress(i, task, task.UniqueIds.Count);
                }
            }
        }

        private void SetTaskProgress(int index, WeeklyTaskProgressData task, int newProgress)
        {
            bool wasCompleted = task.IsCompleted;
            task.Progress = Mathf.Clamp(newProgress, 0, task.Target);
            _isDirty = true;

            WeeklyTaskSignals.Instance.onTaskProgressChanged?.Invoke(index);
            if (!wasCompleted && task.IsCompleted)
            {
                WeeklyTaskSignals.Instance.onTaskCompleted?.Invoke(index);
                SaveNow();
            }
        }
        #endregion

        #region Claiming & Queries
        private void OnClaimTask(int index)
        {
            if (_state == null || _state.Tasks == null || index < 0 || index >= _state.Tasks.Count)
            {
                return;
            }

            var task = _state.Tasks[index];
            if (task == null || !task.IsClaimable)
            {
                return;
            }

            task.IsClaimed = true;
            SaveNow();

            ScoreSignals.Instance.onScoreIncrease?.Invoke(ScoreTypeEnums.Money, task.Reward);
            AudioSignals.Instance.onPlaySound?.Invoke(AudioSoundEnums.Click);
            PitchSignals.Instance.onPlayPitch?.Invoke(PitchEnums.Win);

            WeeklyTaskSignals.Instance.onTaskProgressChanged?.Invoke(index);
            WeeklyTaskSignals.Instance.onWeeklyTasksRefreshed?.Invoke();
        }

        private void OnClaimGrandPrize()
        {
            if (!OnIsGrandPrizeClaimable())
            {
                return;
            }

            _state.IsGrandPrizeClaimed = true;
            SaveNow();

            int reward = OnGetGrandPrizeReward();
            ScoreSignals.Instance.onScoreIncrease?.Invoke(ScoreTypeEnums.Money, reward);
            AudioSignals.Instance.onPlaySound?.Invoke(AudioSoundEnums.Win);
            PitchSignals.Instance.onPlayPitch?.Invoke(PitchEnums.Win);

            WeeklyTaskSignals.Instance.onWeeklyTasksRefreshed?.Invoke();
        }

        private List<WeeklyTaskProgressData> OnGetTasks()
        {
            return _state != null && _state.Tasks != null ? _state.Tasks : new List<WeeklyTaskProgressData>();
        }

        private string OnGetTaskDescription(int index)
        {
            if (_state == null || _state.Tasks == null || index < 0 || index >= _state.Tasks.Count)
            {
                return string.Empty;
            }

            var task = _state.Tasks[index];
            if (task == null)
            {
                return string.Empty;
            }

            var def = _configData != null ? _configData.GetDefinition(task.Type) : null;
            string format = def != null && !string.IsNullOrEmpty(def.DescriptionFormat)
                ? def.DescriptionFormat
                : "Complete {0} objectives";

            string dynamicParam = GetDynamicParameterName(task);
            try
            {
                return string.Format(format, task.Target, dynamicParam);
            }
            catch
            {
                return $"{format} ({task.Target})";
            }
        }

        private string GetDynamicParameterName(WeeklyTaskProgressData task)
        {
            switch (task.Type)
            {
                case WeeklyTaskType.DestroyMissileType:
                    MissileEnums mType = (MissileEnums)task.ParamId;
                    return _configData != null
                        ? _configData.GetMissileDisplayName(mType)
                        : WeeklyTasksData.GetFallbackMissileDisplayName(mType);

                case WeeklyTaskType.CompleteLevelsWithBoomerang:
                    if (cdBoomerangs != null)
                    {
                        var item = cdBoomerangs.GetBoomerang(task.ParamId);
                        if (item != null && !string.IsNullOrEmpty(item.Title))
                        {
                            return item.Title.Trim();
                        }
                    }
                    return "Classic V-Curve";

                case WeeklyTaskType.ReachCombo:
                    if (cdComments != null && cdComments.Data != null && cdComments.Data.CommentsList != null && cdComments.Data.CommentsList.Count > 0)
                    {
                        int idx = Mathf.Clamp(task.ParamId, 0, cdComments.Data.CommentsList.Count - 1);
                        return cdComments.Data.CommentsList[idx];
                    }
                    return "Amazing!";

                default:
                    return string.Empty;
            }
        }

        private bool OnHasClaimable()
        {
            if (_state == null || _state.Tasks == null)
            {
                return false;
            }

            for (int i = 0; i < _state.Tasks.Count; i++)
            {
                if (_state.Tasks[i] != null && _state.Tasks[i].IsClaimable)
                {
                    return true;
                }
            }

            return OnIsGrandPrizeClaimable();
        }

        private bool OnIsGrandPrizeClaimable()
        {
            if (_state == null || _state.Tasks == null || _state.Tasks.Count == 0 || _state.IsGrandPrizeClaimed)
            {
                return false;
            }

            for (int i = 0; i < _state.Tasks.Count; i++)
            {
                if (_state.Tasks[i] == null || !_state.Tasks[i].IsCompleted)
                {
                    return false;
                }
            }

            return true;
        }

        private bool OnIsGrandPrizeClaimed()
        {
            return _state != null && _state.IsGrandPrizeClaimed;
        }

        private int OnGetGrandPrizeReward()
        {
            return _configData != null ? _configData.GrandPrizeReward : 1500;
        }

        private long OnGetRemainingSeconds()
        {
            DateTime now = DateTime.Now;
            DateTime mondayStart = _state != null && _state.WeekStartTicks > 0L
                ? new DateTime(_state.WeekStartTicks)
                : GetLocalMondayStart(now);
            DateTime nextMonday = mondayStart.AddDays(7);
            double remaining = (nextMonday - now).TotalSeconds;
            return remaining > 0 ? (long)remaining : 0L;
        }

        private void SaveNow()
        {
            _isDirty = false;
            _saveCommand?.SaveState(_state);
        }

        private void FlushSaveIfDirty()
        {
            if (_isDirty)
            {
                SaveNow();
            }
        }
        #endregion

#if UNITY_EDITOR
        #region Debug Context Menus
        [ContextMenu("Debug: Force New Week")]
        private void DebugForceNewWeek()
        {
            GrantUnclaimedRewardsSilently();
            DateTime nextWeekMonday = _state != null && _state.WeekStartTicks > 0L
                ? new DateTime(_state.WeekStartTicks).AddDays(7)
                : GetLocalMondayStart(DateTime.Now).AddDays(7);
            GenerateNewWeek(nextWeekMonday, nextWeekMonday);
            _state.LastLoginDayTicks = nextWeekMonday.Date.Ticks;
            IncrementTasksOfType(WeeklyTaskType.LoginDays, 1);
            SaveNow();
            WeeklyTaskSignals.Instance.onWeeklyTasksRefreshed?.Invoke();
            DebugLogTasks();
        }

        [ContextMenu("Debug: Simulate Next Day Login")]
        private void DebugSimulateNextDayLogin()
        {
            if (_state == null)
            {
                return;
            }

            DateTime nextDay = _state.LastLoginDayTicks > 0L
                ? new DateTime(_state.LastLoginDayTicks).AddDays(1)
                : DateTime.Now.Date.AddDays(1);
            _state.LastLoginDayTicks = nextDay.Ticks;
            IncrementTasksOfType(WeeklyTaskType.LoginDays, 1);
            SaveNow();
            WeeklyTaskSignals.Instance.onWeeklyTasksRefreshed?.Invoke();
        }

        [ContextMenu("Debug: Complete All Tasks")]
        private void DebugCompleteAllTasks()
        {
            if (_state == null || _state.Tasks == null)
            {
                return;
            }

            for (int i = 0; i < _state.Tasks.Count; i++)
            {
                var task = _state.Tasks[i];
                if (task != null)
                {
                    SetTaskProgress(i, task, task.Target);
                }
            }

            SaveNow();
            WeeklyTaskSignals.Instance.onWeeklyTasksRefreshed?.Invoke();
        }

        [ContextMenu("Debug: Log Tasks")]
        private void DebugLogTasks()
        {
            if (_state == null || _state.Tasks == null)
            {
                Debug.Log("[WeeklyTaskManager] No tasks loaded.");
                return;
            }

            for (int i = 0; i < _state.Tasks.Count; i++)
            {
                var t = _state.Tasks[i];
                Debug.Log($"[WeeklyTaskManager] Slot {i + 1} [{t.Difficulty}] {OnGetTaskDescription(i)} | {t.Progress}/{t.Target} | Reward: {t.Reward} | Claimed: {t.IsClaimed}");
            }
        }
        #endregion
#endif
    }
}
