using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public SceneList sceneToLoad;
    public SceneList sceneToReset;
    public SceneList sceneToUndo;

    [Header("Timer Settings")]
    private float levelTimer = 0f;
    private bool isTiming = false;
    public TextMeshProUGUI timerText; // Gán trong Canvas UI
    public TextMeshProUGUI CompletePanelTimeText;
    public GameObject CompletePanel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartTimer();
    }

    private void Update()
    {
        if (isTiming)
        {
            levelTimer += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(levelTimer / 60);
            float seconds = levelTimer % 60;
            timerText.text = $"{minutes:00}:{seconds:00.00}"; // mm:ss.ff
            //CompletePanelTimeText = timerText;
             // mm:ss.ff
        }
    }

    public void StartTimer()
    {
        levelTimer = 0f;
        isTiming = true;
    }

    public void StopTimer()
    {
        isTiming = false;
    }

    public void RestartLevel()
    {
        string address = sceneToReset.ToString();
        Addressables.LoadSceneAsync(address, LoadSceneMode.Single, true);
        //SceneManager.LoadScene(sceneToReset.ToString());
        PlayerController.Instance.Disableplayer = false;

        if (PlayerController.Instance.meshRenderer != null)
            PlayerController.Instance.meshRenderer.enabled = true;

        if (ReviveManager.Instance != null)
            ReviveManager.Instance.ResetReviveStatus();
    }

    public void SceneUndo()
    {
        string undoaddress = sceneToUndo.ToString();
        Addressables.LoadSceneAsync(undoaddress, LoadSceneMode.Single, true);
        //SceneManager.LoadScene(sceneToReset.ToString());
        
    }
    /* PlayerFerb Load
    public void LoadNextLevel()
    {
        StopTimer();

        string currentScene = SceneManager.GetActiveScene().name;
        string key = "BestTime_" + currentScene;

        // So sánh và lưu Best Time
        if (PlayerPrefs.HasKey(key))
        {
            float oldBest = PlayerPrefs.GetFloat(key);
            if (levelTimer < oldBest)
            {
                PlayerPrefs.SetFloat(key, levelTimer);
                PlayerPrefs.Save();
            }
        }
        else
        {
            PlayerPrefs.SetFloat(key, levelTimer);
            PlayerPrefs.Save();
        }

        // Đánh dấu level hoàn thành
        LevelMapManager mapManager = FindObjectOfType<LevelMapManager>();
        if (mapManager != null)
        {
            if (System.Enum.TryParse(currentScene, out SceneList currentSceneEnum))
            {
                LevelMapManager.MarkLevelComplete(currentSceneEnum);
            }
            else
            {
                Debug.LogWarning("Scene hiện tại không khớp enum SceneList, không thể lưu Complete");
            }
        }

        if (ReviveManager.Instance != null)
            ReviveManager.Instance.ResetReviveStatus();

        SceneManager.LoadScene(sceneToLoad.ToString());
    }
    */

    public void CompleteCheck()
    {
        
        CompletePanel.SetActive(true);
        CompletePanelTimeText.text = timerText.text;
        StopTimer();


    }
    public void LoadNextLevel()
    {
        

        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        //string currentScene = SceneManager.GetActiveScene().name;

        // 🔹 So sánh & lưu Best Time lên Firebase
        FirebaseManager.Instance.LoadBestTime(currentScene, (oldBest) =>
        {
            if (oldBest < 0 || levelTimer < oldBest)
            {
                FirebaseManager.Instance.SaveBestTime(currentScene, levelTimer);
            }
        });

        // 🔹 Đánh dấu level hoàn thành
        FirebaseManager.Instance.MarkLevelComplete(currentScene);

        if (ReviveManager.Instance != null)
            ReviveManager.Instance.ResetReviveStatus();

        //SceneManager.LoadScene(sceneToLoad.ToString());
        string nextAddress = sceneToLoad.ToString();
        Addressables.LoadSceneAsync(nextAddress, LoadSceneMode.Single, true);
    }
}
