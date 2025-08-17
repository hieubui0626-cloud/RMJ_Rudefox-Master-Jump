using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class LevelMapManager : MonoBehaviour
{
    public static LevelMapManager Instance { get; private set; }
    public SceneList sceneToLoad;



    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Giữ duy nhất 1 instance
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Giữ xuyên scene
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name == "Boot_Scene")
        {
            
            SceneManager.LoadScene(sceneToLoad.ToString());
        }
        
    }

   

    /// <summary>
    /// Đánh dấu level đã hoàn thành
    /// </summary>
    public static void MarkLevelComplete(SceneList scene)
    {
        string key = $"Level_{scene}_Completed";
        if (PlayerPrefs.GetInt(key, 0) != 1)
        {
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Lấy danh sách các level đã hoàn thành
    /// </summary>
    public List<SceneList> LoadCompletedLevels()
    {
        var completed = new List<SceneList>();
        foreach (SceneList scene in System.Enum.GetValues(typeof(SceneList)))
        {
            string key = $"Level_{scene}_Completed";
            if (PlayerPrefs.GetInt(key, 0) == 1)
            {
                completed.Add(scene);
            }
        }
        return completed;
    }
}
