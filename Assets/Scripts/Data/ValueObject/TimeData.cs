using System;
using UnityEngine;

namespace Data.ValueObject
{
    [Serializable]
    public class TimeData
    {
        public float ClickingTimeScale = 0.5f;
        public float NormalTimeScale = 1f;
        public float MissingBoomerangTimeScale = 0.05f;
    }
}