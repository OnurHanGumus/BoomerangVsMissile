using DG.Tweening;
using Data.UnityObject;
using Data.ValueObject;
using Enums;
using Interfaces;
using Managers;
using Signals;
using UnityEngine;

namespace Controllers.Missile.Abilities
{
    public class ArmoredMissileAbility : MonoBehaviour, IMissileDamageHandler, IMissileHealthProvider
    {
        [Header("Armor Data")]
        [SerializeField] private CD_ArmoredMissile cdArmoredMissile;

        private ArmoredMissileData _data;
        private MissileManager _manager;
        private MeshRenderer _meshRenderer;

        public ArmoredMissileData Data
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

        public void Initialize(MissileManager manager)
        {
            _manager = manager;
            _meshRenderer = GetComponentInChildren<MeshRenderer>();
            LoadData();
        }

        private void LoadData()
        {
            if (cdArmoredMissile == null)
            {
                cdArmoredMissile = Resources.Load<CD_ArmoredMissile>("Data/Missiles/CD_ArmoredMissile");
            }
            _data = cdArmoredMissile != null && cdArmoredMissile.Data != null 
                ? cdArmoredMissile.Data 
                : new ArmoredMissileData();
        }

        public int GetAdditionalHealth()
        {
            return Data != null ? Data.BonusHealth : 1;
        }

        public void OnDamageTaken(int damage, int currentHealth)
        {
            // 1. Play metallic deflection sound
            AudioSignals.Instance.onPlaySound(AudioSoundEnums.Pitch);

            // 2. Trigger micro hit-stop & camera shake via signal
            MissileSignals.Instance.onMissileArmorHit?.Invoke();

            // 3. Physical flinch / recoil
            transform.DOPunchScale(Vector3.one * 0.25f, 0.2f, 10, 1).SetUpdate(true);

            // 4. Visual flash to indicate armor crack / damaged state
            if (_meshRenderer != null && _meshRenderer.material != null)
            {
                _meshRenderer.material.DOColor(new Color(1f, 0.35f, 0.1f), 0.1f)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetUpdate(true);
            }
        }
    }
}
