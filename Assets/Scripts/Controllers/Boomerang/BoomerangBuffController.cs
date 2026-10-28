using System;
using System.Collections.Generic;
using Data.ValueObject;
using Enums;
using Signals;
using UnityEngine;

namespace Controllers.Boomerang
{
    public class BoomerangBuffController : MonoBehaviour
    {
        #region Self Variables

        #region Private Variables
        private sealed class ActiveBuffEntry
        {
            public int Stacks;
            public float RemainingTime;
            public float TotalDuration = 1f;
            public float BonusPerStack;
            public float MaxCap = float.MaxValue;
        }

        private static readonly BoomerangStatEnums[] AllStatTypes =
            (BoomerangStatEnums[])Enum.GetValues(typeof(BoomerangStatEnums));

        private readonly Dictionary<BoomerangStatEnums, ActiveBuffEntry> _activeBuffs =
            new Dictionary<BoomerangStatEnums, ActiveBuffEntry>(3);

        private bool _isSubscribed;
        #endregion

        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }

        private void Init()
        {
            for (int i = 0; i < AllStatTypes.Length; i++)
            {
                BoomerangStatEnums stat = AllStatTypes[i];
                if (!_activeBuffs.ContainsKey(stat))
                {
                    _activeBuffs.Add(stat, new ActiveBuffEntry());
                }
            }
        }

        #region Event Subscription
        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            if (_isSubscribed)
            {
                return;
            }

            if (BoomerangSignals.Instance != null)
            {
                BoomerangSignals.Instance.onRewardBuffTriggered += OnRewardBuffTriggered;
            }

            if (CoreGameSignals.Instance != null)
            {
                CoreGameSignals.Instance.onLevelFailed += ClearAllBuffs;
                CoreGameSignals.Instance.onLevelSuccessful += ClearAllBuffs;
                CoreGameSignals.Instance.onRestartLevel += ClearAllBuffs;
                CoreGameSignals.Instance.onReset += ClearAllBuffs;
            }

            _isSubscribed = true;
        }

        private void UnsubscribeEvents()
        {
            if (!_isSubscribed)
            {
                return;
            }

            if (BoomerangSignals.Instance != null)
            {
                BoomerangSignals.Instance.onRewardBuffTriggered -= OnRewardBuffTriggered;
            }

            if (CoreGameSignals.Instance != null)
            {
                CoreGameSignals.Instance.onLevelFailed -= ClearAllBuffs;
                CoreGameSignals.Instance.onLevelSuccessful -= ClearAllBuffs;
                CoreGameSignals.Instance.onRestartLevel -= ClearAllBuffs;
                CoreGameSignals.Instance.onReset -= ClearAllBuffs;
            }

            _isSubscribed = false;
        }
        #endregion

        private void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f)
            {
                return;
            }

            bool anyExpired = false;
            for (int i = 0; i < AllStatTypes.Length; i++)
            {
                BoomerangStatEnums stat = AllStatTypes[i];
                ActiveBuffEntry entry = _activeBuffs[stat];
                if (entry.Stacks <= 0)
                {
                    continue;
                }

                entry.RemainingTime -= dt;
                if (entry.RemainingTime <= 0f)
                {
                    entry.Stacks = 0;
                    entry.RemainingTime = 0f;
                    anyExpired = true;

                    if (BoomerangSignals.Instance != null)
                    {
                        BoomerangSignals.Instance.onBuffTimerUpdated?.Invoke(stat, 0, 0f, entry.TotalDuration);
                    }
                }
                else if (BoomerangSignals.Instance != null)
                {
                    BoomerangSignals.Instance.onBuffTimerUpdated?.Invoke(
                        stat,
                        entry.Stacks,
                        entry.RemainingTime,
                        entry.TotalDuration);
                }
            }

            if (anyExpired && BoomerangSignals.Instance != null)
            {
                BoomerangSignals.Instance.onBoomerangStatsChanged?.Invoke();
            }
        }

        private void OnRewardBuffTriggered(BoomerangStatEnums stat, RewardMissileData data, Vector3 worldPos)
        {
            if (!_activeBuffs.TryGetValue(stat, out ActiveBuffEntry entry))
            {
                entry = new ActiveBuffEntry();
                _activeBuffs[stat] = entry;
            }

            float duration = data != null ? Mathf.Max(0.5f, data.Duration) : 6.0f;
            int maxStacks = data != null ? Mathf.Max(1, data.MaxStacks) : 2;
            bool refreshOnStack = data == null || data.RefreshDurationOnStack;

            int previousStacks = entry.Stacks;
            entry.Stacks = Mathf.Min(entry.Stacks + 1, maxStacks);

            if (previousStacks == 0 || refreshOnStack)
            {
                entry.RemainingTime = duration;
                entry.TotalDuration = duration;
            }

            switch (stat)
            {
                case BoomerangStatEnums.Damage:
                    entry.BonusPerStack = data != null ? data.DamageMultiplierBonus : 1.0f;
                    entry.MaxCap = float.MaxValue;
                    break;
                case BoomerangStatEnums.Size:
                    entry.BonusPerStack = data != null ? data.SizeMultiplierBonus : 0.6f;
                    entry.MaxCap = data != null ? data.MaxEffectiveSize : 2.4f;
                    break;
                case BoomerangStatEnums.HyperSpeed:
                    entry.BonusPerStack = data != null ? data.SpeedMultiplierBonus : 0.5f;
                    entry.MaxCap = data != null ? data.MaxEffectiveSpeed : 18.0f;
                    break;
            }

            if (BoomerangSignals.Instance != null)
            {
                BoomerangSignals.Instance.onBuffTimerUpdated?.Invoke(
                    stat,
                    entry.Stacks,
                    entry.RemainingTime,
                    entry.TotalDuration);

                if (entry.Stacks != previousStacks)
                {
                    BoomerangSignals.Instance.onBoomerangStatsChanged?.Invoke();
                }
            }
        }

        public void ClearAllBuffs()
        {
            bool hadActiveBuffs = false;
            for (int i = 0; i < AllStatTypes.Length; i++)
            {
                BoomerangStatEnums stat = AllStatTypes[i];
                if (!_activeBuffs.TryGetValue(stat, out ActiveBuffEntry entry))
                {
                    continue;
                }

                if (entry.Stacks > 0)
                {
                    hadActiveBuffs = true;
                    entry.Stacks = 0;
                    entry.RemainingTime = 0f;

                    if (BoomerangSignals.Instance != null)
                    {
                        BoomerangSignals.Instance.onBuffTimerUpdated?.Invoke(stat, 0, 0f, entry.TotalDuration);
                    }
                }
            }

            if (hadActiveBuffs && BoomerangSignals.Instance != null)
            {
                BoomerangSignals.Instance.onBoomerangStatsChanged?.Invoke();
            }
        }

        public int ApplyDamageBuff(int baseDamage)
        {
            if (_activeBuffs.TryGetValue(BoomerangStatEnums.Damage, out ActiveBuffEntry entry) && entry.Stacks > 0)
            {
                float mult = 1f + (entry.Stacks * entry.BonusPerStack);
                return Mathf.Max(1, Mathf.RoundToInt(baseDamage * mult));
            }

            return baseDamage;
        }

        public float ApplySizeBuff(float baseSize)
        {
            if (_activeBuffs.TryGetValue(BoomerangStatEnums.Size, out ActiveBuffEntry entry) && entry.Stacks > 0)
            {
                float mult = 1f + (entry.Stacks * entry.BonusPerStack);
                return Mathf.Min(baseSize * mult, entry.MaxCap);
            }

            return baseSize;
        }

        public float ApplySpeedBuff(float baseSpeed)
        {
            if (_activeBuffs.TryGetValue(BoomerangStatEnums.HyperSpeed, out ActiveBuffEntry entry) && entry.Stacks > 0)
            {
                float mult = 1f + (entry.Stacks * entry.BonusPerStack);
                return Mathf.Min(baseSpeed * mult, entry.MaxCap);
            }

            return baseSpeed;
        }

        public float ApplyReturnSpeedBuff(float baseReturnMultiplier)
        {
            if (_activeBuffs.TryGetValue(BoomerangStatEnums.HyperSpeed, out ActiveBuffEntry entry) && entry.Stacks > 0)
            {
                float mult = 1f + (entry.Stacks * entry.BonusPerStack);
                return baseReturnMultiplier * mult;
            }

            return baseReturnMultiplier;
        }

        public float GetReturnSplineSpeedMultiplier()
        {
            if (_activeBuffs.TryGetValue(BoomerangStatEnums.HyperSpeed, out ActiveBuffEntry entry) && entry.Stacks > 0)
            {
                return 1f + (entry.Stacks * entry.BonusPerStack);
            }

            return 1f;
        }
    }
}
