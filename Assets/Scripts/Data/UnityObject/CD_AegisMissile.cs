using Data.ValueObject;
using UnityEngine;

namespace Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_AegisMissile", menuName = "Picker3D/CD_AegisMissile", order = 1)]
    public class CD_AegisMissile : ScriptableObject
    {
        public AegisMissileData Data;
    }
}
