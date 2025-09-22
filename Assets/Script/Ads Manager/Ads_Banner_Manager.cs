using UnityEngine;
using GoogleMobileAds.Api;

public class AdsBannerManager : MonoBehaviour
{
    public static AdsBannerManager Instance;

    [Header("Switch Test / Real Ads")]
    public bool useTestAds = true; // ✅ Chỉ chỉnh ở đây 1 lần cho toàn project

    // Test IDs
    private string testBannerId_Android = "ca-app-pub-3940256099942544/6300978111";
    private string testBannerId_IOS = "ca-app-pub-3940256099942544/2934735716";

    // Real IDs (gắn từ AdMob của bạn)
    [Header("Real Ad Unit IDs")]
    public string realBannerId_Android = "ca-app-pub-3940256099942544/6300978111";
    public string realBannerId_IOS = "ca-app-pub-3940256099942544/2934735716";

    private BannerView bannerView;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        
    }

    private void Start()
    {
        MobileAds.Initialize(initStatus => { });
    }

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
}