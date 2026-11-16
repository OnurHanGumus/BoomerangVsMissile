using Enums;
using Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Data.UnityObject;
using DG.Tweening;

public class LevelPanelController : MonoBehaviour
{
    #region Self Variables
    #region Public Variables
    #endregion
    #region SerializeField Variables
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject restartButton;
    #endregion
    #region Private Variables


    #endregion
    #endregion
    private void Awake()
    {
        Init();
        SubscribeEvents();
    }

    private void Start()
    {
        UpdateRestartButtonVisibility();
    }

    private void OnEnable()
    {
        UpdateRestartButtonVisibility();
    }

    private void Init()
    {


    }

    #region Event Subscription
    private void SubscribeEvents()
    {
        if (UISignals.Instance != null)
        {
            UISignals.Instance.onOpenPanel += OnOpenPanel;
        }

        if (CoreGameSignals.Instance != null)
        {
            CoreGameSignals.Instance.onPlay += OnPlay;
            CoreGameSignals.Instance.onLevelInitialize += OnLevelInitialize;
            CoreGameSignals.Instance.onNextLevel += OnNextLevel;
        }
    }

    private void UnsubscribeEvents()
    {
        if (UISignals.Instance != null)
        {
            UISignals.Instance.onOpenPanel -= OnOpenPanel;
        }

        if (CoreGameSignals.Instance != null)
        {
            CoreGameSignals.Instance.onPlay -= OnPlay;
            CoreGameSignals.Instance.onLevelInitialize -= OnLevelInitialize;
            CoreGameSignals.Instance.onNextLevel -= OnNextLevel;
        }
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }
    #endregion

    public void OnScoreUpdateText(ScoreTypeEnums type, int score)
    {
        if (type.Equals(ScoreTypeEnums.Money))
        {
            scoreText.text = score.ToString();
        }
    }

    public void OnRestartLevel()
    {
        UpdateRestartButtonVisibility();
    }

    private void OnOpenPanel(UIPanels panel)
    {
        if (panel == UIPanels.LevelPanel)
        {
            UpdateRestartButtonVisibility();
        }
    }

    private void OnPlay()
    {
        UpdateRestartButtonVisibility();
    }

    private void OnLevelInitialize()
    {
        UpdateRestartButtonVisibility();
    }

    private void OnNextLevel()
    {
        UpdateRestartButtonVisibility();
    }

    public void UpdateRestartButtonVisibility()
    {
        int currentLevelId = GetCurrentLevelId();
        restartButton.SetActive(currentLevelId != 0);
    }

    private int GetCurrentLevelId()
    {
        if (LevelSignals.Instance != null && LevelSignals.Instance.onGetLevelId != null)
        {
            return LevelSignals.Instance.onGetLevelId();
        }

        return 0;
    }
}
