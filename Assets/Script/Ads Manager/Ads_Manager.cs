using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using System;
using UnityEngine.SceneManagement;

public class Ads_Manager : MonoBehaviour
{
    public static Ads_Manager Instance;
    [Header("Switch Test / Real Ads")]
    public bool useTestAds = true;
    public bool ads_Active;

    public SceneList sceneToLoad;


    // Test IDs (Google cung cấp sẵn)
    private string testRewardId_Android = "ca-app-pub-3940256099942544/5224354917";
    private string testRewardId_IOS = "ca-app-pub-3940256099942544/1712485313";

    // Real IDs (bạn gắn từ AdMob)
    [Header("Real Reward Ad Unit IDs")]
    public string realRewardId_Android = "ca-app-pub-3940256099942544/6300978111";
    public string realRewardId_IOS = "ca-app-pub-3940256099942544/1712485313";

    

    // Test IDs
    private string testBannerId_Android = "ca-app-pub-3940256099942544/6300978111";
    private string testBannerId_IOS = "ca-app-pub-3940256099942544/2934735716";

    // Real IDs (gắn từ AdMob của bạn)
    [Header("Real Ad Unit IDs")]
    public string realBannerId_Android = "ca-app-pub-2045503601876077/1987630747";
    public string realBannerId_IOS = "ca-app-pub-2045503601876077/8696509322";

    private BannerView bannerView;

    private string GetBannerAdUnitId()
    {
#if UNITY_ANDROID
        return useTestAds ? testBannerId_Android : realBannerId_Android;
#elif UNITY_IOS
        return useTestAds ? testBannerId_IOS : realBannerId_IOS;
#else
        return "unexpected_platform";
#endif
    }

    private string GetRewardAdUnitId()
    {
#if UNITY_ANDROID
        return useTestAds ? testRewardId_Android : realRewardId_Android;
#elif UNITY_IOS
        return useTestAds ? testRewardId_IOS : realRewardId_IOS;
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
            //DontDestroyOnLoad(gameObject);

            
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("[AdManager] Google Mobile Ads SDK initialized");
            LoadRewardAd();
            
            ads_Active = true;

            


        });
    }
    #region Test
    /*
    public void LoadScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        if (currentScene.name == "Boot_Scene")
        {
            if (FirebaseManager.IsReady)
            {
                if (GoogleFirebaseAuth.Instance.IsSignedIn())
                {
                    Debug.Log("[AdManager] Firebase ready & user logged in -> Load GameScene");
                    SceneManager.LoadScene(sceneToLoad.ToString());
                }
                else
                {
                    Debug.Log("[AdManager] Firebase ready but no user -> Load LoginScene");
                    SceneManager.LoadScene("LoginScene");
                }
            }
            else
            {
                Debug.Log("[AdManager] Firebase not ready -> wait...");
                FirebaseManager.OnFirebaseReady += () =>
                {
                    if (GoogleFirebaseAuth.Instance.IsSignedIn())
                    {
                        SceneManager.LoadScene(sceneToLoad.ToString());
                    }
                    else
                    {
                        SceneManager.LoadScene("LoginScene");
                    }
                };
            }
        }

    }
    */
    #endregion
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
    #region Banner
    public void ShowBanner(AdPosition position = AdPosition.Bottom)
    {
        if (bannerView != null) bannerView.Destroy();

        string adUnitId = GetBannerAdUnitId();
        bannerView = new BannerView(adUnitId, AdSize.Banner, position);

        AdRequest request = new AdRequest();
        bannerView.LoadAd(request);
    }

    public void HideBanner()
    {
        bannerView?.Hide();
    }
    #endregion
}
