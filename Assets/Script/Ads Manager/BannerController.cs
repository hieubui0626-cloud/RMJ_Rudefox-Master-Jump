using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BannerController : MonoBehaviour
{
    [Tooltip("Tick nếu muốn hiển thị banner trong scene này.")]
    public bool showBannerInThisScene = true;

    void Start()
    {
        if (BannerAdManager.Instance == null) return;

        if (showBannerInThisScene)
            BannerAdManager.Instance.ShowBanner();
        else
            BannerAdManager.Instance.HideBanner();
    }
}
