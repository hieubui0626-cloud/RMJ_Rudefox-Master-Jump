using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LevelNode : MonoBehaviour
{
    public SceneList sceneToLoad;

    [Tooltip("Các level phải hoàn thành để mở khóa level này")]
    public List<SceneList> requiredCompletedLevels;

    public bool isCompleted = false;
    public bool isUnlocked = false;

    public Material Lock_Material;
    public Material Unlocked_Material;
    public Material Completed_Material;

    [Header("UI")]
    public TextMeshPro timeText; // Gán TextMeshPro 3D dưới node

    private Camera mainCam;
    private Animator levelAnimator;
    private Collider myCollider;

    [Header("Custom Render Objects")]
    public GameObject objectA; // Gán trong Inspector
    public GameObject objectB; // Gán trong Inspector

    void Awake()
    {
        mainCam = Camera.main;
        levelAnimator = GetComponent<Animator>();
        myCollider = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        LoadBestTime();
    }

    void Update()
    {
        if (!isUnlocked && !isCompleted) return;

        if (InputManager.IsInputDown())
        {
            Vector3 inputPos = InputManager.GetInputPosition();
            Ray ray = mainCam.ScreenPointToRay(inputPos);

            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider == myCollider)
            {
                OnSelect();
            }
        }
    }

    public void TryUnlock(List<SceneList> completedLevels)
    {
        if (isUnlocked || isCompleted) return;

        // Level đầu tiên luôn unlock nếu không có yêu cầu
        if (requiredCompletedLevels == null || requiredCompletedLevels.Count == 0)
        {
            isUnlocked = true;
        }
        else
        {
            // Chỉ cần 1 trong các level chỉ định đã hoàn thành là mở
            bool anyMet = requiredCompletedLevels.Any(id => completedLevels.Contains(id));
            if (anyMet) isUnlocked = true;
        }
    }

    

    public void UpdateVisualState()
    {
        if (levelAnimator != null)
        {
            levelAnimator.SetBool("isUnlocked", isUnlocked);
            levelAnimator.SetBool("isCompleted", isCompleted);
        }

        Material targetMat = Lock_Material;
        if (isCompleted)
        {
            targetMat = Completed_Material;
        }
        else if (isUnlocked)
        {
            targetMat = Unlocked_Material;
        }

        // Gán cho objectA
        if (objectA != null)
        {
            Renderer rendA = objectA.GetComponent<Renderer>();
            if (rendA != null) rendA.material = targetMat;
        }

        // Gán cho objectB
        if (objectB != null)
        {
            Renderer rendB = objectB.GetComponent<Renderer>();
            if (rendB != null) rendB.material = targetMat;
        }
    }

    private void OnSelect()
    {
        if (isUnlocked || isCompleted)
        {
            LevelTransition.Instance.EndTransition();
            StartCoroutine(WailTime());
        }
    }

    IEnumerator WailTime()
    {
        yield return new WaitForSeconds(1f);
        //SceneManager.LoadScene(sceneToLoad.ToString());
        string address = sceneToLoad.ToString();
        Addressables.LoadSceneAsync(address, LoadSceneMode.Single, true);
    }

    /*
    private void LoadBestTime()
    {
        string key = "BestTime_" + sceneToLoad.ToString();
        if (PlayerPrefs.HasKey(key) && timeText != null)
        {
            float bestTime = PlayerPrefs.GetFloat(key);
            int minutes = Mathf.FloorToInt(bestTime / 60);
            float seconds = bestTime % 60;
            timeText.text = $"{minutes:00}:{seconds:00.00}";
        }
        else if (timeText != null)
        {
            timeText.text = "--:--.--";
        }
    }
    */
    private void LoadBestTime()
    {
        FirebaseManager.Instance.LoadBestTime(sceneToLoad.ToString(), (bestTime) =>
        {
            if (bestTime >= 0 && timeText != null)
            {
                int minutes = Mathf.FloorToInt(bestTime / 60);
                float seconds = bestTime % 60;
                timeText.text = $"{minutes:00}:{seconds:00.00}";
            }
            else if (timeText != null)
            {
                timeText.text = "--:--.--";
            }
        });
    }

}
