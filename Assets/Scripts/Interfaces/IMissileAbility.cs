using UnityEngine;
using Managers;

namespace Interfaces
{
    public interface IMissileAbility
    {
        void Initialize(MissileManager manager);
    }

    public interface IMissileDeathEffect : IMissileAbility
    {
        /// <summary>
        /// Called when the missile explodes / dies.
        /// Returns true if this effect handled custom explosion audio, false otherwise.
        /// </summary>
        bool OnMissileDeath(Vector3 position, bool isLevelEnd);
    }

    public interface IMissileDamageHandler : IMissileAbility
    {
        /// <summary>
        /// Called when the missile takes damage and remains alive (currentHealth > 0).
        /// </summary>
        void OnDamageTaken(int damage, int currentHealth);
    }

    public interface IMissileHealthProvider : IMissileAbility
    {
        int GetAdditionalHealth();
    }
}
