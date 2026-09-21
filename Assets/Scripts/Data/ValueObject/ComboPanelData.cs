using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Data.ValueObject
{
    [Serializable]
    public struct ComboTierGradientData
    {
        public Color TopLeft;
        public Color TopRight;
        public Color BottomLeft;
        public Color BottomRight;

        public ComboTierGradientData(Color topLeft, Color topRight, Color bottomLeft, Color bottomRight)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
        }

        public VertexGradient ToVertexGradient()
        {
            return new VertexGradient(TopLeft, TopRight, BottomLeft, BottomRight);
        }
    }

    [Serializable]
    public class ComboPanelData
    {
        [Header("Animation Settings")]
        public float TargetScale = 1f;
        public float ScaleDuration = 0.5f;
        public float DisplayDuration = 1f;
        public Ease ScaleEase = Ease.Flash;

        [Header("Audio Feedback Settings")]
        public float BasePitch = 1.0f;
        public float PitchStep = 0.05f;
        public float MaxPitchOffset = 0.25f;

        [Header("Fallback Settings")]
        public int DefaultMoneyMultiplier = 50;
        public string DefaultComment = "COMBO!";

        [Header("Text Display Settings")]
        public string MoneyColorHex = "#FFE600";
        public int MoneyTextSizePercent = 70;

        [Header("Tier Gradients")]
        public List<ComboTierGradientData> TierGradients = new List<ComboTierGradientData>
        {
            // Tier 0: 2 Hits - Electric Cyan & Mint
            new ComboTierGradientData(
                new Color(0.0f, 0.95f, 1.0f),
                new Color(0.0f, 1.0f, 0.7f),
                new Color(1.0f, 1.0f, 1.0f),
                new Color(0.6f, 0.95f, 1.0f)
            ),
            // Tier 1: 3 Hits - Vibrant Gold & Warm Amber
            new ComboTierGradientData(
                new Color(1.0f, 0.9f, 0.0f),
                new Color(1.0f, 0.65f, 0.0f),
                new Color(1.0f, 1.0f, 0.9f),
                new Color(1.0f, 0.8f, 0.2f)
            ),
            // Tier 2: 4 Hits - Fiery Flame Orange & Hot Coral
            new ComboTierGradientData(
                new Color(1.0f, 0.45f, 0.0f),
                new Color(1.0f, 0.15f, 0.35f),
                new Color(1.0f, 0.9f, 0.8f),
                new Color(1.0f, 0.3f, 0.1f)
            ),
            // Tier 3: 5 Hits - Cosmic Neon Violet & Electric Magenta
            new ComboTierGradientData(
                new Color(0.85f, 0.2f, 1.0f),
                new Color(1.0f, 0.1f, 0.65f),
                new Color(1.0f, 0.85f, 1.0f),
                new Color(0.6f, 0.15f, 0.95f)
            ),
            // Tier 4+: 6+ Hits - Radiant Hyper Crimson & Sunfire Gold
            new ComboTierGradientData(
                new Color(1.0f, 0.05f, 0.3f),
                new Color(1.0f, 0.85f, 0.1f),
                new Color(1.0f, 1.0f, 1.0f),
                new Color(1.0f, 0.1f, 0.8f)
            )
        };
    }
}
