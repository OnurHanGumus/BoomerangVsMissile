using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Data.ValueObject
{
    [Serializable]
    public class FortuneWheelData
    {
        [Header("Cooldown Settings")]
        [Tooltip("Hours between free daily spins (default 24h).")]
        public float CooldownHours = 24f;

        [Header("Spin Animation Settings")]
        [Tooltip("Total spin animation duration in seconds.")]
        public float SpinDuration = 4.2f;

        [Tooltip("Minimum full revolutions during a spin.")]
        public int MinFullSpins = 5;

        [Tooltip("Maximum full revolutions during a spin.")]
        public int MaxFullSpins = 7;

        [Tooltip("Angular jitter in degrees (+/-) so pointer does not stop on border.")]
        public float JitterDegrees = 9f;

        [Header("Default Prize Sectors (10 Sectors)")]
        public List<FortuneWheelSectorData> Sectors = new List<FortuneWheelSectorData>()
        {
            new FortuneWheelSectorData(FortuneWheelPrizeType.Coin, 100, 2, "100"),
            new FortuneWheelSectorData(FortuneWheelPrizeType.Life, 1, 25, "+1"),
            new FortuneWheelSectorData(FortuneWheelPrizeType.Coin, 25, 18, "25"),
            new FortuneWheelSectorData(FortuneWheelPrizeType.Coin, 10, 35, "10"),
            new FortuneWheelSectorData(FortuneWheelPrizeType.Life, 2, 10, "+2"),
            new FortuneWheelSectorData(FortuneWheelPrizeType.Coin, 50, 6, "50"),
            new FortuneWheelSectorData(FortuneWheelPrizeType.Life, 1, 25, "+1"),
            new FortuneWheelSectorData(FortuneWheelPrizeType.Coin, 25, 18, "25"),
            new FortuneWheelSectorData(FortuneWheelPrizeType.Coin, 10, 35, "10"),
            new FortuneWheelSectorData(FortuneWheelPrizeType.Life, 3, 4, "+3")
        };
    }
}

