using System.Collections.Generic;
using Data.ValueObject;
using UnityEngine;
namespace Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_Prices", menuName = "Picker3D/CD_Prices", order = 0)]
    public class CD_Prices : ScriptableObject
    {
        public ItemPricesData Data;

    }
}