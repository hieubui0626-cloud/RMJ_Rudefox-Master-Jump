using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
//public enum AdPosition { Top, Bottom }
public class BannerAdManager : MonoBehaviour
{
    public static BannerAdManager Instance;

    [Header("AdMob Banner Settings")]
    public bool useAdMob = false;
    public string bannerAdUnitId = "YOUR_BANNER_AD_UNIT_ID";
    public AdPosition bannerPosition = AdPosition.Bottom;

    // private BannerView bannerView;
    private bool isBannerLoaded = false;
    private bool isVisible = false;

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

        if (useAdMob)
        {
            // MobileAds.Initialize(initStatus => Debug.Log("AdMob Initialized"));
        }
    }

    public void InitBanner()
    {
        if (isBannerLoaded) return; // Đã load rồi thì bỏ qua

        if (!useAdMob)
        {
            Debug.Log("Test Mode: Banner giả lập.");
            isBannerLoaded = true;
            return;
        }

        // AdMob Banner:
        /*
        bannerView = new BannerView(bannerAdUnitId, AdSize.Banner, bannerPosition);
        AdRequest request = new AdRequest.Builder().Build();
        bannerView.LoadAd(request);
        */

        isBannerLoaded = true;
    }

    public void ShowBanner()
    {
        if (!isBannerLoaded)
            InitBanner();

        isVisible = true;
        Debug.Log("Hiển thị banner");
        // if (bannerView != null) bannerView.Show();
    }

    public void HideBanner()
    {
        isVisible = false;
        Debug.Log("Ẩn banner");
        // if (bannerView != null) bannerView.Hide();
    }

    public bool IsBannerVisible()
    {
        return isVisible;
    }
}
