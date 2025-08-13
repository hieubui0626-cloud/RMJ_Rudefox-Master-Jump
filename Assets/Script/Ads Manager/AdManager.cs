using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance;

    [Header("Use Test Ads (uncheck for release)")]
    public bool useTestAds = true;

    [Header("Ad Unit IDs - Banner")]
    public string bannerAdUnitId_Android = "ca-app-pub-3940256099942544/6300978111"; // Test Banner
    public string bannerAdUnitId_IOS = "ca-app-pub-3940256099942544/2934735716";

    [Header("Ad Unit IDs - Rewarded")]
    public string rewardAdUnitId_Android = "ca-app-pub-3940256099942544/5224354917"; // Test Reward
    public string rewardAdUnitId_IOS = "ca-app-pub-3940256099942544/1712485313";

    private BannerView bannerView;
    private RewardedAd rewardedAd;

    private Action onRewardedAdComplete;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("Google Mobile Ads SDK initialized");
        });

        LoadRewardAd();
    }

    #region Banner
    public void ShowBanner(AdPosition position = AdPosition.Bottom)
    {
#if UNITY_ANDROID
        string adUnitId = bannerAdUnitId_Android;
#elif UNITY_IOS
        string adUnitId = bannerAdUnitId_IOS;
#else
        string adUnitId = "unexpected_platform";
#endif

        if (bannerView != null)
        {
            bannerView.Show();
            return;
        }

        bannerView = new BannerView(adUnitId, AdSize.Banner, position);
        bannerView.LoadAd(new AdRequest());
    }

    public void HideBanner()
    {
        bannerView?.Hide();
    }

    public void DestroyBanner()
    {
        bannerView?.Destroy();
        bannerView = null;
    }
    #endregion

    #region Rewarded
    public void LoadRewardAd()
    {
#if UNITY_ANDROID
        string adUnitId = rewardAdUnitId_Android;
#elif UNITY_IOS
        string adUnitId = rewardAdUnitId_IOS;
#else
        string adUnitId = "unexpected_platform";
#endif

        Debug.Log("Loading rewarded ad...");

        RewardedAd.Load(adUnitId, new AdRequest(), (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Failed to load rewarded ad: " + error);
                return;
            }

            rewardedAd = ad;
            Debug.Log("Rewarded ad loaded.");
        });
    }

    public void ShowRewardAd(Action onComplete)
    {
        if (rewardedAd != null)
        {
            rewardedAd.Show((Reward reward) =>
            {
                Debug.Log($"User earned reward: {reward.Type} - {reward.Amount}");
                onComplete?.Invoke();
                LoadRewardAd(); // Load lại quảng cáo sau khi xem
            });
        }
        else
        {
            Debug.LogWarning("Rewarded ad not ready, restart level");
            GameManager.Instance.RestartLevel();
        }
    }



    #endregion
}
