using Data.ValueObject;
using UnityEngine;

namespace Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_Money", menuName = "Picker3D/CD_Money", order = 0)]
    public class CD_Money : ScriptableObject
    {
        public GainMoneyData Data;
    }
}