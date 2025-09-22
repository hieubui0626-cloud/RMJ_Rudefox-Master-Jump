using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// ===== DATA STRUCT =====
[System.Serializable]
public class LayoutObjectData
{
    public string prefabId;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public GameObject cloneprefab;
}

[System.Serializable]
public class LayoutData
{
    public List<LayoutObjectData> objects = new List<LayoutObjectData>();
}

/// ===== EDITOR TOOL =====
public class LayoutExporterWindow : EditorWindow
{
    private GameObject layoutRoot;
    private string exportPath = "Assets/StreamingAssets/Layouts/";
    private string fileName;
    private string previewJson = "";

    [MenuItem("Tools/Layout Exporter")]
    public static void ShowWindow()
    {
        GetWindow<LayoutExporterWindow>("Layout Exporter");
    }

    private void OnEnable()
    {
        // Đăng ký auto export khi Save Scene
        EditorSceneManager.sceneSaved += OnSceneSaved;
    }

    private void OnDisable()
    {
        // Hủy đăng ký khi đóng window
        EditorSceneManager.sceneSaved -= OnSceneSaved;
    }

    private void OnSceneSaved(Scene scene)
    {
        // Auto export khi Save Scene
        if (layoutRoot != null)
        {
            Debug.Log($"💾 Scene {scene.name} saved → auto export layout JSON...");
            ExportLayout(auto: true);
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Layout Root", EditorStyles.boldLabel);
        layoutRoot = (GameObject)EditorGUILayout.ObjectField("Root Object", layoutRoot, typeof(GameObject), true);

        GUILayout.Space(10);
        GUILayout.Label("Export Settings", EditorStyles.boldLabel);
        exportPath = EditorGUILayout.TextField("Export Path", exportPath);


        if(layoutRoot != null)
        {
            string objectName = layoutRoot.name;
            fileName = $"{objectName}.json";
        }

        
        
        
        
        fileName = EditorGUILayout.TextField("File Name", fileName);

        GUILayout.Space(15);

        if (GUILayout.Button("Export Layout to JSON", GUILayout.Height(30)))
        {
            ExportLayout();
        }

        GUILayout.Space(15);
        GUILayout.Label("Preview JSON", EditorStyles.boldLabel);
        EditorGUILayout.TextArea(previewJson, GUILayout.Height(200));
    }

    private void ExportLayout(bool auto = false)
    {
        if (layoutRoot == null)
        {
            if (!auto)
            {
                Debug.LogError("❌ Chưa chọn LayoutRoot!");
                EditorUtility.DisplayDialog("Export Failed", "Bạn chưa chọn LayoutRoot!", "OK");
            }
            return;
        }

        LayoutData data = new LayoutData();

        foreach (Transform child in layoutRoot.transform)
        {

            string prefabName = child.name.Replace("(Clone)", "");

#if UNITY_EDITOR
            // Lấy prefab gốc nếu child là instance prefab
            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
            if (prefab != null)
            {
                prefabName = prefab.name;
            }
#endif

            LayoutObjectData entry = new LayoutObjectData
            {
                prefabId = prefabName,
                position = child.position,
                rotation = child.rotation,
                scale = child.localScale
            };

            data.objects.Add(entry);
        }

        string json = JsonUtility.ToJson(data, true);
        previewJson = json;

        if (!Directory.Exists(exportPath))
            Directory.CreateDirectory(exportPath);

        string filePath = Path.Combine(exportPath, fileName);
        File.WriteAllText(filePath, json);

        if (!auto)
        {
            EditorUtility.DisplayDialog("Export Success", $"Đã export layout thành công:\n{filePath}", "OK");
        }

        Debug.Log($"✅ Exported Layout: {filePath}");
        AssetDatabase.Refresh();
    }
}
