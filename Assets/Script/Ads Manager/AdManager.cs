using UnityEngine;
using GoogleMobileAds.Api;
using System;
using UnityEngine.SceneManagement;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance;

    [Header("Use Test Ads (uncheck for release)")]
    public bool useTestAds = true;

    [Header("Ad Unit IDs - Banner")]
    public string bannerAdUnitId_Android = "ca-app-pub-3940256099942544/6300978111"; // Test Banner
    public string bannerAdUnitId_IOS = "ca-app-pub-3940256099942544/2934735716";

    public AdPosition bannerPosition = AdPosition.Bottom;

    [Header("Ad Unit IDs - Rewarded")]
    public string rewardAdUnitId_Android = "ca-app-pub-3940256099942544/5224354917"; // Test Reward
    public string rewardAdUnitId_IOS = "ca-app-pub-3940256099942544/1712485313";

    public SceneList sceneToLoad;
    public bool loadMenuAfterInit = true;


    private BannerView bannerView;
    private RewardedAd rewardedAd;
    private bool isBannerLoaded = false;
    private bool isBannerVisible = false;

    private Action onRewardedAdComplete;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;


            // Init AdMob
            Scene currentScene = SceneManager.GetActiveScene();
            MobileAds.Initialize(initStatus =>
            {


                // Load ads
                LoadRewardAd();
                InitBanner();
                ShowBanner();



                if (currentScene.name == "Boot_Scene")
                {
                    Debug.Log("Google Mobile Ads SDK initialized");
                    SceneManager.LoadScene(sceneToLoad.ToString());
                }


                // Nhảy sang menu scene

            });
            //DontDestroyOnLoad(gameObject);
            // Đảm bảo khi đổi scene, banner vẫn còn
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

   void stars()
    {
        




#if UNITY_EDITOR

    SceneManager.LoadScene(sceneToLoad.ToString());
    
#endif
    }



    #region Banner
    private string GetBannerAdUnitId()
    {
#if UNITY_ANDROID
        return bannerAdUnitId_Android;
#elif UNITY_IOS
        return bannerAdUnitId_IOS;
#else
        return "unexpected_platform";
#endif
    }

    public void InitBanner()
    {
        if (isBannerLoaded && bannerView != null) return;

        string adUnitId = GetBannerAdUnitId();
        bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);

        AdRequest request = new AdRequest();
        bannerView.LoadAd(request);

        isBannerLoaded = true;
        Debug.Log("[AdManager] Banner loaded.");

    }

    public void ShowBanner()
    {
        if (bannerView == null)
            InitBanner();

        bannerView?.Show();
        isBannerVisible = true;
        Debug.Log("[AdManager] Banner hiển thị.");
    }

    public void HideBanner()
    {
        bannerView?.Hide();
        isBannerVisible = false;
        Debug.Log("[AdManager] Banner ẩn.");
    }

    public bool IsBannerVisible()
    {
        return isBannerVisible;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Nếu banner mất khi load scene → khởi tạo lại
        if (bannerView == null)
        {
            Debug.LogWarning("[AdManager] Banner bị mất khi chuyển scene → reload lại.");
            InitBanner();
            if (isBannerVisible) ShowBanner();
        }
    }
    #endregion

    #region Rewarded
    private string GetRewardAdUnitId()
    {
#if UNITY_ANDROID
        return rewardAdUnitId_Android;
#elif UNITY_IOS
        return rewardAdUnitId_IOS;
#else
        return "unexpected_platform";
#endif
    }

    public void LoadRewardAd()
    {
        string adUnitId = GetRewardAdUnitId();
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

    public void ShowRewardAd(Action onRewardEarned, Action onAdClosed)
    {
        if (rewardedAd != null)
        {
            rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Reward Ad đã đóng");
                onAdClosed?.Invoke();
                LoadRewardAd();
            };

            rewardedAd.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogWarning("Reward Ad đóng do lỗi: " + error);
                onAdClosed?.Invoke();
                LoadRewardAd();
            };

            rewardedAd.Show((Reward reward) =>
            {
                Debug.Log($"User earned reward: {reward.Type} - {reward.Amount}");
                onRewardEarned?.Invoke();
            });
        }
        else
        {
            Debug.LogWarning("Rewarded ad not ready.");
            onAdClosed?.Invoke();
            LoadRewardAd();
        }
    }
    #endregion
}
