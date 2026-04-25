using UnityEngine;
using UnityEngine.Advertisements;
using System;


public class UnityAdsManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public static UnityAdsManager Instance;
    public bool ads_Active { get; private set; } = false;
    public bool _testMode = true;
    public string Ads_Banner_placementID;
    public string Ads_Reward_placementID;

    [Header("Unity Ads Settings")]
    // Khai báo thêm iOS ID
    [SerializeField] private string _androidGameId = "YOUR_ANDROID_GAME_ID";
    [SerializeField] private string _iOSGameId = "YOUR_IOS_GAME_ID";
    [SerializeField] private string _webGLGameId = "YOUR_WEBGL_GAME_ID";

    // IDs cho các loại quảng cáo (sử dụng các biến này để dễ quản lý hơn)
    private string _rewardedAdUnitId;
    private string _bannerAdUnitId;

    private string _gameId;

    private Action onRewardEarned;
    private Action onAdClosed;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // >>> SỬA LỖI: Đưa logic gán Game ID vào Awake() <<<
#if UNITY_ANDROID
            _gameId = _androidGameId;
            _rewardedAdUnitId = Ads_Banner_placementID; // Gán ID thực tế của bạn
            _bannerAdUnitId = Ads_Reward_placementID;  // Gán ID thực tế của bạn

#elif UNITY_IOS
            _gameId = _iOSGameId;
            _rewardedAdUnitId = "Rewarded_iOS"; 
            _bannerAdUnitId = "Banner_iOS"; 

#elif UNITY_WEBGL
            _gameId = _webGLGameId;
            _rewardedAdUnitId = "Rewarded_WebGL"; 
            _bannerAdUnitId = "Banner_WebGL"; 
#endif
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeAds();
    }

    public void InitializeAds()
    {
        
        if (!Advertisement.isInitialized)
        {
            // Debug.Log để kiểm tra ID đang dùng
            Debug.Log($"Initializing Unity Ads for {_gameId} in test mode {_testMode}");
            Advertisement.Initialize(_gameId, _testMode, this);
        }
    }

    #region Banner
    public void LoadBanner()
    {
        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
        // >>> Dùng _bannerAdUnitId đã gán ở Awake() <<<
        Advertisement.Banner.Load(_bannerAdUnitId,
            new BannerLoadOptions
            {
                //loadCallback = OnBannerLoaded,
                errorCallback = OnBannerError
            });
    }

    public void OnBannerLoaded()
    {
        Advertisement.Banner.Show(_bannerAdUnitId);
        Debug.Log("Show Ads Banner");
    }
    
    private void OnBannerError(string message) => Debug.LogError($"Banner failed: {message}");
    public void HideBanner() => Advertisement.Banner.Hide();
    #endregion

    #region Rewarded
    public void ShowRewardAd(Action onRewardEarned = null, Action onAdClosed = null)
    {
        this.onRewardEarned = onRewardEarned;
        this.onAdClosed = onAdClosed;

        if (Advertisement.isInitialized)
        {
            // >>> Dùng _rewardedAdUnitId đã gán ở Awake() <<<
            Advertisement.Load(_rewardedAdUnitId, this);
        }
        else
        {
            Debug.LogWarning("⚠️ Unity Ads chưa khởi tạo, revive không hiển thị quảng cáo.");
            onAdClosed?.Invoke();
        }
    }

    // ===== Callback từ Unity Ads =====
    public void OnUnityAdsAdLoaded(string placementId)
    {
        // >>> Dùng _rewardedAdUnitId để so sánh <<<
        if (placementId == _rewardedAdUnitId)
        {
            Advertisement.Show(placementId, this);
        }
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        // >>> Dùng _rewardedAdUnitId để so sánh <<<
        if (placementId == _rewardedAdUnitId)
        {
            if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
            {
                Debug.Log("🎁 Reward xem xong quảng cáo!");
                onRewardEarned?.Invoke();
            }
            else
            {
                Debug.Log("⏹️ Quảng cáo bị đóng sớm");
                onAdClosed?.Invoke();
            }
        }
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"❌ Load ad lỗi: {error} - {message}");
        onAdClosed?.Invoke();
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"❌ Show ad lỗi: {error} - {message}");
        onAdClosed?.Invoke();
    }

    public void OnUnityAdsShowStart(string placementId) { }
    public void OnUnityAdsShowClick(string placementId) { }
    #endregion


    public void OnInitializationComplete()
    {
        ads_Active = true;
        Debug.Log("✅ Unity Ads initialized and active");
        LoadBanner(); // Tải banner ngay sau khi khởi tạo xong
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        ads_Active = false;
        Debug.LogError($"❌ Unity Ads initialization failed: {error} - {message}");
    }
}