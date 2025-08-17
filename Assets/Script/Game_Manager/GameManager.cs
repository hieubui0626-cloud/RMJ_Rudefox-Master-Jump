using UnityEngine;
using System.Collections;
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
        SceneManager.LoadScene(sceneToReset.ToString());
        PlayerController.Instance.Disableplayer = false;

        if (PlayerController.Instance.meshRenderer != null)
            PlayerController.Instance.meshRenderer.enabled = true;

        if (ReviveManager.Instance != null)
            ReviveManager.Instance.ResetReviveStatus();


        
    }

    public void LoadNextLevel()
    {
        // Đánh dấu level hiện tại hoàn thành
        LevelMapManager mapManager = FindObjectOfType<LevelMapManager>();
        if (mapManager != null)
        {
            // Lấy scene hiện tại từ SceneManager
            if (System.Enum.TryParse(SceneManager.GetActiveScene().name, out SceneList currentScene))
            {
                LevelMapManager.MarkLevelComplete(currentScene);
            }
            else
            {
                Debug.LogWarning("Scene hiện tại không khớp enum SceneList, không thể lưu Complete");
            }
        }

        // Reset trạng thái revive
        if (ReviveManager.Instance != null)
            ReviveManager.Instance.ResetReviveStatus();

        // Load scene tiếp theo
        SceneManager.LoadScene(sceneToLoad.ToString());

        // Hiển thị banner nếu đang bật
        
    }

    
}
