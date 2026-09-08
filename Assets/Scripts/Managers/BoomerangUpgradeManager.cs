using System;
using System.Collections.Generic;
using Data.UnityObject;
using Data.ValueObject;
using Enums;
using Extensions;
using Signals;
using UnityEngine;

namespace Managers
{
    public class BoomerangUpgradeManager : MonoSingleton<BoomerangUpgradeManager>
    {
        #region Serialized Variables
        [SerializeField] private CD_BoomerangUpgrades cdBoomerangUpgrades;
        #endregion

        #region Private Variables
        private readonly Dictionary<BoomerangUpgradeType, int> _upgradeLevels = new Dictionary<BoomerangUpgradeType, int>();
        private const string SaveFileName = "SaveFile.es3";
        #endregion

        public CD_BoomerangUpgrades UpgradesConfig => cdBoomerangUpgrades;

        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        private void Init()
        {
            if (cdBoomerangUpgrades == null)
            {
                cdBoomerangUpgrades = Resources.Load<CD_BoomerangUpgrades>("Data/CD_BoomerangUpgrades");
            }

            LoadUpgradeLevels();
        }

        private void LoadUpgradeLevels()
        {
            _upgradeLevels.Clear();

            // Load levels for all known upgrade types
            foreach (BoomerangUpgradeType type in Enum.GetValues(typeof(BoomerangUpgradeType)))
            {
                string key = "Upgrade_" + type.ToString();
                int level = 0;
                try
                {
                    if (ES3.KeyExists(key, SaveFileName))
                    {
                        level = ES3.Load<int>(key, SaveFileName, 0);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[BoomerangUpgradeManager] Could not load key {key}: {e.Message}");
                    level = 0;
                }

                _upgradeLevels[type] = level;
            }
        }

        public int GetLevel(BoomerangUpgradeType type)
        {
            if (_upgradeLevels.TryGetValue(type, out int level))
            {
                return level;
            }
            return 0;
        }

        public UpgradeDefinition GetDefinition(BoomerangUpgradeType type)
        {
            if (cdBoomerangUpgrades == null)
            {
                cdBoomerangUpgrades = Resources.Load<CD_BoomerangUpgrades>("Data/CD_BoomerangUpgrades");
            }
            return cdBoomerangUpgrades != null ? cdBoomerangUpgrades.GetDefinition(type) : null;
        }

        public int GetMaxLevel(BoomerangUpgradeType type)
        {
            var def = GetDefinition(type);
            return def != null ? def.MaxLevel : 10;
        }

        public bool IsMaxLevel(BoomerangUpgradeType type)
        {
            return GetLevel(type) >= GetMaxLevel(type);
        }

        public int GetCost(BoomerangUpgradeType type)
        {
            var def = GetDefinition(type);
            if (def == null)
            {
                return 0;
            }
            return def.CalculateCost(GetLevel(type));
        }

        public float GetTotalBonus(BoomerangUpgradeType type)
        {
            var def = GetDefinition(type);
            if (def == null)
            {
                return 0f;
            }
            return def.CalculateTotalBonus(GetLevel(type));
        }

        public bool CanAfford(BoomerangUpgradeType type)
        {
            if (IsMaxLevel(type))
            {
                return false;
            }

            int cost = GetCost(type);
            int currentGems = ScoreSignals.Instance.onGetGem();
            return currentGems >= cost;
        }

        public bool TryPurchaseUpgrade(BoomerangUpgradeType type)
        {
            if (IsMaxLevel(type))
            {
                return false;
            }

            int cost = GetCost(type);
            int currentGems = ScoreSignals.Instance.onGetGem();

            if (currentGems < cost)
            {
                return false;
            }

            // Deduct cost
            ScoreSignals.Instance.onScoreDecrease?.Invoke(ScoreTypeEnums.Gem, cost);

            // Increment level
            int nextLevel = GetLevel(type) + 1;
            _upgradeLevels[type] = nextLevel;

            // Save to EasySave
            string key = "Upgrade_" + type.ToString();
            try
            {
                ES3.Save(key, nextLevel, SaveFileName);
            }
            catch (Exception e)
            {
                Debug.LogError($"[BoomerangUpgradeManager] Failed to save {key}: {e.Message}");
            }

            // Notify gameplay controllers to apply new stats
            BoomerangSignals.Instance.onBoomerangStatsChanged?.Invoke();

            return true;
        }
    }
}
