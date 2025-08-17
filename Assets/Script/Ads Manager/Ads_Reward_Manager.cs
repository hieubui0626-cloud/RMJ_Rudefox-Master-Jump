using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class Ads_Reward_Manager : MonoBehaviour
{
    public static Ads_Reward_Manager Instance;
    public string rewardAdUnitId_Andorid = "ca-app-pub-3940256099942544/5224354917"; // Test Reward Android
    public string rewardAdUnitId_IOS = "ca-app-pub-3940256099942544/1712485313"; // Test Reward iOS

    
    private string GetRewardAdUnitId()
    {
#if UNITY_ANDROID
        return rewardAdUnitId_Andorid;
#elif UNITY_IOS
        return rewardAdUnitId_IOS;
#else
        return "unexpected_platform";
#endif
    }


    private RewardedAd rewardedAd;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            MobileAds.Initialize(initStatus =>
            {
                Debug.Log("[AdManager] Google Mobile Ads SDK initialized");
                LoadRewardAd();
            });
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    #region Rewarded
    public void LoadRewardAd()
    {
        Debug.Log("[AdManager] Loading rewarded ad...");
        string rewardAdUnitId = GetRewardAdUnitId();

    RewardedAd.Load(rewardAdUnitId, new AdRequest(), (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("[AdManager] Failed to load rewarded ad: " + error);
                return;
            }

            rewardedAd = ad;
            Debug.Log("[AdManager] Rewarded ad loaded.");
        });
    }

    public void ShowRewardAd(Action onRewardEarned, Action onAdClosed)
    {
        if (rewardedAd != null)
        {
            rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("[AdManager] Reward Ad closed");
                onAdClosed?.Invoke();
                LoadRewardAd();
            };

            rewardedAd.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogWarning("[AdManager] Reward Ad failed to show: " + error);
                onAdClosed?.Invoke();
                LoadRewardAd();
            };

            rewardedAd.Show((Reward reward) =>
            {
                Debug.Log($"[AdManager] User earned reward: {reward.Type} - {reward.Amount}");
                onRewardEarned?.Invoke();
            });
        }
        else
        {
            Debug.LogWarning("[AdManager] Rewarded ad not ready.");
            onAdClosed?.Invoke();
            LoadRewardAd();
        }
    }
    #endregion
}
