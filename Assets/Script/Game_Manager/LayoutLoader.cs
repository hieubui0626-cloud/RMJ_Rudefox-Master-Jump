using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Networking;
using System.IO;
using System.Collections;
using System.Collections.Generic;

/// ===== DATA STRUCT =====
[System.Serializable]
public class LayoutObjectData
{
    public string prefabId;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
}

[System.Serializable]
public class LayoutData
{
    public List<LayoutObjectData> objects = new List<LayoutObjectData>();
}

/// ===== RUNTIME LOADER =====
public class LayoutLoader : MonoBehaviour
{
    [Header("Layout Settings")]
    public string jsonFileName;      // VD: "Level_01_Layout.json"
    public Transform layoutRoot;     // Empty GameObject trong scene (làm cha chứa layout)

    [Header("Editor Options")]
    public bool useEditorLayoutIfExists = true; // Cho phép dùng LayoutRoot trong Editor

    private LayoutData layoutData;

    void Start()
    {
#if UNITY_EDITOR
        // Nếu đang trong Editor và LayoutRoot đã có con → giữ nguyên để designer test
        if (useEditorLayoutIfExists && layoutRoot != null && layoutRoot.childCount > 0)
        {
            Debug.Log("🔹 Dùng LayoutRoot thật trong Editor (không load JSON).");
            return;
        }
#endif
        StartCoroutine(LoadLayoutFromJson());
    }

    private IEnumerator LoadLayoutFromJson()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Layouts", jsonFileName);
        string json = null;

#if UNITY_ANDROID && !UNITY_EDITOR
        // Android: phải đọc bằng UnityWebRequest
        UnityWebRequest www = UnityWebRequest.Get(path);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Không thể load JSON layout: " + path + " | " + www.error);
            yield break;
        }
        json = www.downloadHandler.text;
#else
        if (!File.Exists(path))
        {
            Debug.LogError("❌ Không tìm thấy JSON layout: " + path);
            yield break;
        }
        json = File.ReadAllText(path);
#endif

        layoutData = JsonUtility.FromJson<LayoutData>(json);

        if (layoutData == null || layoutData.objects == null || layoutData.objects.Count == 0)
        {
            Debug.LogWarning("⚠ JSON layout rỗng hoặc không hợp lệ.");
            yield break;
        }

        Debug.Log($"🔹 Load Layout từ JSON: {jsonFileName}, tổng object: {layoutData.objects.Count}");

        foreach (var obj in layoutData.objects)
        {
            Addressables.LoadAssetAsync<GameObject>(obj.prefabId).Completed += (AsyncOperationHandle<GameObject> handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    GameObject instance = Instantiate(
                        handle.Result,
                        obj.position,
                        obj.rotation,
                        layoutRoot
                    );
                    instance.transform.localScale = obj.scale;
                    instance.name = obj.prefabId; // đặt lại tên cho gọn
                }
                else
                {
                    Debug.LogError($"❌ Không load được prefab: {obj.prefabId}");
                }
            };
        }
    }
}
