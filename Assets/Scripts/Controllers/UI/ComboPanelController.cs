using Data.UnityObject;
using Data.ValueObject;
using DG.Tweening;
using Enums;
using Signals;
using System;
using TMPro;
using UnityEngine;

public class ComboPanelController : MonoBehaviour
{
    #region Self Variables

    #region Public Variables
    public bool IsVisible => _isShowing || (comboPanel != null && comboPanel.localScale.sqrMagnitude > 0.001f);
    #endregion

    #region SerializeField Variables
    [SerializeField] private TextMeshProUGUI commentTxt;
    [SerializeField] private Transform comboPanel;
    [SerializeField] private CD_ComboPanel cdComboPanel;
    #endregion

    #region Private Variables
    private ComboCommentsData _commentsData;
    private GainMoneyData _gainMoneyData;
    private ComboPanelData _comboPanelData;
    private Sequence _comboSequence;
    private bool _isShowing;
    #endregion

    #endregion

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (comboPanel == null)
        {
            comboPanel = transform;
        }

        _isShowing = false;
        _commentsData = GetCommentData();
        _gainMoneyData = GetMoneyData();
        _comboPanelData = GetComboPanelData();
        comboPanel.localScale = Vector3.zero;
    }

    private ComboCommentsData GetCommentData() => Resources.Load<CD_Comments>("Data/CD_Comments")?.Data;
    private GainMoneyData GetMoneyData() => Resources.Load<CD_Money>("Data/CD_Money")?.Data;

    private ComboPanelData GetComboPanelData()
    {
        if (cdComboPanel != null && cdComboPanel.Data != null)
        {
            return cdComboPanel.Data;
        }
        return Resources.Load<CD_ComboPanel>("Data/CD_ComboPanel")?.Data ?? new ComboPanelData();
    }

    public void OnCombo(int value)
    {
        int earnedMoney = GetEarnedMoney(value);
        string comment = GetComment(value);

        UpdateDisplayText(comment, earnedMoney, value);
        PlayComboAnimation();
        PlayFeedback(value);

        ScoreSignals.Instance.onScoreIncrease?.Invoke(ScoreTypeEnums.Money, earnedMoney);
        TutorialSignals.Instance.onTutorialSatisfied?.Invoke();
    }

    private int GetEarnedMoney(int value)
    {
        if (_gainMoneyData != null && _gainMoneyData.GainMoneyList != null && _gainMoneyData.GainMoneyList.Count > 0)
        {
            int index = Mathf.Clamp(value, 0, _gainMoneyData.GainMoneyList.Count - 1);
            return _gainMoneyData.GainMoneyList[index];
        }
        int multiplier = _comboPanelData != null ? _comboPanelData.DefaultMoneyMultiplier : 50;
        return multiplier * (value + 1);
    }

    private string GetComment(int value)
    {
        if (_commentsData != null && _commentsData.CommentsList != null && _commentsData.CommentsList.Count > 0)
        {
            int index = Mathf.Clamp(value, 0, _commentsData.CommentsList.Count - 1);
            return _commentsData.CommentsList[index];
        }
        return _comboPanelData != null ? _comboPanelData.DefaultComment : "COMBO!";
    }

    private void UpdateDisplayText(string comment, int earnedMoney, int value)
    {
        if (commentTxt == null) return;

        commentTxt.color = Color.white;
        commentTxt.enableVertexGradient = true;
        //commentTxt.colorGradient = GetTierGradient(value);

        string hexColor = _comboPanelData != null ? _comboPanelData.MoneyColorHex : "#FFE600";
        int sizePercent = _comboPanelData != null ? _comboPanelData.MoneyTextSizePercent : 70;

        commentTxt.text = $"<b>{comment.ToUpper()}</b>\n<size={sizePercent}%><color={hexColor}>+{earnedMoney} Money</color></size>";
        commentTxt.ForceMeshUpdate();
    }

    private VertexGradient GetTierGradient(int value)
    {
        if (_comboPanelData != null && _comboPanelData.TierGradients != null && _comboPanelData.TierGradients.Count > 0)
        {
            int index = Mathf.Clamp(value, 0, _comboPanelData.TierGradients.Count - 1);
            return _comboPanelData.TierGradients[index].ToVertexGradient();
        }

        return new VertexGradient(Color.white, Color.white, Color.white, Color.white);
    }

    private void PlayComboAnimation()
    {
        float targetScale = _comboPanelData != null ? _comboPanelData.TargetScale : 1f;
        float duration = _comboPanelData != null ? _comboPanelData.ScaleDuration : 0.5f;
        Ease ease = _comboPanelData != null ? _comboPanelData.ScaleEase : Ease.Flash;
        float displayInterval = _comboPanelData != null ? _comboPanelData.DisplayDuration : 1f;

        if (_isShowing)
        {
            _comboSequence?.Kill();
            comboPanel.DOKill();
            comboPanel.localScale = Vector3.one * targetScale;

            _comboSequence = DOTween.Sequence();
            _comboSequence.Append(comboPanel.DOScale(targetScale, duration).SetEase(ease)).AppendInterval(displayInterval)
                .OnComplete(() =>
                {
                    _isShowing = false;
                    comboPanel.localScale = Vector3.zero;
                });

            return;
        }

        _isShowing = true;
        _comboSequence?.Kill();
        comboPanel.DOKill();

        comboPanel.localScale = Vector3.zero;

        _comboSequence = DOTween.Sequence();
        _comboSequence.Append(comboPanel.DOScale(targetScale, duration).SetEase(ease)).AppendInterval(displayInterval)
        .OnComplete(() =>
        {
            _isShowing = false;
            comboPanel.localScale = Vector3.zero;
        });
    }

    private void PlayFeedback(int value)
    {
        float basePitch = _comboPanelData != null ? _comboPanelData.BasePitch : 1.0f;
        float pitchStep = _comboPanelData != null ? _comboPanelData.PitchStep : 0.05f;
        float maxPitchOffset = _comboPanelData != null ? _comboPanelData.MaxPitchOffset : 0.25f;

        // Rising pitch for combo sound
        float pitch = basePitch + Mathf.Min(value * pitchStep, maxPitchOffset);
        AudioSignals.Instance.onPlaySoundWithPitch?.Invoke(AudioSoundEnums.Combo, pitch);

        // Escalating haptic feedback
        PitchSignals.Instance.onPlayPitch?.Invoke(PitchEnums.Combo);
    }

    public void CloseComboPanel()
    {
        _isShowing = false;
        _comboSequence?.Kill();
        if (comboPanel != null)
        {
            comboPanel.DOKill();
            comboPanel.localScale = Vector3.zero;
        }
    }

    public void OnRestartLevel()
    {
        CloseComboPanel();
    }


    private void OnDestroy()
    {
        _comboSequence?.Kill();
        if (comboPanel != null)
        {
            comboPanel.DOKill();
        }
    }
}
