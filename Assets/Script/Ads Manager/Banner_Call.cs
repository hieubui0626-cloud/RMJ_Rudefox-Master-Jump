using UnityEngine;

public class Banner_Call : MonoBehaviour
{
    public bool BannerShow = true;

    void Start()
    {
        if (BannerShow)
        {
            Ads_Manager.Instance.ShowBanner(); // hiện banner
        }
        else
        {
            Ads_Manager.Instance.HideBanner(); // ẩn banner
        }
    }
}

    

    


