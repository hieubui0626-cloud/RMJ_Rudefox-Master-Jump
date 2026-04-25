using UnityEngine;

public class Banner_Call : MonoBehaviour
{
    public bool BannerShow = true;

    void Start()
    {
        // === Kiểm tra trạng thái Banner ===
        if (BannerShow)
        {
            ShowBanner();
        }
        else
        {
            HideBanner();
        }
    }

    void ShowBanner()
    {
        // Ưu tiên hệ thống Ads_Manager cũ nếu đang active
        if(!Boots_Level.Instance.Replace_GGAds_by_UnityAds)
        {
            if (Ads_Manager.Instance != null && Ads_Manager.Instance.ads_Active)
            {
                Ads_Manager.Instance.ShowBanner();
                Debug.Log("📢 Show Banner: Ads_Manager");
            }
            else
            {
                Debug.LogWarning("⚠️ Không có hệ thống banner nào sẵn sàng để hiển thị.");
            }
        }
        else
        {
            if (UnityAdsManager.Instance != null && UnityAdsManager.Instance.ads_Active)
            {
                UnityAdsManager.Instance.OnBannerLoaded();
                Debug.Log("📢 Show Banner: Unity Ads");
            }
            else
            {
                Debug.LogWarning("⚠️ Không có hệ thống banner nào sẵn sàng để hiển thị.");
            }
        }
        

        // Nếu Unity Ads đã sẵn sàng, gọi banner Unity
        
        
    }   

    void HideBanner()
    {
        // Tắt cả hai banner (nếu đang bật)
        if (Ads_Manager.Instance != null)
        {
            Ads_Manager.Instance.HideBanner();
            Debug.Log("🚫 Hide Banner: Ads_Manager");
        }

        if (UnityAdsManager.Instance != null)
        {
            UnityAdsManager.Instance.HideBanner();
            Debug.Log("🚫 Hide Banner: Unity Ads");
        }
    }
}






