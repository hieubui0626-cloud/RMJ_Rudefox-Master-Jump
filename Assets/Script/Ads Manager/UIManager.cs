using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Revive UI")]
    public GameObject revivePanel;
    public Button yesButton;
    public Button noButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (yesButton != null)
        {
            yesButton.onClick.RemoveAllListeners();
            yesButton.onClick.AddListener(() =>
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    Debug.LogWarning("⚠️ No internet connection – skipping ads, reviving directly.");
                    ReviveManager.Instance.OnReviveConfirmed();
                    return;
                }
                revivePanel.SetActive(false);
                void HandleRevive(bool success)
                {
                    if (success) ReviveManager.Instance.OnReviveConfirmed();
                    else
                    {
                        if (PlayerController.Instance != null)
                        {
                            PlayerController.Instance.Disableplayer = false;
                            if (PlayerController.Instance.meshRenderer != null)
                                PlayerController.Instance.meshRenderer.enabled = true;
                        }
                    }
                }
                if (!Boots_Level.Instance.Replace_GGAds_by_UnityAds)
                {
                    if (Ads_Manager.Instance?.ads_Active == true)
                        Ads_Manager.Instance.ShowRewardAd(() => HandleRevive(true), () => HandleRevive(false));
                    else
                        HandleRevive(true);
                }
                else
                {
                    if (UnityAdsManager.Instance?.ads_Active == true)
                        UnityAdsManager.Instance.ShowRewardAd(() => HandleRevive(true), () => HandleRevive(false));
                    else
                        HandleRevive(true);
                }





            });
        }

        if (noButton != null)
        {
            noButton.onClick.RemoveAllListeners();
            noButton.onClick.AddListener(() =>
            {
                revivePanel.SetActive(false);
                GameManager.Instance.RestartLevel();
            });
        }
    }

    public void ShowReviveOption()
    {
        if (revivePanel == null)
        {
            Debug.LogWarning("Revive panel is missing.");
            GameManager.Instance.RestartLevel();
            return;
        }

        if (ReviveManager.Instance.HasRevived())
        {
            GameManager.Instance.RestartLevel();
        }
        else
        {
            revivePanel.SetActive(true);
        }
    }
}
