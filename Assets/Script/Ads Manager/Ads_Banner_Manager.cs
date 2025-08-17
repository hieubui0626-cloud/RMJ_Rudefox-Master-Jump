using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;


public class Ads_Banner_Manager : MonoBehaviour
{
    public static Ads_Banner_Manager Instance;
    public string bannerAdUnitId_Android = "ca-app-pub-3940256099942544/6300978111";
    public string bannerAdUnitId_IOS = "ca-app-pub-3940256099942544/2934735716";
    public AdPosition bannerPosition = AdPosition.Bottom;
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

    private BannerView bannerView;

    void Start()
    {
        string adUnitId = GetBannerAdUnitId();
        bannerView = new BannerView(adUnitId, AdSize.Banner, bannerPosition);

        bannerView.OnBannerAdLoaded += () =>
        {
            Debug.Log("[SceneBanner] Banner loaded for scene " + gameObject.scene.name);
            bannerView.Show();
        };

        bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError("[SceneBanner] Failed to load banner: " + error);
        };

        bannerView.LoadAd(new AdRequest());
    }

    void OnDestroy()
    {
        bannerView?.Destroy();
    }
}
