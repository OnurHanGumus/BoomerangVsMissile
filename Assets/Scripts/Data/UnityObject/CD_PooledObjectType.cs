using Enums;
using UnityEngine;

namespace Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_PooledObjectType", menuName = "Picker3D/CD_PooledObjectType", order = 0)]
    public class CD_PooledObjectType : ScriptableObject
    {
        public PoolEnums PoolEnums;
        public GameObject Prefab;
    }
}
