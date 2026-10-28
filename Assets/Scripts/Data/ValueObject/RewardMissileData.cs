using System;
using Enums;
using UnityEngine;

namespace Data.ValueObject
{
    [Serializable]
    public class RewardMissileData
    {
        [Header("Reward Roll Mode")]
        public RewardSelectionMode SelectionMode = RewardSelectionMode.RandomSurprise;
        public BoomerangStatEnums FixedStatType = BoomerangStatEnums.Damage;

        [Header("Timing & Stacking")]
        [Min(0.5f)] public float Duration = 6.0f;
        [Min(1)] public int MaxStacks = 2;
        public bool RefreshDurationOnStack = true;

        [Header("Stat Multipliers (Per Stack)")]
        [Tooltip("1.0 = +100% base damage per stack (x2 at 1 stack, x3 at 2 stacks)")]
        [Min(0.1f)] public float DamageMultiplierBonus = 1.0f;

        [Tooltip("0.6 = +60% base size per stack (x1.6 at 1 stack, x2.2 at 2 stacks)")]
        [Min(0.1f)] public float SizeMultiplierBonus = 0.6f;
        [Min(0.5f)] public float MaxEffectiveSize = 2.4f;

        [Tooltip("0.5 = +50% throw & return speed per stack (x1.5 at 1 stack, x2.0 at 2 stacks)")]
        [Min(0.1f)] public float SpeedMultiplierBonus = 0.5f;
        [Min(5f)] public float MaxEffectiveSpeed = 18.0f;
    }
}
