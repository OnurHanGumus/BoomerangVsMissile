using System;
using Data.UnityObject;

namespace Data.ValueObject
{
    [Serializable]
    public struct PooledObject
    {
        public CD_PooledObjectType TypeData;
        public int Amounts;
    }
}
