using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityRandom = UnityEngine.Random;
using Controllers;
using Managers;
using Enums;
using Signals;
using GoogleMobileAds.Api;

public class AdInterstitialManager : MonoBehaviour
{
    #region Self Variables

    #region Serialized Variables
    [SerializeField] private bool isTest = true;
    [SerializeField] private int levelsRequiredForAd = 2;
    #endregion

    #region Private Variables
    private string _testInterstitialId = "ca-app-pub-3940256099942544/1033173712",
                       _interstitialId = "ca-app-pub-4708991160900195/1593992855";
    private InterstitialAd _interstitialAd = null;
    private AdRequest _adRequest = null;
    private bool _isFirstTime = true;
    private int _passedLevelsCount;
    private bool _isLastLevelSuccessful;
    #endregion

    #endregion

    private void Awake()
    {
        Init();
        RequestAd();
        SubscribeEvents();
    }

    #region Event Subscription

    private void SubscribeEvents()
    {
        CoreGameSignals.Instance.onPlay += OnPlay;
        CoreGameSignals.Instance.onLevelSuccessful += OnLevelSuccessful;
        CoreGameSignals.Instance.onLevelFailed += OnLevelFailed;
    }

    private void UnsubscribeEvents()
    {
        CoreGameSignals.Instance.onPlay -= OnPlay;
        CoreGameSignals.Instance.onLevelSuccessful -= OnLevelSuccessful;
        CoreGameSignals.Instance.onLevelFailed -= OnLevelFailed;
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    #endregion

    private void OnPlay()
    {
        _isLastLevelSuccessful = false;
        if (!_isFirstTime)
        {
            RequestAd();
            _isFirstTime = true;
        }
    }

    private void OnLevelSuccessful()
    {
        _passedLevelsCount++;
        _isLastLevelSuccessful = true;
    }

    private void OnLevelFailed()
    {
        _isLastLevelSuccessful = false;
    }

    private void Init()
    {
        MobileAds.Initialize((InitializationStatus initStatus) => { });
    }

    private void RequestAd()
    {
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        _adRequest = new AdRequest();
        _adRequest.Keywords.Add("unity-admob-sample");

        InterstitialAd.Load(isTest ? _testInterstitialId : _interstitialId, _adRequest,
          (InterstitialAd ad, LoadAdError error) =>
          {
              if (error != null || ad == null)
              {
                  return;
              }
              _interstitialAd = ad;
          });
    }

    public void ShowAd()
    {
        if (!_isLastLevelSuccessful || _passedLevelsCount < levelsRequiredForAd)
        {
            return;
        }

        _passedLevelsCount = 0;
        _isLastLevelSuccessful = false;

        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            _interstitialAd.Show();
            _isFirstTime = false;
        }
    }

    #region AdEvents Not Working -- Use normal signals instead of this
    /// <summary>
    /// listen to events the banner may raise.
    /// </summary>
    private void ListenToAdEvents(InterstitialAd ad)
    {
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            _isFirstTime = true;
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            _isFirstTime = true;
        };
    }
    #endregion
}
