using System;
using Data.UnityObject;
using Data.ValueObject;
using Enums;
using Signals;
using UnityEngine;

namespace Managers
{
    public class EnergyManager : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables
        [SerializeField] private CD_Energy cdEnergy;
        #endregion

        #region Private Variables
        private const float PollIntervalSeconds = 0.2f;

        private EnergyData _data;
        private int _currentEnergy;
        private long _nextRestoreUtcTicks;
        private bool _isLoaded;
        private float _pollTimer;
        private int _lastBroadcastedSeconds = -1;
        #endregion

        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }

        private void Start()
        {
            EnsureLoaded();
            EvaluateRegeneration(true);
        }

        private void Init()
        {
            _data = GetData();
        }

        private EnergyData GetData()
        {
            if (cdEnergy == null)
            {
                cdEnergy = Resources.Load<CD_Energy>("Data/CD_Energy");
            }

            return cdEnergy != null && cdEnergy.Data != null ? cdEnergy.Data : new EnergyData();
        }

        #region Event Subscription
        private void SubscribeEvents()
        {
            EnergySignals.Instance.onTryConsumeEnergy += OnTryConsumeEnergy;
            EnergySignals.Instance.onAddEnergy += OnAddEnergy;
            EnergySignals.Instance.onGetCurrentEnergy += OnGetCurrentEnergy;
            EnergySignals.Instance.onGetMaxEnergy += OnGetMaxEnergy;
            EnergySignals.Instance.onGetRemainingSeconds += OnGetRemainingSeconds;
        }

        private void UnsubscribeEvents()
        {
            if (EnergySignals.Instance != null)
            {
                EnergySignals.Instance.onTryConsumeEnergy -= OnTryConsumeEnergy;
                EnergySignals.Instance.onAddEnergy -= OnAddEnergy;
                EnergySignals.Instance.onGetCurrentEnergy -= OnGetCurrentEnergy;
                EnergySignals.Instance.onGetMaxEnergy -= OnGetMaxEnergy;
                EnergySignals.Instance.onGetRemainingSeconds -= OnGetRemainingSeconds;
            }
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }
        #endregion

        private void Update()
        {
            if (!_isLoaded || _currentEnergy >= _data.MaxEnergy)
            {
                return;
            }

            _pollTimer += Time.unscaledDeltaTime;
            if (_pollTimer < PollIntervalSeconds)
            {
                return;
            }

            _pollTimer = 0f;
            EvaluateRegeneration(false);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus && _isLoaded)
            {
                EvaluateRegeneration(true);
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && _isLoaded)
            {
                EvaluateRegeneration(true);
            }
        }

        private void EnsureLoaded()
        {
            if (_isLoaded)
            {
                return;
            }

            int initialDefault = Mathf.Max(0, _data.InitialEnergy);
            _currentEnergy = SaveSignals.Instance != null
                ? SaveSignals.Instance.onGetIntWithDefault(SaveLoadStates.Energy, initialDefault, SaveFiles.SaveFile)
                : initialDefault;
            _currentEnergy = Mathf.Max(0, _currentEnergy);

            _nextRestoreUtcTicks = SaveSignals.Instance != null
                ? SaveSignals.Instance.onGetLong(SaveLoadStates.EnergyNextRestoreTicks, SaveFiles.SaveFile)
                : 0L;

            _isLoaded = true;
        }

        private void EvaluateRegeneration(bool forceBroadcast)
        {
            EnsureLoaded();

            int maxEnergy = Mathf.Max(1, _data.MaxEnergy);
            int cooldownSeconds = Mathf.Max(1, _data.RegenCooldownSeconds);
            long cooldownTicks = TimeSpan.FromSeconds(cooldownSeconds).Ticks;
            long nowTicks = DateTime.UtcNow.Ticks;

            bool energyChanged = false;

            if (_currentEnergy >= maxEnergy)
            {
                if (_nextRestoreUtcTicks != 0L)
                {
                    _nextRestoreUtcTicks = 0L;
                    SaveNextRestoreTicks();
                }
            }
            else
            {
                if (_nextRestoreUtcTicks <= 0L)
                {
                    _nextRestoreUtcTicks = nowTicks + cooldownTicks;
                    SaveNextRestoreTicks();
                }
                else if (_nextRestoreUtcTicks > nowTicks + cooldownTicks)
                {
                    // Guard against device clock rollback creating oversized timers
                    _nextRestoreUtcTicks = nowTicks + cooldownTicks;
                    SaveNextRestoreTicks();
                }
                else if (nowTicks >= _nextRestoreUtcTicks)
                {
                    long overdueTicks = nowTicks - _nextRestoreUtcTicks;
                    int gained = 1 + (int)(overdueTicks / cooldownTicks);
                    int previousEnergy = _currentEnergy;

                    _currentEnergy = Mathf.Min(maxEnergy, _currentEnergy + gained);
                    energyChanged = _currentEnergy != previousEnergy;

                    if (_currentEnergy >= maxEnergy)
                    {
                        _nextRestoreUtcTicks = 0L;
                    }
                    else
                    {
                        _nextRestoreUtcTicks += gained * cooldownTicks;
                    }

                    SaveCurrentEnergy();
                    SaveNextRestoreTicks();
                }
            }

            BroadcastState(forceBroadcast || energyChanged);
        }

        private bool OnTryConsumeEnergy(int amount)
        {
            EnsureLoaded();
            EvaluateRegeneration(false);

            if (_data.IsTutorialFree && LevelSignals.Instance != null && LevelSignals.Instance.onGetLevelId() == 0)
            {
                return true;
            }

            int cost = amount > 0 ? amount : _data.EnergyCostPerStart;
            if (cost <= 0)
            {
                return true;
            }

            if (_currentEnergy < cost)
            {
                EnergySignals.Instance.onInsufficientEnergy?.Invoke();
                PitchSignals.Instance.onPlayPitch?.Invoke(PitchEnums.Lose);

                if (_data.EnableCoinRefillPrompt &&
                    _data.CoinRefillCost > 0 &&
                    ScoreSignals.Instance != null &&
                    ScoreSignals.Instance.onGetMoney() >= _data.CoinRefillCost &&
                    UISignals.Instance != null)
                {
                    UISignals.Instance.onOpenApprovalPanel?.Invoke(
                        $"Out of Energy!\nBuy +1 Energy for {_data.CoinRefillCost} Coins?",
                        OnConfirmCoinRefill,
                        null);
                }

                return false;
            }

            bool shouldStartNewTimer = _currentEnergy >= _data.MaxEnergy || _nextRestoreUtcTicks <= 0L;
            _currentEnergy -= cost;

            if (_currentEnergy < _data.MaxEnergy && shouldStartNewTimer)
            {
                int cooldownSeconds = Mathf.Max(1, _data.RegenCooldownSeconds);
                _nextRestoreUtcTicks = DateTime.UtcNow.Ticks + TimeSpan.FromSeconds(cooldownSeconds).Ticks;
                SaveNextRestoreTicks();
            }

            SaveCurrentEnergy();
            BroadcastState(true);
            return true;
        }

        private void OnConfirmCoinRefill()
        {
            if (ScoreSignals.Instance == null || ScoreSignals.Instance.onGetMoney() < _data.CoinRefillCost)
            {
                return;
            }

            ScoreSignals.Instance.onScoreDecrease?.Invoke(ScoreTypeEnums.Money, _data.CoinRefillCost);
            AudioSignals.Instance.onPlaySound?.Invoke(AudioSoundEnums.Click);
            PitchSignals.Instance.onPlayPitch?.Invoke(PitchEnums.Click);

            OnAddEnergy(1);
        }

        private void OnAddEnergy(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            EnsureLoaded();
            EvaluateRegeneration(false);

            int maxEnergy = Mathf.Max(1, _data.MaxEnergy);
            _currentEnergy += amount;

            if (_currentEnergy >= maxEnergy && _nextRestoreUtcTicks != 0L)
            {
                _nextRestoreUtcTicks = 0L;
                SaveNextRestoreTicks();
            }

            SaveCurrentEnergy();
            BroadcastState(true);
        }

        private int OnGetCurrentEnergy()
        {
            EnsureLoaded();
            return _currentEnergy;
        }

        private int OnGetMaxEnergy()
        {
            return Mathf.Max(1, _data.MaxEnergy);
        }

        private int OnGetRemainingSeconds()
        {
            EnsureLoaded();
            return CalculateRemainingSeconds(DateTime.UtcNow.Ticks);
        }

        private int CalculateRemainingSeconds(long nowTicks)
        {
            int maxEnergy = Mathf.Max(1, _data.MaxEnergy);
            if (_currentEnergy >= maxEnergy || _nextRestoreUtcTicks <= 0L)
            {
                return 0;
            }

            int cooldownSeconds = Mathf.Max(1, _data.RegenCooldownSeconds);
            long remainingTicks = Math.Max(0L, _nextRestoreUtcTicks - nowTicks);
            double remainingSecDouble = (double)remainingTicks / TimeSpan.TicksPerSecond;
            return Mathf.Clamp(Mathf.CeilToInt((float)remainingSecDouble), 1, cooldownSeconds);
        }

        private void BroadcastState(bool forceEnergyBroadcast)
        {
            int maxEnergy = Mathf.Max(1, _data.MaxEnergy);
            int remainingSeconds = CalculateRemainingSeconds(DateTime.UtcNow.Ticks);

            if (forceEnergyBroadcast)
            {
                EnergySignals.Instance.onEnergyChanged?.Invoke(_currentEnergy, maxEnergy);
            }

            if (forceEnergyBroadcast || remainingSeconds != _lastBroadcastedSeconds)
            {
                _lastBroadcastedSeconds = remainingSeconds;
                EnergySignals.Instance.onEnergyTimerUpdated?.Invoke(remainingSeconds);
            }
        }

        private void SaveCurrentEnergy()
        {
            SaveSignals.Instance?.onSave?.Invoke(_currentEnergy, SaveLoadStates.Energy, SaveFiles.SaveFile);
        }

        private void SaveNextRestoreTicks()
        {
            SaveSignals.Instance?.onSaveLong?.Invoke(_nextRestoreUtcTicks, SaveLoadStates.EnergyNextRestoreTicks, SaveFiles.SaveFile);
        }
    }
}
