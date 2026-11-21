using System;
using System.Collections.Generic;
using Data.UnityObject;
using Data.ValueObject;
using DG.Tweening;
using UnityEngine;

namespace Controllers.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIPanelAnimator : MonoBehaviour
    {
        [Serializable]
        public struct PopTargetConfig
        {
            public RectTransform target;
            public float startScaleMultiplier;
            public float closeScaleMultiplier;
            public Vector2 enterOffset;
            public float openDelay;
            public float openDuration;
            public Ease openEase;
            public Ease closeEase;
        }

        [Serializable]
        public struct SlideTargetConfig
        {
            public RectTransform target;
            public Vector2 enterOffset;
            public Vector2 exitOffset;
            public float openDelay;
            public float openDuration;
            public Ease openEase;
            public Ease closeEase;
            public bool shakeOnOpen;
        }

        #region Self Variables

        #region Serialized Variables

        [Header("Timing")]
        [SerializeField] private bool useGlobalDefaults = true;
        [SerializeField] private float openFadeDuration = 0.25f;
        [SerializeField] private float closeFadeDuration = 0.18f;
        [SerializeField] private bool playOnStart = false;

        [Header("Animated Child Targets")]
        [SerializeField] private List<PopTargetConfig> popTargets = new List<PopTargetConfig>();
        [SerializeField] private List<SlideTargetConfig> slideTargets = new List<SlideTargetConfig>();

        #endregion

        #region Private Variables

        private CanvasGroup _canvasGroup;
        private UIData _uiData;
        private Vector3[] _popInitialScales;
        private Vector2[] _popInitialPositions;
        private Vector2[] _slideInitialPositions;
        private Sequence _activeSequence;
        private bool _hasActiveSequence;

        #endregion

        #endregion

        private void Awake()
        {
            Init();
        }

        private void Start()
        {
            if (playOnStart)
            {
                AnimateOpen();
            }
        }

        private void Init()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _uiData = Resources.Load<CD_UI>("Data/CD_UI").Data;

            if (useGlobalDefaults)
            {
                openFadeDuration = _uiData.PanelOpenDuration;
                closeFadeDuration = _uiData.PanelCloseDuration;
            }

            _popInitialScales = new Vector3[popTargets.Count];
            _popInitialPositions = new Vector2[popTargets.Count];
            for (int i = 0; i < popTargets.Count; i++)
            {
                _popInitialScales[i] = popTargets[i].target.localScale;
                _popInitialPositions[i] = popTargets[i].target.anchoredPosition;
            }

            _slideInitialPositions = new Vector2[slideTargets.Count];
            for (int i = 0; i < slideTargets.Count; i++)
            {
                _slideInitialPositions[i] = slideTargets[i].target.anchoredPosition;
            }
        }

        public void AnimateOpen()
        {
            KillActiveSequence();

            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
            _canvasGroup.alpha = 0f;

            _activeSequence = DOTween.Sequence().SetUpdate(true);
            _hasActiveSequence = true;

            _activeSequence.Insert(0f, _canvasGroup.DOFade(1f, openFadeDuration).SetEase(Ease.OutQuad));

            for (int i = 0; i < popTargets.Count; i++)
            {
                PopTargetConfig cfg = popTargets[i];
                float startScale = cfg.startScaleMultiplier > 0f ? cfg.startScaleMultiplier : _uiData.PopStartScaleMultiplier;
                float duration = cfg.openDuration > 0f ? cfg.openDuration : openFadeDuration;

                cfg.target.localScale = _popInitialScales[i] * startScale;
                _activeSequence.Insert(cfg.openDelay, cfg.target.DOScale(_popInitialScales[i], duration).SetEase(cfg.openEase));

                if (cfg.enterOffset != Vector2.zero)
                {
                    cfg.target.anchoredPosition = _popInitialPositions[i] + cfg.enterOffset;
                    _activeSequence.Insert(cfg.openDelay, cfg.target.DOAnchorPos(_popInitialPositions[i], duration).SetEase(Ease.OutCubic));
                }
            }

            for (int i = 0; i < slideTargets.Count; i++)
            {
                SlideTargetConfig cfg = slideTargets[i];
                float duration = cfg.openDuration > 0f ? cfg.openDuration : openFadeDuration;
                Vector2 restPos = _slideInitialPositions[i];

                cfg.target.anchoredPosition = restPos + cfg.enterOffset;
                _activeSequence.Insert(cfg.openDelay, cfg.target.DOAnchorPos(restPos, duration).SetEase(cfg.openEase));

                if (cfg.shakeOnOpen)
                {
                    float shakeStart = cfg.openDelay + duration;
                    _activeSequence.Insert(
                        shakeStart,
                        cfg.target.DOShakeAnchorPos(0.22f, new Vector2(14f, 0f), 18, 0f, false, true));
                    _activeSequence.InsertCallback(shakeStart + 0.23f, () => cfg.target.anchoredPosition = restPos);
                }
            }
        }

        public void AnimateClose()
        {
            KillActiveSequence();

            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;

            _activeSequence = DOTween.Sequence().SetUpdate(true);
            _hasActiveSequence = true;

            _activeSequence.Insert(0f, _canvasGroup.DOFade(0f, closeFadeDuration).SetEase(Ease.OutQuad));

            for (int i = 0; i < popTargets.Count; i++)
            {
                PopTargetConfig cfg = popTargets[i];
                float closeScale = cfg.closeScaleMultiplier > 0f ? cfg.closeScaleMultiplier : _uiData.PopCloseScaleMultiplier;

                _activeSequence.Insert(0f, cfg.target.DOScale(_popInitialScales[i] * closeScale, closeFadeDuration).SetEase(cfg.closeEase));

                if (cfg.enterOffset != Vector2.zero)
                {
                    _activeSequence.Insert(
                        0f,
                        cfg.target.DOAnchorPos(_popInitialPositions[i] + cfg.enterOffset * 0.5f, closeFadeDuration).SetEase(Ease.InQuad));
                }
            }

            for (int i = 0; i < slideTargets.Count; i++)
            {
                SlideTargetConfig cfg = slideTargets[i];
                _activeSequence.Insert(
                    0f,
                    cfg.target.DOAnchorPos(_slideInitialPositions[i] + cfg.exitOffset, closeFadeDuration).SetEase(cfg.closeEase));
            }

            _activeSequence.OnComplete(RestoreRestTransforms);
        }

        private void RestoreRestTransforms()
        {
            for (int i = 0; i < popTargets.Count; i++)
            {
                popTargets[i].target.localScale = _popInitialScales[i];
                popTargets[i].target.anchoredPosition = _popInitialPositions[i];
            }

            for (int i = 0; i < slideTargets.Count; i++)
            {
                slideTargets[i].target.anchoredPosition = _slideInitialPositions[i];
            }
        }

        private void KillActiveSequence()
        {
            if (!_hasActiveSequence)
            {
                return;
            }

            _activeSequence.Kill();
            _hasActiveSequence = false;
        }

        private void OnDestroy()
        {
            KillActiveSequence();
        }
    }
}
