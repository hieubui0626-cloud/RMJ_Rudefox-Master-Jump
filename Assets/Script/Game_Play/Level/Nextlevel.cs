using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Nextlevel : MonoBehaviour
{
    [Header("Scene kế tiếp sẽ được load")]
    public SceneList sceneToLoad;

    [Header("Thời gian giữ trong vùng thắng để load scene")]
    public float countLoadscene;
    public float MaxcountLoadscene = 2f;

    [Header("Tên Scene hiện tại (để đánh dấu đã hoàn thành)")]
    public SceneList currentScene;

    private bool hasCompleted = false;


    void OnTriggerStay(Collider other)
    {
        Debug.Log("Tải Level tiếp theo");

        if (other.CompareTag("Player"))
        {
            countLoadscene = countLoadscene + 1;
            Debug.Log("Tải Level tiếp theo");

            string key = "Level_" + currentScene.ToString() + "_Completed";
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();

            hasCompleted = true;


            



            if (countLoadscene >= MaxcountLoadscene)
            {
                GameManager.Instance.LoadNextLevel();

            }


            

            


        }
    }
    

    

    
}
