using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Casual Settings")]
    public bool LimitRevive = false; // Nếu true, chỉ cho phép revive 1 lần mỗi level
    public TextMeshProUGUI forceText;
    
    [SerializeField] private string targetTag;
    [SerializeField] private float rotationSpeed = 10f;
    private Camera mainCamera;
    public RectTransform arrowRectTransform;
    public GameObject main_UI;
    public GameObject targetObject;

    [Header("Scene Control")]
    public SceneList sceneToLoad;
    public SceneList sceneToReset;
    public SceneList sceneToUndo;

    [Header("Revive System")]
    public GameObject revivePanel;
    private Vector3 lastSafePosition;
    public bool hasRevived = false;

    [Header("Timer Settings")]
    public bool Campaign;
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

    [Header("Height")]
    public float height = 0;
    public float currentHeightest = 0;
    public TextMeshProUGUI heightText; // Gán UI để hiển thị high score
    public TextMeshProUGUI Heightest_Player; // Gán UI để hiển thị high score
    public Transform playerTransform; // Gán Transform của Player để theo dõi chiều cao

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {

        forceText.gameObject.SetActive(false);
        mainCamera = Camera.main;
        if (Campaign)
        {
            levelTimer = 0f;
            is_Timing = true;
            targetTag= "Goal";

        }
        else
        {
            playerTransform = PlayerController.Instance.transform;
            Height_Count(height);
            targetTag = "Token";
            if (FirebaseManager.Instance != null)
            {
                FirebaseManager.Instance.LoadBestHeight(sceneToReset.ToString("F2"), bestHeight =>
                {
                    if (bestHeight >= 0)
                    {
                        Heightest_Player.text = $"Heightest:  {bestHeight}m";
                    }
                    else
                    {
                        Heightest_Player.text = "Heightest:  0m";
                    }
                });
            }
            else
            {
                if (PlayerPrefs.GetInt(sceneToReset.ToString() + "_BestHeight", 0) >= 0)
                {
                    Heightest_Player.text = "Heightest:  " + PlayerPrefs.GetInt(sceneToReset.ToString() + "_BestHeight", 0) + "m";
                }
                else
                {
                    Heightest_Player.text = "Heightest: 0m";
                }
            }

        }

        currentLevelTokens = 0;
        UpdateTokenUI();
        
    }

    private void Update()
    {
        if (is_Timing && Campaign)
        {
            levelTimer += Time.deltaTime;
            UpdateTimerUI();

        }
        if (!Campaign)
        {
            Height_Count(height);
        }
            
        UpdateUIForce();
        Arrow_Token_Rotate();
        Main_UI_Update();


    }
    #region UI Control

    public void Main_UI_Update()
    {
        if (PlayerController.Instance != null)
        {
            if (!PlayerController.Instance.isGrounded)
            {
                main_UI.SetActive(false);
            }
            else
            {
                main_UI.SetActive(true);
            }
        }
    }
    public void Arrow_Token_Rotate()
    {
        Transform closetarget = null;
        if(targetTag == null) return;
        if (targetTag == "Goal")
        {
            closetarget = GameObject.FindGameObjectWithTag(targetTag)?.transform;
            //Debug.Log($"🔍 Target Tag: {targetTag}, Found Target: {closetarget?.name}");
        }
        if (targetTag == "Token")
        {
            closetarget = FindClosestToken();
            //Debug.Log($"🔍 Target Tag: {targetTag}, Found Target: {closetarget?.name}");
        } 
        

        if (closetarget != null && mainCamera != null)
        {
            // Bước 1: Chuyển tọa độ 3D của Token ngoài World Space thành tọa độ màn hình (Screen Space)
            Vector3 tokenScreenPos = mainCamera.WorldToScreenPoint(closetarget.position);

            // Kiểm tra nếu Token nằm phía sau Camera (tránh lỗi mũi tên quay ngược hướng)
            if (tokenScreenPos.z < 0)
            {
                tokenScreenPos *= -1;
            }

            // Bước 2: Tính hướng từ vị trí mũi tên UI đến vị trí màn hình của Token
            Vector2 direction = (Vector2)tokenScreenPos - (Vector2)arrowRectTransform.position;

            // Bước 3: Tính góc quay (Z-axis)
            //float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Nếu Sprite mũi tên mặc định của bạn hướng thẳng lên (Up), hãy trừ đi 90 độ:
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // Bước 4: Xoay mượt mà mũi tên UI
            arrowRectTransform.rotation = Quaternion.Slerp(arrowRectTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    private Transform FindClosestToken()
    {
        GameObject[] tokens = GameObject.FindGameObjectsWithTag(targetTag);
        GameObject closest = null;
        float shortestDistance = Mathf.Infinity;
        Vector3 currentPosition = playerTransform.position;

        foreach (GameObject token in tokens)
        {
            float distanceToToken = Vector3.SqrMagnitude(token.transform.position - currentPosition); // Dùng SqrMagnitude trong 3D sẽ tối ưu hiệu năng hơn
            if (distanceToToken < shortestDistance)
            {
                shortestDistance = distanceToToken;
                closest = token;
            }
        }

        return closest != null ? closest.transform : null;
    }
    public void UpdateUIForce()
    {
        if (forceText != null)
        {
            if(!PlayerController.Instance.isCharging)
            {
                forceText.gameObject.SetActive(false);
            }
            else 
            {
                forceText.gameObject.SetActive(true);
                forceText.text = PlayerController.Instance.forecAmount.ToString("F2");
            }
                
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

    public void StopTimer()
    {
        is_Timing = false;
    }
    public void Height_Count(float currentHeight)
    {
        currentHeight = playerTransform.position.y;
        height = currentHeight;
        if (heightText != null && height >= currentHeightest)
        {
            currentHeightest = height;
            heightText.text = height.ToString("F2") + "m";
        }
    }

    #endregion




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
        ResetReviveStatus();
        if(!Campaign)
        {
            float currentHeight = currentHeightest;
            if (FirebaseManager.Instance != null)
            {
                FirebaseManager.Instance.LoadBestHeight(sceneToReset.ToString(), oldBest =>
                {
                    if (oldBest < 0 || currentHeight > oldBest)
                    {
                        FirebaseManager.Instance.SaveBestHeight(sceneToReset.ToString(), currentHeight);
                    }
                });
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
            else
            {
                PlayerPrefs.SetFloat(sceneToReset.ToString() + "_BestHeight", currentHeight);
                int oldTotal = PlayerPrefs.GetInt("TotalTokens", 0);
                int newTotal = oldTotal + currentLevelTokens;
                PlayerPrefs.SetInt("TotalTokens", newTotal);
                UpdateTokenUI();
            }
            
        }


    }

    public void SceneUndo()
    {
        string undoaddress = sceneToUndo.ToString();
        Addressables.LoadSceneAsync(undoaddress, LoadSceneMode.Single, true);
        //SceneManager.LoadScene(sceneToReset.ToString());
        if (!Campaign)
        {
            float currentHeight = currentHeightest;
            if (FirebaseManager.Instance != null)
            {
                FirebaseManager.Instance.LoadBestHeight(sceneToReset.ToString(), oldBest =>
                {
                    if (oldBest < 0 || currentHeight > oldBest)
                    {
                        FirebaseManager.Instance.SaveBestHeight(sceneToReset.ToString(), currentHeight);
                    }
                });
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
            else
            {
                PlayerPrefs.SetFloat(sceneToReset.ToString() + "_BestHeight", currentHeight);
                int oldTotal = PlayerPrefs.GetInt("TotalTokens", 0);
                int newTotal = oldTotal + currentLevelTokens;
                PlayerPrefs.SetInt("TotalTokens", newTotal);
                UpdateTokenUI();
            }

        }

    }

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
        ResetReviveStatus();
        

        //SceneManager.LoadScene(sceneToLoad.ToString());
        string nextAddress = sceneToLoad.ToString();
        Addressables.LoadSceneAsync(nextAddress, LoadSceneMode.Single, true);


        // 🔹 Số token sẽ cộng thêm



        // 🔹 Cập nhật token an toàn (Firebase + Cache)

    }
    #endregion

    

    #region Revive Option

    public void RecordSafePosition(Vector3 position)
    {
        lastSafePosition = position + new Vector3(0, 1, 0);
        Debug.Log($"📌 Recorded safe position: {lastSafePosition}");
    }

    public bool HasRevived()
    {
        return hasRevived;
    }

    public void OnReviveConfirmed()
    {
        hasRevived = true;

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.ReviveAt(lastSafePosition);
            Debug.Log($"✅ Player revived at: {lastSafePosition}");
        }
    }

    public void ResetReviveStatus()
    {
        hasRevived = false;
    }

    public void ShowReviveOption()
    {
        if (revivePanel == null)
        {
            Debug.LogWarning("Revive panel is missing.");
            GameManager.Instance.RestartLevel();
            return;
        }

        if (HasRevived() && LimitRevive)
        {
            GameManager.Instance.RestartLevel();
        }
        else
        {
            revivePanel.SetActive(true);
        }
    }

    public void RevivePlayer()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogWarning("⚠️ No internet connection – skipping ads, reviving directly.");
            OnReviveConfirmed();
            
            return;
        }
        revivePanel.SetActive(false);
        void HandleRevive(bool success)
        {
            if (success) OnReviveConfirmed();
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
        if(Boots_Level.Instance != null)
        {
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
        }
        else
        {
            HandleRevive(true);
        }

        
    }

    public void SkipRevive()
    {
        revivePanel.SetActive(false);
        RestartLevel();
    }

    #endregion



}
