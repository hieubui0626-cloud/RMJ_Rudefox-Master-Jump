using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject revivePanel;
    public Button yesButton;
    public Button noButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        AdManager.Instance.ShowBanner();

        // Gắn callback nút Yes
        if (yesButton != null)
        {
            yesButton.onClick.RemoveAllListeners();
            yesButton.onClick.AddListener(() =>
            {
                revivePanel.SetActive(false);

                // Xem quảng cáo trước khi revive
                if (AdManager.Instance != null)
                {
                    AdManager.Instance.ShowRewardAd(() =>
                    {
                        ReviveManager.Instance.OnReviveConfirmed();
                    });
                }
                else
                {
                    // Nếu không có AdManager, revive luôn
                    ReviveManager.Instance.OnReviveConfirmed();
                }
            });
        }

        // Gắn callback nút No
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

    

    private void OnYesClicked()
    {
        revivePanel.SetActive(false);
        ReviveManager.Instance.OnReviveConfirmed();

       
    }

    private void OnNoClicked()
    {
        revivePanel.SetActive(false);
        GameManager.Instance.RestartLevel();
    }

    public void ShowReviveOption()
    {
        if (revivePanel == null)
        {
            Debug.LogWarning("Revive panel is missing — cannot show revive UI.");
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
