using Controllers;
using Enums;
using Signals;
using UnityEngine;
using Data.UnityObject;
using Data.ValueObject;

using Controllers.UI;

namespace Managers
{
    public class UIManager : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables

        [SerializeField] private UIPanelActivenessController uiPanelController;
        [SerializeField] private GameOverPanelController gameOverPanelController;
        [SerializeField] private LevelPanelController levelPanelController;
        [SerializeField] private HighScorePanelController highScorePanelController;
        [SerializeField] private ComboPanelController comboPanelController;
        [SerializeField] private ChargeSliderController chargeSliderController;
        [SerializeField] private ApprovalPanelController approvalPanelController;
        [SerializeField] private RewardBuffUIController rewardBuffUIController;

        #endregion
        #region Private Variables
        private UIData _data;
        private bool _isStorePanelOpened = false;
        private bool _isGameActive = false;

        #endregion
        #endregion

        #region Event Subscriptions
        private void Awake()
        {
            Init();
            SubscribeEvents();
        }

        private void Init()
        {
            _data = GetData();
            if (chargeSliderController == null)
            {
                chargeSliderController = FindFirstObjectByType<ChargeSliderController>();
                if (chargeSliderController == null)
                {
                    GameObject chargeObj = new GameObject("ChargeSliderController");
                    chargeSliderController = chargeObj.AddComponent<ChargeSliderController>();
                }
            }

            if (rewardBuffUIController == null)
            {
                rewardBuffUIController = FindFirstObjectByType<RewardBuffUIController>();
                if (rewardBuffUIController == null)
                {
                    Transform hudParent = levelPanelController != null ? levelPanelController.transform : transform;
                    GameObject rewardHudObj = new GameObject("RewardBuffHUD", typeof(RectTransform));
                    rewardHudObj.transform.SetParent(hudParent, false);
                    rewardBuffUIController = rewardHudObj.AddComponent<RewardBuffUIController>();
                }
            }
        }

        private UIData GetData() => Resources.Load<CD_UI>("Data/CD_UI").Data;

        private void SubscribeEvents()
        {
            UISignals.Instance.onOpenPanel += OnOpenPanel;
            UISignals.Instance.onClosePanel += OnClosePanel;
            UISignals.Instance.onSetChangedText += levelPanelController.OnScoreUpdateText;
            CoreGameSignals.Instance.onPlay += OnPlay;
            CoreGameSignals.Instance.onLevelFailed += OnLevelFailed;
            CoreGameSignals.Instance.onLevelSuccessful += OnLevelSuccessful;
            CoreGameSignals.Instance.onRestartLevel += levelPanelController.OnRestartLevel;
            ScoreSignals.Instance.onHighScoreChanged += highScorePanelController.OnUpdateText;
            BoomerangSignals.Instance.onCombo += comboPanelController.OnCombo;
        }

        private void UnsubscribeEvents()
        {
            if (UISignals.Instance != null)
            {
                UISignals.Instance.onOpenPanel -= OnOpenPanel;
                UISignals.Instance.onClosePanel -= OnClosePanel;
                if (levelPanelController != null)
                {
                    UISignals.Instance.onSetChangedText -= levelPanelController.OnScoreUpdateText;
                }
            }

            if (CoreGameSignals.Instance != null)
            {
                CoreGameSignals.Instance.onPlay -= OnPlay;
                CoreGameSignals.Instance.onLevelFailed -= OnLevelFailed;
                CoreGameSignals.Instance.onLevelSuccessful -= OnLevelSuccessful;
                if (levelPanelController != null)
                {
                    CoreGameSignals.Instance.onRestartLevel -= levelPanelController.OnRestartLevel;
                }
            }

            if (ScoreSignals.Instance != null && highScorePanelController != null)
            {
                ScoreSignals.Instance.onHighScoreChanged -= highScorePanelController.OnUpdateText;
            }

            if (BoomerangSignals.Instance != null && comboPanelController != null)
            {
                BoomerangSignals.Instance.onCombo -= comboPanelController.OnCombo;
            }
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        #endregion

        private void OnOpenPanel(UIPanels panelParam)
        {
            uiPanelController.OpenMenu(panelParam);
        }

        private void OnClosePanel(UIPanels panelParam)
        {
            uiPanelController.CloseMenu(panelParam);
        }

        private void OnPlay()
        {
            _isGameActive = true;
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.StartPanel);
            UISignals.Instance.onOpenPanel?.Invoke(UIPanels.LevelPanel);
        }

        private void OnLevelFailed()
        {
            _isGameActive = false;
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.ApprovalPanel);
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.LevelPanel);
            UISignals.Instance.onOpenPanel?.Invoke(UIPanels.FailPanel);
            //gameOverPanelController.ShowThePanel();
        }

        private void OnLevelSuccessful()
        {
            _isGameActive = false;
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.ApprovalPanel);
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.LevelPanel);
            UISignals.Instance.onOpenPanel?.Invoke(UIPanels.WinPanel);
        }

        public void Play()
        {
            CoreGameSignals.Instance.onPlay?.Invoke();
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.StorePanel);
            _isStorePanelOpened = false;
        }

        public void NextLevel()
        {
            _isGameActive = false;
            CoreGameSignals.Instance.onNextLevel?.Invoke();
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.WinPanel);
            UISignals.Instance.onOpenPanel?.Invoke(UIPanels.StartPanel);
        }

        public void RestartLevel()
        {
            if (_isGameActive)
            {
                RestartButton();
                return;
            }

            ExecuteRestartLevel();
        }

        public void RestartButton()
        {
            Time.timeScale = 0f;
            InputSignals.Instance.onDisableInput?.Invoke();
            UISignals.Instance.onOpenApprovalPanel?.Invoke(
                "Are you sure?",
                ConfirmRestartLevel,
                CancelRestartLevel);
        }

        public void ConfirmRestartLevel()
        {
            InputSignals.Instance.onEnableInput?.Invoke();
            ExecuteRestartLevel();
        }

        public void CancelRestartLevel()
        {
            Time.timeScale = 1f;
            InputSignals.Instance.onEnableInput?.Invoke();
        }

        private void ExecuteRestartLevel()
        {
            _isGameActive = false;
            CoreGameSignals.Instance.onRestartLevel?.Invoke();
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.ApprovalPanel);
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.FailPanel);
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.LevelPanel);
            UISignals.Instance.onOpenPanel?.Invoke(UIPanels.StartPanel);
        }

        public void PauseButton()
        {
            UISignals.Instance.onOpenPanel?.Invoke(UIPanels.PausePanel);
            Time.timeScale = 0f;
        }
        public void HighScoreButton()
        {
            UISignals.Instance.onOpenPanel?.Invoke(UIPanels.HighScorePanel);
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.StartPanel);
        }
        public void OptionsButton()
        {
            UISignals.Instance.onOpenPanel?.Invoke(UIPanels.OptionsPanel);
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.StartPanel);
            Time.timeScale = 0f;
            //Debug.Log("Clicked");
        }

        public void StoreButton()
        {
            UISignals.Instance.onOpenPanel?.Invoke(UIPanels.StorePanel);
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.StartPanel);
            _isStorePanelOpened = true;
        }

        public void CloseStoreButton()
        {
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.StorePanel);
            UISignals.Instance.onOpenPanel?.Invoke(UIPanels.StartPanel);
            _isStorePanelOpened = false;
        }
    }
}
