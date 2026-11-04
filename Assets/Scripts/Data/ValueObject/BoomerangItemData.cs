using Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data.ValueObject
{
    [Serializable]
    public class BoomerangItemData
    {
        public int Id;
        public string Title;
        [TextArea(2, 3)]
        public string Description;
        public int Price;
        public Sprite Icon;
        public GameObject Prefab;

        [Header("Weapon Stats")]
        [Range(3f, 15f)] public float Speed = 5f;
        [Range(1f, 3.5f)] public float ReturnSpeedMultiplier = 1.25f;
        [Range(1, 5)] public int Damage = 1;
        [Range(0.5f, 2f)] public float SizeMultiplier = 1f;
        public BoomerangTierEnums Tier;

        [Header("Unlock Requirements")]
        [Tooltip("Optional custom level requirement. Set to -1 to inherit tier unlock level.")]
        public int CustomUnlockLevel = -1;

        public int GetUnlockLevel(UnityObject.CD_Boomerangs config = null)
        {
            if (CustomUnlockLevel >= 0)
            {
                return CustomUnlockLevel;
            }

            if (config != null)
            {
                return config.GetRequiredUnlockLevel(Tier);
            }

            return GetDefaultTierUnlockLevel(Tier);
        }

        public static int GetDefaultTierUnlockLevel(BoomerangTierEnums tier)
        {
            switch (tier)
            {
                case BoomerangTierEnums.Tier1:
                    return 0;
                case BoomerangTierEnums.Tier2:
                    return 35;
                case BoomerangTierEnums.Tier3:
                    return 160;
                case BoomerangTierEnums.Tier4:
                    return 400;
                default:
                    return 0;
            }
        }

        [Header("Visual Theme")]
        public Color ThemeColor = Color.cyan;

        [Header("Hand Grip Transform")]
        public Vector3 HandLocalPosition = new Vector3(0.106f, 0.425f, -0.144f);
        public Vector3 HandLocalEulerAngles = new Vector3(19.138f, 52.601f, -105.943f);
    }
}
