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
    #endregion

    #region SerializeField Variables
    [SerializeField] private TextMeshProUGUI commentTxt;
    [SerializeField] private Transform comboPanel;
    #endregion

    #region Private Variables
    private ComboCommentsData _commentsData;
    private GainMoneyData _gainMoneyData;
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
        comboPanel.localScale = Vector3.zero;
    }

    private ComboCommentsData GetCommentData() => Resources.Load<CD_Comments>("Data/CD_Comments")?.Data;
    private GainMoneyData GetMoneyData() => Resources.Load<CD_Money>("Data/CD_Money")?.Data;

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
        return 50 * (value + 1);
    }

    private string GetComment(int value)
    {
        if (_commentsData != null && _commentsData.CommentsList != null && _commentsData.CommentsList.Count > 0)
        {
            int index = Mathf.Clamp(value, 0, _commentsData.CommentsList.Count - 1);
            return _commentsData.CommentsList[index];
        }
        return "COMBO!";
    }

    private void UpdateDisplayText(string comment, int earnedMoney, int value)
    {
        if (commentTxt == null) return;

        commentTxt.color = Color.white;
        commentTxt.enableVertexGradient = true;
        //commentTxt.colorGradient = GetTierGradient(value);

        commentTxt.text = $"<b>{comment.ToUpper()}</b>\n<size=70%><color=#FFE600>+{earnedMoney} Money</color></size>";
        commentTxt.ForceMeshUpdate();
    }

    private VertexGradient GetTierGradient(int value)
    {
        switch (value)
        {
            case 0: // 2 Hits - Electric Cyan & Mint
                return new VertexGradient(
                    new Color(0.0f, 0.95f, 1.0f),   // Top-Left (Electric Cyan)
                    new Color(0.0f, 1.0f, 0.7f),    // Top-Right (Neon Aquamarine)
                    new Color(1.0f, 1.0f, 1.0f),    // Bottom-Left (Pure White)
                    new Color(0.6f, 0.95f, 1.0f)    // Bottom-Right (Ice Blue)
                );

            case 1: // 3 Hits - Vibrant Gold & Warm Amber
                return new VertexGradient(
                    new Color(1.0f, 0.9f, 0.0f),    // Top-Left (Bright Gold)
                    new Color(1.0f, 0.65f, 0.0f),   // Top-Right (Warm Amber)
                    new Color(1.0f, 1.0f, 0.9f),    // Bottom-Left (Soft White-Gold)
                    new Color(1.0f, 0.8f, 0.2f)     // Bottom-Right (Deep Gold)
                );

            case 2: // 4 Hits - Fiery Flame Orange & Hot Coral
                return new VertexGradient(
                    new Color(1.0f, 0.45f, 0.0f),   // Top-Left (Flame Orange)
                    new Color(1.0f, 0.15f, 0.35f),  // Top-Right (Hot Coral)
                    new Color(1.0f, 0.9f, 0.8f),    // Bottom-Left (Light Peach)
                    new Color(1.0f, 0.3f, 0.1f)     // Bottom-Right (Fiery Red)
                );

            case 3: // 5 Hits - Cosmic Neon Violet & Electric Magenta
                return new VertexGradient(
                    new Color(0.85f, 0.2f, 1.0f),   // Top-Left (Neon Violet)
                    new Color(1.0f, 0.1f, 0.65f),   // Top-Right (Electric Magenta)
                    new Color(1.0f, 0.85f, 1.0f),   // Bottom-Left (Lavender)
                    new Color(0.6f, 0.15f, 0.95f)   // Bottom-Right (Deep Purple)
                );

            default: // 6+ Hits - Radiant Hyper Crimson & Sunfire Gold
                return new VertexGradient(
                    new Color(1.0f, 0.05f, 0.3f),   // Top-Left (Hyper Crimson)
                    new Color(1.0f, 0.85f, 0.1f),   // Top-Right (Sunfire Gold)
                    new Color(1.0f, 1.0f, 1.0f),    // Bottom-Left (Pure White)
                    new Color(1.0f, 0.1f, 0.8f)     // Bottom-Right (Cosmic Pink)
                );
        }
    }

    private void PlayComboAnimation()
    {
        if (_isShowing)
        {
            _comboSequence?.Kill();
            comboPanel.DOKill();
            comboPanel.localScale = Vector3.one;

            _comboSequence = DOTween.Sequence();
            _comboSequence.Append(comboPanel.DOScale(1, 0.5f).SetEase(Ease.Flash)).AppendInterval(1f)
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
        _comboSequence.Append(comboPanel.DOScale(1, 0.5f).SetEase(Ease.Flash)).AppendInterval(1f)
        .OnComplete(() =>
        {
            _isShowing = false;
            comboPanel.localScale = Vector3.zero;
        });
    }

    private void PlayFeedback(int value)
    {
        // Rising pitch for combo sound
        float pitch = 1.0f + Mathf.Min(value * 0.05f, 0.25f);
        AudioSignals.Instance.onPlaySoundWithPitch?.Invoke(AudioSoundEnums.Combo, pitch);

        // Escalating haptic feedback
        PitchSignals.Instance.onPlayPitch?.Invoke(PitchEnums.Combo);
    }

    public void OnRestartLevel()
    {
        _isShowing = false;
        _comboSequence?.Kill();
        if (comboPanel != null)
        {
            comboPanel.DOKill();
            comboPanel.localScale = Vector3.zero;
        }
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
