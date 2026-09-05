using Enums;
using Interfaces;
using Managers;
using Signals;
using UnityEngine;

namespace Controllers.Missile.Abilities
{
    public class PinkMissileAbility : MonoBehaviour, IMissileDeathEffect
    {
        private MissileManager _manager;

        public void Initialize(MissileManager manager)
        {
            _manager = manager;
        }

        public bool OnMissileDeath(Vector3 position, bool isLevelEnd)
        {
            if (!isLevelEnd)
            {
                MissileSignals.Instance.onPinkMissileDestroyed?.Invoke();
            }
            AudioSignals.Instance.onPlaySound(AudioSoundEnums.Explosion1);
            return true; // Handled custom explosion audio
        }
    }
}
