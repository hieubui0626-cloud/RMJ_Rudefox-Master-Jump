using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public SceneList sceneToLoad;
    public SceneList sceneToReset;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RestartLevel()
    {
        PlayerController.Instance.Disableplayer = false;
        if (PlayerController.Instance.meshRenderer != null)
            PlayerController.Instance.meshRenderer.enabled = true;

        if (ReviveManager.Instance != null)
            ReviveManager.Instance.ResetReviveStatus();

        SceneManager.LoadScene(sceneToReset.ToString());
    }

    public void LoadNextLevel()
    {
        if (ReviveManager.Instance != null)
            ReviveManager.Instance.ResetReviveStatus();

        SceneManager.LoadScene(sceneToLoad.ToString());

        if (BannerAdManager.Instance != null && BannerAdManager.Instance.IsBannerVisible())
            BannerAdManager.Instance.ShowBanner();
    }
}
