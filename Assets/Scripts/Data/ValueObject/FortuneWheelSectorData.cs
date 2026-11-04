using System;
using Enums;
using UnityEngine;

namespace Data.ValueObject
{
    [Serializable]
    public class FortuneWheelSectorData
    {
        public FortuneWheelPrizeType PrizeType = FortuneWheelPrizeType.Coin;
        public int Amount = 10;
        [Tooltip("Relative probability weight (higher = more likely to be selected).")]
        public int Weight = 10;
        [Tooltip("Optional text label override (e.g. '100', '1 Life'). If blank, auto-generates.")]
        public string CustomLabel = "";
        [Tooltip("Optional icon sprite override. If null, uses default Coin or Life sprite.")]
        public Sprite CustomIcon;

        public FortuneWheelSectorData() { }

        public FortuneWheelSectorData(FortuneWheelPrizeType prizeType, int amount, int weight, string customLabel = "")
        {
            PrizeType = prizeType;
            Amount = amount;
            Weight = weight;
            CustomLabel = customLabel;
        }
    }
}

