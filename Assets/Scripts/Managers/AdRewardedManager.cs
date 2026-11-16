using System;
using Extensions;
using GoogleMobileAds.Api;
using Signals;
using UnityEngine;
using UnityEngine.Events;

namespace Managers
{
    public class AdRewardedManager : MonoSingleton<AdRewardedManager>
    {
        #region Self Variables

        #region Public Variables
        public bool IsTest => isTest;
        #endregion

        #region Serialized Variables
        [SerializeField] private bool isTest = true;
        [SerializeField] private string rewardedId = "ca-app-pub-4708991160900195/8130269787";
        #endregion

        #region Private Variables
        private readonly string _testRewardedId = "ca-app-pub-3940256099942544/5224354917";
        private RewardedAd _rewardedAd;
        private AdRequest _adRequest;
        private UnityAction _pendingRewardCallback;
        private UnityAction _pendingFailureCallback;
        private bool _isLoading;
        private bool _rewardGrantedForCurrentShow;
        #endregion

        #endregion

        protected override void Awake()
        {
            base.Awake();
            Init();
            SubscribeEvents();
        }

        private void Start()
        {
            RequestRewardedAd();
        }

        private void Init()
        {
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
            MobileAds.Initialize((InitializationStatus initStatus) => { });
        }

        #region Event Subscription
        private void SubscribeEvents()
        {
            AdSignals.Instance.onShowRewardedAd += ShowRewardedAd;
            AdSignals.Instance.onIsRewardedAdReady += IsAdReady;
        }

        private void UnsubscribeEvents()
        {
            if (AdSignals.Instance != null)
            {
                AdSignals.Instance.onShowRewardedAd -= ShowRewardedAd;
                AdSignals.Instance.onIsRewardedAdReady -= IsAdReady;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnsubscribeEvents();

            if (_rewardedAd != null)
            {
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }
        }
        #endregion

        public void RequestRewardedAd()
        {
            if (_isLoading)
            {
                return;
            }

            if (_rewardedAd != null)
            {
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }

            _isLoading = true;
            _adRequest = new AdRequest();
            _adRequest.Keywords.Add("unity-admob-sample");

            string adUnitId = isTest ? _testRewardedId : rewardedId;

            RewardedAd.Load(adUnitId, _adRequest, OnAdLoaded);
        }

        private void OnAdLoaded(RewardedAd ad, LoadAdError error)
        {
            _isLoading = false;

            if (error != null || ad == null)
            {
                Debug.LogWarning($"[AdRewardedManager] Rewarded ad failed to load: {error}");
                return;
            }

            _rewardedAd = ad;
            RegisterAdEvents(_rewardedAd);
        }

        private void RegisterAdEvents(RewardedAd ad)
        {
            ad.OnAdFullScreenContentClosed += HandleAdClosed;
            ad.OnAdFullScreenContentFailed += HandleAdFailedToOpen;
        }

        private void HandleAdClosed()
        {
#if UNITY_EDITOR
            // In Unity Editor, if the tester closed the placeholder ad before Google's 5s countdown finished,
            // award the reward so testing works properly.
            if (isTest && !_rewardGrantedForCurrentShow && _pendingRewardCallback != null)
            {
                Debug.Log("[AdRewardedManager] Unity Editor test ad closed: awarding reward.");
                _rewardGrantedForCurrentShow = true;
                _pendingRewardCallback?.Invoke();
                AdSignals.Instance.onRewardedAdCompleted?.Invoke();
            }
#endif
            _rewardGrantedForCurrentShow = false;
            _pendingRewardCallback = null;
            _pendingFailureCallback = null;
            RequestRewardedAd();
        }

        private void HandleAdFailedToOpen(AdError error)
        {
            Debug.LogWarning($"[AdRewardedManager] Rewarded ad failed to open full screen: {error}");

#if UNITY_EDITOR
            if (isTest && !_rewardGrantedForCurrentShow && _pendingRewardCallback != null)
            {
                Debug.Log("[AdRewardedManager] Editor test fallback: awarding reward on open failure.");
                _rewardGrantedForCurrentShow = true;
                _pendingRewardCallback?.Invoke();
                AdSignals.Instance.onRewardedAdCompleted?.Invoke();
                _pendingRewardCallback = null;
                _pendingFailureCallback = null;
                RequestRewardedAd();
                return;
            }
#endif
            _rewardGrantedForCurrentShow = false;
            UnityAction failureAction = _pendingFailureCallback;
            _pendingRewardCallback = null;
            _pendingFailureCallback = null;

            failureAction?.Invoke();
            RequestRewardedAd();
        }

        public bool IsAdReady()
        {
            return _rewardedAd != null && _rewardedAd.CanShowAd();
        }

        public void ShowRewardedAd(UnityAction onRewardEarned, UnityAction onAdFailedOrSkipped)
        {
            _pendingRewardCallback = onRewardEarned;
            _pendingFailureCallback = onAdFailedOrSkipped;
            _rewardGrantedForCurrentShow = false;

            if (IsAdReady())
            {
                _rewardedAd.Show((Reward reward) =>
                {
                    if (!_rewardGrantedForCurrentShow)
                    {
                        _rewardGrantedForCurrentShow = true;
                        Debug.Log($"[AdRewardedManager] User earned reward: {reward.Type} ({reward.Amount})");
                        _pendingRewardCallback?.Invoke();
                        AdSignals.Instance.onRewardedAdCompleted?.Invoke();
                    }
                });
                return;
            }

#if UNITY_EDITOR
            if (isTest)
            {
                Debug.Log("[AdRewardedManager] Editor Simulation: Rewarded ad not cached yet, granting simulated reward.");
                _pendingRewardCallback?.Invoke();
                AdSignals.Instance.onRewardedAdCompleted?.Invoke();
                _pendingRewardCallback = null;
                _pendingFailureCallback = null;
                RequestRewardedAd();
                return;
            }
#endif

            Debug.LogWarning("[AdRewardedManager] Rewarded ad is not ready to display.");
            UnityAction failure = _pendingFailureCallback;
            _pendingRewardCallback = null;
            _pendingFailureCallback = null;
            failure?.Invoke();

            RequestRewardedAd();
        }
    }
}
