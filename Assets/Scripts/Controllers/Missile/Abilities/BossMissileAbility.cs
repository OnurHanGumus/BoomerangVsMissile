using System.Collections.Generic;
using Data.UnityObject;
using Data.ValueObject;
using Enums;
using Interfaces;
using Managers;
using Signals;
using UnityEngine;

namespace Controllers.Missile.Abilities
{
    public class BossMissileAbility : MonoBehaviour, IMissileDeathEffect
    {
        [Header("Boss Data")]
        [SerializeField] private CD_BossMissile cdBossMissile;

        private BossMissileData _data;
        public BossMissileData Data
        {
            get
            {
                if (_data == null)
                {
                    LoadData();
                }
                return _data;
            }
        }

        public float ExplosionRadius => Data != null ? Data.ExplosionRadius : 4f;

        private MissileManager _manager;

        public void Initialize(MissileManager manager)
        {
            _manager = manager;
            LoadData();
        }

        private void LoadData()
        {
            if (cdBossMissile == null)
            {
                cdBossMissile = Resources.Load<CD_BossMissile>("Data/Missiles/CD_BossMissile");
            }
            _data = cdBossMissile != null && cdBossMissile.Data != null 
                ? cdBossMissile.Data 
                : new BossMissileData();
        }

        public bool OnMissileDeath(Vector3 position, bool isLevelEnd)
        {
            if (isLevelEnd)
            {
                return false;
            }

            float radius = ExplosionRadius;
            Collider[] colliders = Physics.OverlapSphere(position, radius);
            HashSet<MissileManager> affectedMissiles = new HashSet<MissileManager>();

            for (int i = 0; i < colliders.Length; i++)
            {
                var col = colliders[i];
                if (col == null || !col.enabled || !col.gameObject.activeInHierarchy)
                {
                    continue;
                }

                var otherMissile = col.GetComponentInParent<MissileManager>();
                if (otherMissile != null && otherMissile != _manager)
                {
                    if (otherMissile.gameObject.activeInHierarchy && otherMissile.gameObject.activeSelf && !otherMissile.IsDead)
                    {
                        affectedMissiles.Add(otherMissile);
                    }
                }
            }

            foreach (var missile in affectedMissiles)
            {
                if (missile != null && missile.gameObject.activeInHierarchy && !missile.IsDead)
                {
                    missile.Explode();
                }
            }

            AudioSignals.Instance.onPlaySound(AudioSoundEnums.Explosion1);
            return true; // Handled custom explosion audio
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, ExplosionRadius);
        }
    }
}
