using System;
using UnityEngine;

namespace Data.ValueObject
{
    [Serializable]
    public class TimeData
    {
        public float ClickingTimeScale = 0.5f;
        public float NormalTimeScale = 1f;

        [Header("Hit-Stop Settings")]
        public float HitStopDuration = 0.05f;
        public float HitStopTimeScale = 0.02f;
    }
}