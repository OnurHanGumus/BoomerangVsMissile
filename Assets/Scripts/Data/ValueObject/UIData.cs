using System;
using UnityEngine;

namespace Data.ValueObject
{
    [Serializable]
    public class UIData
    {
        public float AnimationTime = 0.5f;
        public float ComboInputIncreaseAmount = 1f;

        [Header("Panel Transitions")]
        public float PanelOpenDuration = 0.30f;
        public float PanelCloseDuration = 0.20f;
        public float PopStartScaleMultiplier = 0.85f;
        public float PopCloseScaleMultiplier = 0.92f;

        [Header("Interactive Units")]
        public float ButtonPressScaleMultiplier = 0.92f;
        public float ButtonPressDuration = 0.08f;
        public float ButtonReleaseDuration = 0.20f;
        public float CoinRollDuration = 0.35f;
    }
}