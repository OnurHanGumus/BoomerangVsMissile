using Data.ValueObject;
using UnityEngine;

namespace Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_Tutorial", menuName = "Picker3D/CD_Tutorial", order = 0)]
    public class CD_Tutorial : ScriptableObject
    {
        public TutorialData Data;
    }
}