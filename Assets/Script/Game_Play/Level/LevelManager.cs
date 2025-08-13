using System.Collections.Generic;
using UnityEngine;

public class LevelMapManager : MonoBehaviour
{
    public List<LevelNode> allLevelNodes;

    void Start()
    {
        List<SceneList> completed = LoadCompletedLevels();

        foreach (var node in allLevelNodes)
        {
            node.isCompleted = completed.Contains(node.sceneToLoad);
            node.TryUnlock(completed);
        }
    }

    List<SceneList> LoadCompletedLevels()
    {
        List<SceneList> completed = new List<SceneList>();
        foreach (var node in allLevelNodes)
        {
            string key = "Level_" + node.sceneToLoad.ToString() + "_Completed";
            if (PlayerPrefs.GetInt(key, 0) == 1)
            {
                completed.Add(node.sceneToLoad);
            }
        }
        return completed;
    }

    public void MarkLevelComplete(SceneList scene)
    {
        PlayerPrefs.SetInt("Level_" + scene.ToString() + "_Completed", 1);
        PlayerPrefs.Save();
        Start(); // Refresh lại toàn bộ unlock
    }
}