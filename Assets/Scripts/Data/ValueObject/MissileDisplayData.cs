using System;
using Enums;
using UnityEngine;

namespace Data.ValueObject
{
    [Serializable]
    public class MissileDisplayData
    {
        public MissileEnums Type;
        public string DisplayName;
        public Sprite Icon;

        public MissileDisplayData()
        {
        }

        public MissileDisplayData(MissileEnums type, string displayName, Sprite icon = null)
        {
            Type = type;
            DisplayName = displayName;
            Icon = icon;
        }
    }
}
