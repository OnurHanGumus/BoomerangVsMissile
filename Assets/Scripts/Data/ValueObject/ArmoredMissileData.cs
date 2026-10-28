using System;

namespace Data.ValueObject
{
    [Serializable]
    public class ArmoredMissileData
    {
        public bool ArmorBreakDealsDamage = true;
        public float DeflectPitch = 0.6f;
    }
}
