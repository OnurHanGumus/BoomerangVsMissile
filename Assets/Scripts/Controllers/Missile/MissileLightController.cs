using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MissileLightController : MonoBehaviour
{
    #region Self Variables

    #region Serialized Variables

    #endregion
    #region Private Variables
    private Light _light;
    private bool _isUpper = true;
    #endregion
    #endregion

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _light = GetComponent<Light>();
        SetIntensity(2);
    }

    private void OnEnable()
    {
        //SetIntensity(1);
    }

    private void OnDisable()
    {
        
    }

    private void SetIntensity(float value)
    {
        _light.DOIntensity(value, 0.5f).OnComplete(()=> 
        {
            _isUpper = value == 2f;
            SetIntensity(_isUpper? 1:2);
        });
    }
}
