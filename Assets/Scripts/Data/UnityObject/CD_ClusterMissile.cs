using Data.ValueObject;
using UnityEngine;

namespace Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_ClusterMissile", menuName = "Picker3D/CD_ClusterMissile", order = 1)]
    public class CD_ClusterMissile : ScriptableObject
    {
        public ClusterMissileData Data;
    }
}
