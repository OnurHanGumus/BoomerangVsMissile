using System.Collections.Generic;
using Data.ValueObject;
using UnityEngine;

namespace Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_PoolSettings", menuName = "Picker3D/CD_PoolSettings", order = 0)]
    public class CD_PoolSettings : ScriptableObject
    {
        public List<PooledObject> pooledObjects = new List<PooledObject>();
    }
}
