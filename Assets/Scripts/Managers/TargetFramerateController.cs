using System;
using Enums;
using Extentions;
using Keys;
using Signals;
using UnityEngine;

public class TargetFramerateController : MonoBehaviour
{
    #region Self Variables

    #region Public Variables

    #endregion

    #endregion

    private void Awake()
    {
        Application.targetFrameRate = 120;

    }
}