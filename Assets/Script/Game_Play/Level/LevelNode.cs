using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private Camera mainCam;
    private Animator levelAnimator;
    private Collider myCollider;

    void Awake()
    {
        mainCam = Camera.main;
        levelAnimator = GetComponent<Animator>();
        myCollider = GetComponent<Collider>();
    }
     void Stars()
    {
        
    }

    void Update()
    {
        if (!isUnlocked) return;

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
        Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
        Material targetMat = Lock_Material;
        if (isCompleted)
        {
            targetMat = Completed_Material;
        }
        else if (isUnlocked)
        {
            targetMat = Unlocked_Material;
        }

        foreach (Renderer rend in childRenderers)
        {
            rend.material = targetMat;
        }

    }

    private void OnSelect()
    {
        if (!isUnlocked) return;  // chỉ cần mở khóa là được chơi
        SceneManager.LoadScene(sceneToLoad.ToString());


    }
}
