using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class LevelMapManager : MonoBehaviour
{
    public static LevelMapManager Instance { get; private set; }
    



    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Giữ duy nhất 1 instance
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Giữ xuyên scene
        
        
        
    }


    /*
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
    */
    public static void MarkLevelComplete(SceneList scene)
    {
        FirebaseManager.Instance.MarkLevelComplete(scene.ToString());
    }

    // Lấy danh sách level hoàn thành từ Firebase
    public void LoadCompletedLevels(System.Action<List<SceneList>> callback)
    {
        FirebaseManager.Instance.LoadCompletedLevels((list) =>
        {
            List<SceneList> completed = new List<SceneList>();
            foreach (string s in list)
            {
                if (System.Enum.TryParse(s, out SceneList parsed))
                    completed.Add(parsed);
            }
            callback(completed);
        });
    }

}
