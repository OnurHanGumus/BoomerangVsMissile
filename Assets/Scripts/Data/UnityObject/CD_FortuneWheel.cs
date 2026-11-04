using Data.ValueObject;
using UnityEngine;

namespace Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_FortuneWheel", menuName = "BoomerangVsMissile/CD_FortuneWheel", order = 0)]
    public class CD_FortuneWheel : ScriptableObject
    {
        public FortuneWheelData Data = new FortuneWheelData();
    }
}

