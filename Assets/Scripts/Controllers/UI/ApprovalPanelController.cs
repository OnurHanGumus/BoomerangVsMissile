using DG.Tweening;
using Enums;
using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Controllers.UI
{
    public class ApprovalPanelController : MonoBehaviour
    {
        #region Self Variables

        #region Public Variables
        public CanvasGroup PanelCanvasGroup => canvasGroup;
        #endregion

        #region Serialized Variables
        [Header("UI References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform dialogContainer;
        [SerializeField] private Image dialogBackgroundImage;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button yesButton;
        [SerializeField] private TextMeshProUGUI yesButtonText;
        [SerializeField] private Button noButton;
        [SerializeField] private TextMeshProUGUI noButtonText;

        [Header("Default Texts")]
        [SerializeField] private string defaultMessage = "Are you sure?";
        [SerializeField] private string defaultYesText = "Yes";
        [SerializeField] private string defaultNoText = "No";
        #endregion

        #region Private Variables
        private UnityAction _onConfirmCallback;
        private UnityAction _onCancelCallback;
        private bool _isOpen;
        #endregion

        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }

        private void Init()
        {
            BindButtons();
        }

        private void BindButtons()
        {
            if (yesButton != null)
            {
                yesButton.onClick.RemoveListener(OnYesClicked);
                yesButton.onClick.AddListener(OnYesClicked);
            }

            if (noButton != null)
            {
                noButton.onClick.RemoveListener(OnNoClicked);
                noButton.onClick.AddListener(OnNoClicked);
            }
        }

        #region Event Subscription
        private void SubscribeEvents()
        {
            if (UISignals.Instance != null)
            {
                UISignals.Instance.onOpenApprovalPanel += OnOpenApprovalPanel;
            }

            if (CoreGameSignals.Instance != null)
            {
                CoreGameSignals.Instance.onLevelFailed += OnForceClose;
                CoreGameSignals.Instance.onLevelSuccessful += OnForceClose;
            }
        }

        private void UnsubscribeEvents()
        {
            if (UISignals.Instance != null)
            {
                UISignals.Instance.onOpenApprovalPanel -= OnOpenApprovalPanel;
            }

            if (CoreGameSignals.Instance != null)
            {
                CoreGameSignals.Instance.onLevelFailed -= OnForceClose;
                CoreGameSignals.Instance.onLevelSuccessful -= OnForceClose;
            }
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }
        #endregion

        private void OnOpenApprovalPanel(string message, UnityAction onConfirm, UnityAction onCancel)
        {
            OpenApprovalPanel(message, onConfirm, onCancel, defaultYesText, defaultNoText);
        }

        public void OpenApprovalPanel(
            string message,
            UnityAction onConfirm,
            UnityAction onCancel = null,
            string yesLabel = null,
            string noLabel = null)
        {
            _onConfirmCallback = onConfirm;
            _onCancelCallback = onCancel;
            _isOpen = true;

            if (messageText != null)
            {
                messageText.text = string.IsNullOrEmpty(message) ? defaultMessage : message;
            }

            if (yesButtonText != null)
            {
                yesButtonText.text = string.IsNullOrEmpty(yesLabel) ? defaultYesText : yesLabel;
            }

            if (noButtonText != null)
            {
                noButtonText.text = string.IsNullOrEmpty(noLabel) ? defaultNoText : noLabel;
            }

            if (dialogContainer != null)
            {
                dialogContainer.DOKill();
                dialogContainer.localScale = Vector3.one * 0.85f;
                dialogContainer.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);
            }

            UISignals.Instance.onOpenPanel?.Invoke(UIPanels.ApprovalPanel);
        }

        public void OnYesClicked()
        {
            if (!_isOpen)
            {
                return;
            }

            _isOpen = false;
            UnityAction confirmAction = _onConfirmCallback;
            _onConfirmCallback = null;
            _onCancelCallback = null;

            UISignals.Instance.onClosePanel?.Invoke(UIPanels.ApprovalPanel);
            confirmAction?.Invoke();
        }

        public void OnNoClicked()
        {
            if (!_isOpen)
            {
                return;
            }

            _isOpen = false;
            UnityAction cancelAction = _onCancelCallback;
            _onConfirmCallback = null;
            _onCancelCallback = null;

            UISignals.Instance.onClosePanel?.Invoke(UIPanels.ApprovalPanel);
            cancelAction?.Invoke();
        }

        private void OnForceClose()
        {
            if (!_isOpen)
            {
                return;
            }

            _isOpen = false;
            _onConfirmCallback = null;
            _onCancelCallback = null;
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.ApprovalPanel);
        }
    }
}
