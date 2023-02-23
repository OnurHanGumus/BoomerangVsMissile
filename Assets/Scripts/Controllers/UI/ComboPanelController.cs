using Data.UnityObject;
using Data.ValueObject;
using DG.Tweening;
using Enums;
using Signals;
using System;
using System.Collections;
using System.Collections.Generic;
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


    #endregion
    #endregion
    private void Awake()
    {
        Init();
    }
    private void Init()
    {
        _commentsData = GetData();
        gameObject.transform.localScale = Vector3.zero;
    }
    private ComboCommentsData GetData() => Resources.Load<CD_Comments>("Data/CD_Comments").Data;

    public void OnCombo(int value)
    {
        StartCoroutine(Effect());
        if (value > 0)
        {
            commentTxt.text = _commentsData.CommentsList[value];
            ScoreSignals.Instance.onScoreIncrease?.Invoke(ScoreTypeEnums.Score, value);
        }
    }


    private IEnumerator Effect()
    {
        comboPanel.DOScale(1, 0.5f).SetEase(Ease.Flash);
        yield return new WaitForSeconds(1.5f);
        comboPanel.localScale = Vector3.zero;
    }

    public void OnRestartLevel()
    {
    }
}
