using System.Collections.Generic;
using Data.ValueObject;
using Enums;
using UnityEngine;

namespace Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_BoomerangUpgrades", menuName = "Boomerang/CD_BoomerangUpgrades", order = 2)]
    public class CD_BoomerangUpgrades : ScriptableObject
    {
        public List<UpgradeDefinition> Upgrades = new List<UpgradeDefinition>();

        public UpgradeDefinition GetDefinition(BoomerangUpgradeType type)
        {
            if (Upgrades == null)
            {
                return null;
            }

            for (int i = 0; i < Upgrades.Count; i++)
            {
                if (Upgrades[i] != null && Upgrades[i].Type == type)
                {
                    return Upgrades[i];
                }
            }
            return null;
        }
    }
}
