using System;
using UnityEngine;

namespace Data.ValueObject
{
    [Serializable]
    public class EnergyData
    {
        [Header("Capacity & Cost")]
        public int MaxEnergy = 5;
        public int InitialEnergy = 5;
        public int EnergyCostPerStart = 1;

        [Header("Regeneration Cooldown")]
        [Tooltip("Seconds required to regenerate +1 Energy (1200s = 20 minutes).")]
        public int RegenCooldownSeconds = 1200;

        [Header("Gameplay Rules")]
        [Tooltip("If true, Level 0 (first-time tutorial) does not consume energy.")]
        public bool IsTutorialFree = true;

        [Tooltip("If true, offers +1 Energy refill for coins via ApprovalPanel when at 0 energy and player can afford it.")]
        public bool EnableCoinRefillPrompt = true;
        public int CoinRefillCost = 100;
    }
}
