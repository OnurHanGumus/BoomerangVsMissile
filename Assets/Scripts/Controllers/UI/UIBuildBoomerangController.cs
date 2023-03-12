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
using Data.ValueObject;

public class UIBuildBoomerangController : MonoBehaviour
{
    #region Self Variables
    #region Public Variables
    #endregion
    #region SerializeField Variables
    [SerializeField] private TextMeshProUGUI scoreText, commentText;
    [SerializeField] private Transform pointer;

    [SerializeField] private Image barImg;
    #endregion

    #region Private Variables
    private int _counter = 0;
    private float _counterMaksValue = 25;
    private float _positionIncreaseValue = 20;
    private const int _selectedHSVMaksValue = 120;
    private float _colorIncreaseValue;

    private UIData _data;

    #endregion
    #endregion
    private void Awake()
    {
        Init();
    }
    private void Init()
    {
        _data = GetData();
        _positionIncreaseValue *= _data.ComboInputIncreaseAmount;
        _counterMaksValue /= _data.ComboInputIncreaseAmount;
        _counterMaksValue = Mathf.FloorToInt(_counterMaksValue);

        _colorIncreaseValue = _selectedHSVMaksValue / _counterMaksValue;
    }
    public UIData GetData() => Resources.Load<CD_UI>("Data/CD_UI").Data;

    public void OnAnimationSpeedIncreased()
    {
        if (_counter >= _counterMaksValue)
        {
            return;
        }
        ++_counter;
        pointer.transform.localPosition = new Vector3(pointer.transform.localPosition.x + _positionIncreaseValue, pointer.localPosition.y, 0);
        scoreText.text = ((double)(_counter * _data.ComboInputIncreaseAmount)).ToString() + "x";
        barImg.color = Color.HSVToRGB((float)((_counter * _colorIncreaseValue) /255f), 1, 1);
        scoreText.color = Color.HSVToRGB((float)((_counter * _colorIncreaseValue) / 255f), 1, 1);
        commentText.color = Color.HSVToRGB((float)((_counter * _colorIncreaseValue) / 255f), 1, 1);

    }
    public void OnBoomerangDisapeared()
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
        scoreText.text = (_counter * _data.ComboInputIncreaseAmount).ToString() + "x";
        pointer.transform.localPosition = new Vector3(-250, pointer.localPosition.y, 0);
        barImg.color = Color.HSVToRGB(0, 1, 1);
        scoreText.color = Color.HSVToRGB(0, 1, 1);
        commentText.color = Color.HSVToRGB(0, 1, 1);

    }
}
