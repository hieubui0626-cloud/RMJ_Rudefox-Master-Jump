using System.Collections.Generic;
using UnityEngine;

public class LevelNodeListManager : MonoBehaviour
{
    [Header("Danh sách các Level Node trong scene này")]
    public List<LevelNode> allLevelNodes;

    void Start()
    {
        RefreshNodeStates();
    }

    public void RefreshNodeStates()
    {
        List<SceneList> completedLevels = LevelMapManager.Instance.LoadCompletedLevels();

        foreach (var node in allLevelNodes)
        {
            node.isCompleted = completedLevels.Contains(node.sceneToLoad);
            node.TryUnlock(completedLevels);
            node.UpdateVisualState();
        }
    }
}
