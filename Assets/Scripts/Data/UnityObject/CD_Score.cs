using Data.ValueObject;
using UnityEngine;

namespace Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_Score", menuName = "Picker3D/CD_Score", order = 0)]
    public class CD_Score : ScriptableObject
    {
        public ScoreData Data;
    }
}