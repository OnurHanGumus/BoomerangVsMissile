using System.Collections.Generic;
using Data.ValueObject;
using UnityEngine;

namespace Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_LevelEnvironment", menuName = "Boomerang/CD_LevelEnvironment", order = 6)]
    public class CD_LevelEnvironment : ScriptableObject
    {
        public List<LevelEnvironmentColorData> LevelColors = new List<LevelEnvironmentColorData>();
    }
}
