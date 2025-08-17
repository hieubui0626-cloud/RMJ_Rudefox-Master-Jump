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
                revivePanel.SetActive(false);

                if (Ads_Reward_Manager.Instance != null)
                {
                    FindObjectOfType<Ads_Reward_Manager>().ShowRewardAd(
                        onRewardEarned: () =>
                        {
                            // Xem hết → revive
                            ReviveManager.Instance.OnReviveConfirmed();
                        },
                        onAdClosed: () =>
                        {
                            // Thoát sớm → tiếp tục chơi
                            if (PlayerController.Instance != null)
                            {
                                PlayerController.Instance.Disableplayer = false;
                                if (PlayerController.Instance.meshRenderer != null)
                                    PlayerController.Instance.meshRenderer.enabled = true;
                            }
                        }
                    );
                }
                else
                {
                    ReviveManager.Instance.OnReviveConfirmed();
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
