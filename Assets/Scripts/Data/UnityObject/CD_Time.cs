using Data.ValueObject;
using UnityEngine;

namespace Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_Time", menuName = "Picker3D/CD_Time", order = 0)]
    public class CD_Time : ScriptableObject
    {
        public TimeData Data;
    }
}