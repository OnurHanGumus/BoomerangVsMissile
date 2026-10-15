using System.Collections.Generic;
using Data.ValueObject;
using UnityEngine;

namespace Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_Boomerangs", menuName = "Boomerang/CD_Boomerangs", order = 1)]
    public class CD_Boomerangs : ScriptableObject
    {
        public List<BoomerangItemData> Boomerangs = new List<BoomerangItemData>();

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
