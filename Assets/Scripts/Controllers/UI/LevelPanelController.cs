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
    [SerializeField] private RectTransform coinIconTransform;
    [SerializeField] private GameObject restartButton;
    #endregion
    #region Private Variables
    private Vector3 _initialCoinIconScale;
    private Vector3 _initialScoreTextScale;
    private float _coinRollDuration;
    private int _displayedScore;
    private bool _hasInitializedScore;
    private Tween _coinRollTween;
    private Tween _coinIconTween;
    private Tween _scoreTextTween;
    private bool _hasCoinRollTween;
    private bool _hasCoinIconTween;
    private bool _hasScoreTextTween;
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
        _initialCoinIconScale = coinIconTransform.localScale;
        _initialScoreTextScale = scoreText.rectTransform.localScale;
        _coinRollDuration = Resources.Load<CD_UI>("Data/CD_UI").Data.CoinRollDuration;
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
        KillCoinTweens();
    }
    #endregion

    public void OnScoreUpdateText(ScoreTypeEnums type, int score)
    {
        if (type.Equals(ScoreTypeEnums.Money))
        {
            if (!_hasInitializedScore)
            {
                _displayedScore = score;
                _hasInitializedScore = true;
                scoreText.text = score.ToString();
                return;
            }

            if (score == _displayedScore)
            {
                return;
            }

            KillCoinTweens();

            _coinRollTween = DOTween.To(
                    () => _displayedScore,
                    value =>
                    {
                        _displayedScore = value;
                        scoreText.text = value.ToString();
                    },
                    score,
                    _coinRollDuration)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);
            _hasCoinRollTween = true;

            coinIconTransform.localScale = _initialCoinIconScale;
            _coinIconTween = coinIconTransform
                .DOPunchScale(_initialCoinIconScale * 0.22f, 0.28f, 6, 0.8f)
                .SetUpdate(true);
            _hasCoinIconTween = true;

            scoreText.rectTransform.localScale = _initialScoreTextScale;
            _scoreTextTween = scoreText.rectTransform
                .DOPunchScale(_initialScoreTextScale * 0.10f, 0.22f, 5, 0.8f)
                .SetUpdate(true);
            _hasScoreTextTween = true;
        }
    }

    private void KillCoinTweens()
    {
        if (_hasCoinRollTween)
        {
            _coinRollTween.Kill();
            _hasCoinRollTween = false;
        }

        if (_hasCoinIconTween)
        {
            _coinIconTween.Kill();
            _hasCoinIconTween = false;
            coinIconTransform.localScale = _initialCoinIconScale;
        }

        if (_hasScoreTextTween)
        {
            _scoreTextTween.Kill();
            _hasScoreTextTween = false;
            scoreText.rectTransform.localScale = _initialScoreTextScale;
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
