using UnityEditor;
using UnityEngine;

public class PlayerPrefsEditorTools
{
    [MenuItem("Tools/PlayerPrefs/Delete All %#d")] // Ctrl+Shift+D
    public static void DeleteAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("Đã xóa toàn bộ PlayerPrefs!");
    }

    [MenuItem("Tools/PlayerPrefs/Show All")]
    public static void ShowAllPlayerPrefs()
    {
        int value = PlayerPrefs.GetInt("Level_Level1_Completed", 0);
        Debug.Log("Level1 Completed: " + value);
    }
}