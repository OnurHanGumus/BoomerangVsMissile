using Controllers;
using Enums;
using Signals;
using UnityEngine;
using Data.UnityObject;
using Data.ValueObject;

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
        [SerializeField] private UIBuildBoomerangController uiBuildBoomerangController;
        [SerializeField] private ComboPanelController comboPanelController;

        #endregion
        #region Private Variables
        private UIData _data;
        private bool _isStorePanelOpened = false;

        #endregion
        #endregion

        #region Event Subscriptions
        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _data = GetData();
        }

        private UIData GetData() => Resources.Load<CD_UI>("Data/CD_UI").Data;
        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            UISignals.Instance.onOpenPanel += OnOpenPanel;
            UISignals.Instance.onClosePanel += OnClosePanel;
            UISignals.Instance.onSetChangedText += levelPanelController.OnScoreUpdateText;
            CoreGameSignals.Instance.onPlay += OnPlay;
            CoreGameSignals.Instance.onLevelFailed += OnLevelFailed;
            CoreGameSignals.Instance.onLevelSuccessful += OnLevelSuccessful;
            CoreGameSignals.Instance.onRestartLevel += levelPanelController.OnRestartLevel;
            CoreGameSignals.Instance.onRestartLevel += uiBuildBoomerangController.OnRestartLevel;
            ScoreSignals.Instance.onHighScoreChanged += highScorePanelController.OnUpdateText;
            PlayerSignals.Instance.onAnimationSpeedIncreased += uiBuildBoomerangController.OnAnimationSpeedIncreased;
            BoomerangSignals.Instance.onBoomerangDisapeared += uiBuildBoomerangController.OnBoomerangDisapeared;
            BoomerangSignals.Instance.onCombo += comboPanelController.OnCombo;
        }

        private void UnsubscribeEvents()
        {
            UISignals.Instance.onOpenPanel -= OnOpenPanel;
            UISignals.Instance.onClosePanel -= OnClosePanel;
            UISignals.Instance.onSetChangedText -= levelPanelController.OnScoreUpdateText;
            CoreGameSignals.Instance.onPlay -= OnPlay;
            CoreGameSignals.Instance.onLevelFailed -= OnLevelFailed;
            CoreGameSignals.Instance.onLevelSuccessful -= OnLevelSuccessful;
            CoreGameSignals.Instance.onRestartLevel -= levelPanelController.OnRestartLevel;
            CoreGameSignals.Instance.onRestartLevel -= uiBuildBoomerangController.OnRestartLevel;
            ScoreSignals.Instance.onHighScoreChanged -= highScorePanelController.OnUpdateText;
            PlayerSignals.Instance.onAnimationSpeedIncreased -= uiBuildBoomerangController.OnAnimationSpeedIncreased;
            BoomerangSignals.Instance.onBoomerangDisapeared -= uiBuildBoomerangController.OnBoomerangDisapeared;
            BoomerangSignals.Instance.onCombo -= comboPanelController.OnCombo;
        }

        private void OnDisable()
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
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.StartPanel);
            UISignals.Instance.onOpenPanel?.Invoke(UIPanels.LevelPanel);
        }

        private void OnLevelFailed()
        {
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.LevelPanel);
            UISignals.Instance.onOpenPanel?.Invoke(UIPanels.FailPanel);
            //gameOverPanelController.ShowThePanel();
        }

        private void OnLevelSuccessful()
        {
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.LevelPanel);
            UISignals.Instance.onOpenPanel?.Invoke(UIPanels.WinPanel);
        }

        public void Play()
        {
            CoreGameSignals.Instance.onPlay?.Invoke();
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.StorePanel);
        }

        public void NextLevel()
        {
            CoreGameSignals.Instance.onNextLevel?.Invoke();
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.WinPanel);
            UISignals.Instance.onOpenPanel?.Invoke(UIPanels.StartPanel);
        }

        public void RestartLevel()
        {
            CoreGameSignals.Instance.onRestartLevel?.Invoke();
            UISignals.Instance.onClosePanel?.Invoke(UIPanels.FailPanel);
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
            Time.timeScale = 0f;
            Debug.Log("Clicked");
        }

        public void StoreButton()
        {
            if (!_isStorePanelOpened)
            {
                UISignals.Instance.onOpenPanel?.Invoke(UIPanels.StorePanel);
            }
            else
            {
                UISignals.Instance.onClosePanel?.Invoke(UIPanels.StorePanel);
            }
            _isStorePanelOpened = !_isStorePanelOpened;

        }
    }
}