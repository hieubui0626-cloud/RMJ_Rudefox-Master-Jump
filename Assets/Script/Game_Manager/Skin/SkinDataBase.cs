using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SkinDataBase : MonoBehaviour
{
    [Header("Folder Settings")]
    [Tooltip("Đường dẫn tính từ thư mục Assets, VD: Assets/Game/Skins")]
    public string skinFolderPath = "Assets/Game/Skins";

    [Header("Skin Collection")]
    public List<SkinData> allSkins = new List<SkinData>();

    public static SkinDataBase Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Lấy SkinData theo ID
    /// </summary>
    public SkinSlotData GetSlotByID(string slotID)
    {
        if (string.IsNullOrEmpty(slotID)) return null;

        foreach (var skinTheme in allSkins)
        {
            if (skinTheme == null) continue;
            var slot = skinTheme.GetSlotByID(slotID);
            if (slot != null) return slot;
        }
        return null;
    }

    // Click chuột phải vào Script component trên Inspector để chạy
    [ContextMenu("Auto Load Skins From Folder")]
    public void LoadSkinsFromFolder()
    {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(skinFolderPath))
        {
            Debug.LogError("[SkinDataBase] Chưa nhập đường dẫn skinFolderPath!");
            return;
        }

        // Tìm tất cả asset có kiểu SkinData trong thư mục skinFolderPath
        string[] guids = AssetDatabase.FindAssets("t:SkinData", new[] { skinFolderPath });
        allSkins.Clear();

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            SkinData skin = AssetDatabase.LoadAssetAtPath<SkinData>(assetPath);

            if (skin != null)
            {
                allSkins.Add(skin);
            }
        }

        // Đánh dấu Object thay đổi để Unity lưu lại vào Scene/Prefab
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

        Debug.Log($"[SkinDataBase] Đã nạp thành công {allSkins.Count} SkinData từ đường dẫn: {skinFolderPath}");
#endif
    }
}