using System.Collections.Generic;
using Data.ValueObject;
using Enums;
using UnityEngine;

namespace Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_Boomerangs", menuName = "Boomerang/CD_Boomerangs", order = 1)]
    public class CD_Boomerangs : ScriptableObject
    {
        [Header("Tier Level Requirements")]
        public int Tier1UnlockLevel = 0;
        public int Tier2UnlockLevel = 35;
        public int Tier3UnlockLevel = 160;
        public int Tier4UnlockLevel = 400;

        [Header("Boomerang Catalog")]
        public List<BoomerangItemData> Boomerangs = new List<BoomerangItemData>();

        public int GetRequiredUnlockLevel(BoomerangTierEnums tier)
        {
            switch (tier)
            {
                case BoomerangTierEnums.Tier1:
                    return Tier1UnlockLevel;
                case BoomerangTierEnums.Tier2:
                    return Tier2UnlockLevel;
                case BoomerangTierEnums.Tier3:
                    return Tier3UnlockLevel;
                case BoomerangTierEnums.Tier4:
                    return Tier4UnlockLevel;
                default:
                    return 0;
            }
        }

        public BoomerangItemData GetBoomerang(int id)
        {
            if (Boomerangs == null)
            {
                return null;
            }

            for (int i = 0; i < Boomerangs.Count; i++)
            {
                if (Boomerangs[i] != null && Boomerangs[i].Id == id)
                {
                    return Boomerangs[i];
                }
            }

            return null;
        }
    }
}
