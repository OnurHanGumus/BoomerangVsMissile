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
    public class ArmoredMissileAbility : MonoBehaviour, IMissileDamageFilter, IMissileDamageHandler
    {
        #region Self Variables

        #region Public Variables
        public bool IsArmorIntact => _isArmorIntact;

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
        #endregion

        #region Serialized Variables
        [Header("Armor Data")]
        [SerializeField] private CD_ArmoredMissile cdArmoredMissile;
        [SerializeField] private Transform armorVisual;
        #endregion

        #region Private Variables
        private ArmoredMissileData _data;
        private MissileManager _manager;
        private SpriteRenderer _armorSpriteRenderer;
        private Vector3 _initialArmorScale = Vector3.one;
        private Color _initialArmorColor = Color.white;
        private bool _hasCachedDefaults = false;
        private bool _isArmorIntact = true;
        private bool _brokeArmorOnPendingHit = false;

        private Sequence _armorBreakSequence;
        private Tween _deflectScaleTween;
        private Tween _deflectColorTween;
        #endregion

        #endregion

        private void Awake()
        {
            EnsureInitialized();
        }

        public void Initialize(MissileManager manager)
        {
            _manager = manager;
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_manager == null)
            {
                _manager = GetComponent<MissileManager>();
            }

            LoadData();
            CacheVisualDefaults();
        }

        private void CacheVisualDefaults()
        {
            if (armorVisual == null)
            {
                armorVisual = transform.Find("Parent/ArmorVisual");
                if (armorVisual == null)
                {
                    armorVisual = transform.Find("ArmorVisual");
                }
            }

            if (armorVisual != null && _armorSpriteRenderer == null)
            {
                _armorSpriteRenderer = armorVisual.GetComponent<SpriteRenderer>();
            }

            if (!_hasCachedDefaults)
            {
                if (armorVisual != null)
                {
                    _initialArmorScale = armorVisual.localScale;
                }
                if (_armorSpriteRenderer != null)
                {
                    _initialArmorColor = _armorSpriteRenderer.color;
                }
                _hasCachedDefaults = true;
            }
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

        private void OnEnable()
        {
            EnsureInitialized();
            _isArmorIntact = true;
            _brokeArmorOnPendingHit = false;
            KillTweens();
            RestoreVisualDefaults();
        }

        private void OnDisable()
        {
            _brokeArmorOnPendingHit = false;
            KillTweens();
        }

        private void RestoreVisualDefaults()
        {
            if (armorVisual != null)
            {
                armorVisual.localScale = _initialArmorScale;
                armorVisual.gameObject.SetActive(true);
            }

            if (_armorSpriteRenderer != null)
            {
                _armorSpriteRenderer.color = _initialArmorColor;
            }
        }

        private void KillTweens()
        {
            _armorBreakSequence?.Kill();
            _deflectScaleTween?.Kill();
            _deflectColorTween?.Kill();
        }

        public bool FilterHit(ref MissileHitContext ctx)
        {
            if (!_isArmorIntact)
            {
                return true;
            }

            if (!ctx.IsFullyCharged)
            {
                PlayDeflectFeedback();
                return false;
            }

            _isArmorIntact = false;

            if (!Data.ArmorBreakDealsDamage)
            {
                PlayArmorBreakFeedback();
                return false;
            }

            _brokeArmorOnPendingHit = true;
            ctx.SuppressDefaultHitFeedback = true;
            return true;
        }

        public void OnDamageTaken(int damage, int currentHealth)
        {
            if (!_brokeArmorOnPendingHit)
            {
                return;
            }

            _brokeArmorOnPendingHit = false;
            PlayArmorBreakFeedback();
        }

        private void PlayDeflectFeedback()
        {
            AudioSignals.Instance.onPlaySoundWithPitch?.Invoke(AudioSoundEnums.ShieldBroke, Data.DeflectPitch);
            PitchSignals.Instance.onPlayPitch?.Invoke(PitchEnums.Hit);

            if (armorVisual != null && armorVisual.gameObject.activeInHierarchy)
            {
                _deflectScaleTween?.Kill();
                armorVisual.localScale = _initialArmorScale;
                _deflectScaleTween = armorVisual
                    .DOPunchScale(_initialArmorScale * 0.3f, 0.16f, 8, 1f)
                    .SetUpdate(true)
                    .SetLink(gameObject);
            }

            if (_armorSpriteRenderer != null)
            {
                _deflectColorTween?.Kill();
                _armorSpriteRenderer.color = _initialArmorColor;
                _deflectColorTween = _armorSpriteRenderer
                    .DOColor(new Color(1f, 0.55f, 0.45f, _initialArmorColor.a), 0.08f)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetUpdate(true)
                    .SetLink(gameObject);
            }
        }

        private void PlayArmorBreakFeedback()
        {
            if (PoolSignals.Instance != null)
            {
                GameObject spark = PoolSignals.Instance.onGetObject?.Invoke(PoolEnums.HitSpark);
                if (spark != null)
                {
                    spark.transform.position = transform.position;
                    spark.SetActive(true);
                }
            }

            AudioSignals.Instance.onPlaySound?.Invoke(AudioSoundEnums.ShieldBroke);
            PitchSignals.Instance.onPlayPitch?.Invoke(PitchEnums.ShieldBroke);
            PlayArmorVisualShatter();
        }

        private void PlayArmorVisualShatter()
        {
            if (armorVisual == null)
            {
                return;
            }

            _deflectScaleTween?.Kill();
            _deflectColorTween?.Kill();
            _armorBreakSequence?.Kill();

            armorVisual.localScale = _initialArmorScale;
            if (_armorSpriteRenderer != null)
            {
                _armorSpriteRenderer.color = _initialArmorColor;
            }

            _armorBreakSequence = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
            _armorBreakSequence.Join(armorVisual.DOScale(_initialArmorScale * 1.45f, 0.18f).SetEase(Ease.OutBack));
            if (_armorSpriteRenderer != null)
            {
                _armorBreakSequence.Join(_armorSpriteRenderer.DOFade(0f, 0.18f).SetEase(Ease.InQuad));
            }
            _armorBreakSequence.OnComplete(() =>
            {
                if (armorVisual != null)
                {
                    armorVisual.gameObject.SetActive(false);
                }
            });
        }
    }
}
