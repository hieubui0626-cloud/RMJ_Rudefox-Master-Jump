using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Scene Control")]
    public SceneList sceneToLoad;
    public SceneList sceneToReset;
    public SceneList sceneToUndo;

    [Header("Timer Settings")]
    public float levelTimer = 0f;
    public bool is_Timing = false;
    public TextMeshProUGUI timerText; // Gán trong Canvas UI
    public TextMeshProUGUI CompletePanelTimeText;
    public GameObject CompletePanel;

    [Header("Token System")]
    public int currentLevelTokens = 0; // Token nhặt trong level hiện tại
    public TextMeshProUGUI tokenText;  // Gán UI trong Canvas để hiển thị
    public TextMeshProUGUI totalTokenText; // Hiển thị tổng token đã lưu
    public float countAnimationDuration = 1.0f; // Thời gian chạy animation

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartTimer();
        currentLevelTokens = 0;
        UpdateTokenUI();
        
    }

    private void Update()
    {
        if (is_Timing)
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
            timerText.text = $"{minutes:00}:{seconds:00.00}"; 

        }
    }

    public void StartTimer()
    {
        levelTimer = 0f;
        is_Timing = true;
    }

    public void StopTimer()
    {
        is_Timing = false;
    }


    
    #region TOKEN_COUNT
    

    // ================= TOKEN CONTROL =================
    public void AddToken(int amount)
    {
        currentLevelTokens += amount;
        UpdateTokenUI();
    }

    private void UpdateTokenUI()
    {
        if (tokenText != null)
        {
            tokenText.text = currentLevelTokens.ToString();
        }
    }

    private IEnumerator AnimateTokenCount(int from, int to)
    {
        float elapsed = 0f;
        while (elapsed < countAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / countAnimationDuration);

            int displayValue = Mathf.RoundToInt(Mathf.Lerp(from, to, t));
            if (totalTokenText != null)
                totalTokenText.text = displayValue.ToString();

            yield return null;
        }

        // đảm bảo kết thúc đúng số cuối
        if (totalTokenText != null)
            totalTokenText.text = to.ToString();
    }
    #endregion
    

    #region LEVEL CONTROL

    // ================= LEVEL CONTROL =================
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
    #endregion

    #region CompleteLevel
    
    public void CompleteCheck()
    {

        CompletePanel.SetActive(true);
        CompletePanelTimeText.text = timerText.text;
        StopTimer();
        

        // 🔹 Lấy tổng token hiện tại từ Firebase/cache để animate
        FirebaseManager.Instance.GetTotalTokens(total =>
        {
            int oldTotal = total;
            int newTotal = total + currentLevelTokens;

            // Animate số token tăng (chỉ hiển thị, chưa lưu Firebase)
            StartCoroutine(AnimateTokenCount(oldTotal, newTotal));

            Debug.Log($"🔔 Hiển thị cộng {currentLevelTokens} token (chưa lưu).");
        });

        int amountToAdd = currentLevelTokens;
        FirebaseManager.Instance.UpdateTotalTokens(amountToAdd, newTotal =>
        {
            Debug.Log($"✅ Đã lưu {amountToAdd} token, tổng mới = {newTotal}");

            // Reset lại token tạm
            currentLevelTokens = 0;
            UpdateTokenUI();


        });


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


        // 🔹 Số token sẽ cộng thêm
        


        // 🔹 Cập nhật token an toàn (Firebase + Cache)
        
    }

    #endregion

}
