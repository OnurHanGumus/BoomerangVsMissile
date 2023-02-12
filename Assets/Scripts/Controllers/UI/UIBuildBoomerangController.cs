using Enums;
using Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Data.UnityObject;
using DG.Tweening;
using UnityEngine.UI;

public class UIBuildBoomerangController : MonoBehaviour
{
    #region Self Variables
    #region Public Variables
    #endregion
    #region SerializeField Variables
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Transform pointer;

    [SerializeField] private Image barImg;
    #endregion

    #region Private Variables
    private int _counter = 0;
    private int _positionIncreaseValue = 10;
    private float _textIncreaseValue = 0.5f;

    #endregion
    #endregion
    private void Awake()
    {
        Init();
    }
    private void Init()
    {

    }
    public void OnAnimationSpeedIncreased()
    {
        ++_counter;
        pointer.transform.localPosition = new Vector3(pointer.transform.localPosition.x + _positionIncreaseValue, pointer.localPosition.y, 0);
        scoreText.text = ((double)(_counter * _textIncreaseValue)).ToString() + "x";
        barImg.color = Color.HSVToRGB((float)((_counter * 2)/250f),1, 1);
        scoreText.color = Color.HSVToRGB((float)((_counter * 2) / 250f), 1, 1);

    }
    public void OnBoomerangRebulded()
    {
        ResetValues();
    }

    public void OnRestartLevel()
    {
        ResetValues();
    }
    private void ResetValues()
    {
        _counter = 0;
        scoreText.text = (_counter * _textIncreaseValue).ToString() + "x";
        pointer.transform.localPosition = new Vector3(-250, pointer.localPosition.y, 0);
        barImg.color = Color.HSVToRGB(0, 1, 1);
        scoreText.color = Color.HSVToRGB(0, 1, 1);

    }
}
