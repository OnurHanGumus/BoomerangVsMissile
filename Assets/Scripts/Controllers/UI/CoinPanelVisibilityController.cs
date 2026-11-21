using System.Collections.Generic;
using DG.Tweening;
using Enums;
using Signals;
using UnityEngine;

namespace Controllers.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CoinPanelVisibilityController : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables

        [SerializeField] private List<UIPanels> hideOnPanels = new List<UIPanels>
        {
            UIPanels.WeeklyTaskPanel,
            UIPanels.FortuneWheelPanel
        };
        [SerializeField] private float fadeDuration = 0.18f;

        #endregion

        #region Private Variables

        private CanvasGroup _canvasGroup;
        private readonly HashSet<UIPanels> _activeHiddenPanels = new HashSet<UIPanels>();
        private Tween _fadeTween;
        private bool _hasFadeTween;

        #endregion

        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }

        private void Init()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        #region Event Subscription

        private void SubscribeEvents()
        {
            UISignals.Instance.onOpenPanel += OnOpenPanel;
            UISignals.Instance.onClosePanel += OnClosePanel;
        }

        private void UnsubscribeEvents()
        {
            UISignals.Instance.onOpenPanel -= OnOpenPanel;
            UISignals.Instance.onClosePanel -= OnClosePanel;
        }

        private void OnDestroy()
        {
            KillFadeTween();
            UnsubscribeEvents();
        }

        #endregion

        private void OnOpenPanel(UIPanels panel)
        {
            if (hideOnPanels.Contains(panel))
            {
                _activeHiddenPanels.Add(panel);
                FadeTo(0f);
            }
            else if (panel == UIPanels.StartPanel || panel == UIPanels.LevelPanel || panel == UIPanels.StorePanel || panel == UIPanels.WinPanel)
            {
                _activeHiddenPanels.Clear();
                FadeTo(1f);
            }
        }

        private void OnClosePanel(UIPanels panel)
        {
            if (_activeHiddenPanels.Remove(panel) && _activeHiddenPanels.Count == 0)
            {
                FadeTo(1f);
            }
        }

        private void FadeTo(float targetAlpha)
        {
            KillFadeTween();
            _fadeTween = _canvasGroup
                .DOFade(targetAlpha, fadeDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
            _hasFadeTween = true;
        }

        private void KillFadeTween()
        {
            if (!_hasFadeTween)
            {
                return;
            }

            _fadeTween.Kill();
            _hasFadeTween = false;
        }
    }
}
