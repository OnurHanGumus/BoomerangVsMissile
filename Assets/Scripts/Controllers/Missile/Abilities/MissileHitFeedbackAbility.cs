using DG.Tweening;
using Enums;
using Interfaces;
using Managers;
using Signals;
using UnityEngine;

namespace Controllers.Missile.Abilities
{
    public class MissileHitFeedbackAbility : MonoBehaviour, IMissileDamageHandler
    {
        #region Self Variables

        #region Private Variables
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int LegacyColorId = Shader.PropertyToID("_Color");
        private static readonly Color HitFlashColor = new Color(1f, 0.35f, 0.1f, 1f);

        private MissileManager _manager;
        private Transform _punchTarget;
        private MeshRenderer _meshRenderer;
        private MaterialPropertyBlock _propertyBlock;
        private Vector3 _initialPunchScale = Vector3.one;
        private Color _initialBaseColor = Color.white;
        private bool _hasBaseColorProperty = false;
        private bool _hasLegacyColorProperty = false;
        private bool _hasCachedDefaults = false;

        private Tween _punchTween;
        private Tween _flashTween;
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

            if (_punchTarget == null)
            {
                Transform parentChild = transform.Find("Parent");
                _punchTarget = parentChild != null ? parentChild : transform;
            }

            if (_meshRenderer == null)
            {
                _meshRenderer = GetComponentInChildren<MeshRenderer>();
            }

            if (_propertyBlock == null)
            {
                _propertyBlock = new MaterialPropertyBlock();
            }

            if (!_hasCachedDefaults)
            {
                if (_punchTarget != null)
                {
                    _initialPunchScale = _punchTarget.localScale;
                }

                if (_meshRenderer != null && _meshRenderer.sharedMaterial != null)
                {
                    Material sharedMat = _meshRenderer.sharedMaterial;
                    _hasBaseColorProperty = sharedMat.HasProperty(BaseColorId);
                    _hasLegacyColorProperty = sharedMat.HasProperty(LegacyColorId);

                    if (_hasBaseColorProperty)
                    {
                        _initialBaseColor = sharedMat.GetColor(BaseColorId);
                    }
                    else if (_hasLegacyColorProperty)
                    {
                        _initialBaseColor = sharedMat.GetColor(LegacyColorId);
                    }
                }

                _hasCachedDefaults = true;
            }
        }

        private void OnEnable()
        {
            EnsureInitialized();
            ResetVisuals();
        }

        private void OnDisable()
        {
            ResetVisuals();
        }

        private void ResetVisuals()
        {
            _punchTween?.Kill();
            _flashTween?.Kill();

            if (_hasCachedDefaults && _punchTarget != null)
            {
                _punchTarget.localScale = _initialPunchScale;
            }

            if (_meshRenderer != null && _propertyBlock != null)
            {
                _propertyBlock.Clear();
                _meshRenderer.SetPropertyBlock(_propertyBlock);
            }
        }

        public void OnDamageTaken(int damage, int currentHealth)
        {
            EnsureInitialized();

            if (_manager != null && _manager.LastHitContext.SuppressDefaultHitFeedback)
            {
                return;
            }

            if (AudioSignals.Instance != null)
            {
                AudioSignals.Instance.onPlaySound?.Invoke(AudioSoundEnums.ShieldBroke);
            }

            if (PitchSignals.Instance != null)
            {
                PitchSignals.Instance.onPlayPitch?.Invoke(PitchEnums.Hit);
            }

            if (MissileSignals.Instance != null)
            {
                MissileSignals.Instance.onMissileArmorHit?.Invoke();
            }

            if (_punchTarget != null)
            {
                _punchTween?.Kill();
                _punchTarget.localScale = _initialPunchScale;
                _punchTween = _punchTarget
                    .DOPunchScale(_initialPunchScale * 0.25f, 0.2f, 10, 1f)
                    .SetUpdate(true)
                    .SetLink(gameObject);
            }

            if (_meshRenderer != null && (_hasBaseColorProperty || _hasLegacyColorProperty))
            {
                _flashTween?.Kill();
                int propId = _hasBaseColorProperty ? BaseColorId : LegacyColorId;

                _flashTween = DOVirtual.Float(1f, 0f, 0.2f, t =>
                {
                    if (_meshRenderer == null || _propertyBlock == null)
                    {
                        return;
                    }
                    Color c = Color.Lerp(_initialBaseColor, HitFlashColor, t);
                    _propertyBlock.SetColor(propId, c);
                    _meshRenderer.SetPropertyBlock(_propertyBlock);
                })
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    if (_meshRenderer != null && _propertyBlock != null)
                    {
                        _propertyBlock.Clear();
                        _meshRenderer.SetPropertyBlock(_propertyBlock);
                    }
                });
            }
        }
    }
}
