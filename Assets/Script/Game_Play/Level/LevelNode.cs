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


    [Header("Material States")]
    public Material lockedMaterial;
    public Material unlockedMaterial;
    public Material completedMaterial;
    private Renderer rend;
    private Camera mainCam;

    Animator levelAnimatior;



    void Awake()
    {
        rend = GetComponent<Renderer>();
        mainCam = Camera.main;
        levelAnimatior = GetComponent<Animator>();

    }

    void Update()
    {
        
        

        

        Vector3 inputPos = InputManager.GetInputPosition();
        Ray ray = mainCam.ScreenPointToRay(inputPos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            


            if (hit.collider == GetComponent<Collider>())
            {
                if (InputManager.IsInputDown())
                {
                    Debug.Log("chon Level tiếp theo");
                    OnSelect();

                }
                
            }
        }

        if (!isUnlocked) return;


    }

    public void TryUnlock(List<SceneList> completedLevels)
    {
        if (isUnlocked || isCompleted)
            return;

        if (requiredCompletedLevels == null || requiredCompletedLevels.Count == 0)
        {
            //isUnlocked = (sceneToLoad == SceneList.W_1_1); // Coi Level_1 là mặc định mở
        }
        else
        {
            bool allMet = requiredCompletedLevels.All(id => completedLevels.Contains(id));
            if (allMet)
            {
                

                isUnlocked = true;
                UpdateVisualState();
            }
        }

        UpdateVisualState();
    }

    public void UpdateVisualState()
    {

        if (levelAnimatior != null)
        {
            Debug.Log("unlock");

            levelAnimatior.SetBool("isUnlocked", isUnlocked);
            levelAnimatior.SetBool("isCompleted", isCompleted);
        }

        // TODO: Cập nhật màu sắc / emission theo trạng thái isUnlocked, isCompleted
    }

    public void OnSelect()
    {
        if (!isUnlocked) return;

        Debug.Log("da chon Level tiếp theo");

        SceneManager.LoadScene(sceneToLoad.ToString());
    }
}
