using Data.ValueObject;
using UnityEngine;

namespace Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_Comments", menuName = "Picker3D/CD_Comments", order = 0)]
    public class CD_Comments : ScriptableObject
    {
        public ComboCommentsData Data;
    }
}