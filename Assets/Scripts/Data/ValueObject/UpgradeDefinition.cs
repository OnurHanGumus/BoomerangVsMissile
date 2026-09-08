using System;
using Enums;
using UnityEngine;

namespace Data.ValueObject
{
    [Serializable]
    public class UpgradeDefinition
    {
        public BoomerangUpgradeType Type;
        public string Title;
        public Sprite Icon;
        public int MaxLevel = 10;
        public float BonusPerLevel = 0.5f;
        public int BasePrice = 50;
        public int PriceIncreasePerLevel = 25;
        public string ValueDisplayFormat = "+{0:F2}";

        public int CalculateCost(int currentLevel)
        {
            if (currentLevel >= MaxLevel)
            {
                return 0;
            }
            return BasePrice + (currentLevel * PriceIncreasePerLevel);
        }

        public float CalculateTotalBonus(int currentLevel)
        {
            int level = Mathf.Clamp(currentLevel, 0, MaxLevel);
            return level * BonusPerLevel;
        }

        public string FormatBonus(int currentLevel)
        {
            float bonus = CalculateTotalBonus(currentLevel);
            try
            {
                return string.Format(ValueDisplayFormat, bonus);
            }
            catch
            {
                return $"+{bonus:F2}";
            }
        }
    }
}
