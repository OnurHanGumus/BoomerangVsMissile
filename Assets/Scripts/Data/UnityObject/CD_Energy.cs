using Data.ValueObject;
using UnityEngine;

namespace Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_Energy", menuName = "BoomerangVsMissile/CD_Energy", order = 0)]
    public class CD_Energy : ScriptableObject
    {
        public EnergyData Data = new EnergyData();
    }
}
