#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

public class SceneEnumGenerator : EditorWindow
{
    private DefaultAsset folder; // chọn folder chứa scene
    private string outputPath = "Assets/Script/Game_Manager/Scene/SceneList.cs";

    [MenuItem("Tools/Scene Enum Generator")]
    public static void ShowWindow()
    {
        GetWindow<SceneEnumGenerator>("Scene Enum Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Scene Enum & Addressables Generator", EditorStyles.boldLabel);

        // chọn folder chứa scene
        folder = (DefaultAsset)EditorGUILayout.ObjectField("Scene Folder", folder, typeof(DefaultAsset), false);

        // đường dẫn file enum
        outputPath = EditorGUILayout.TextField("Enum Output Path", outputPath);

        GUILayout.Space(10);

        if (GUILayout.Button("🔄 Replace Enum (Ghi đè toàn bộ)"))
        {
            if (ValidateFolder())
            {
                string folderPath = AssetDatabase.GetAssetPath(folder);
                GenerateSceneEnum(folderPath, outputPath, replace: true);
            }
        }

        if (GUILayout.Button("➕ Add To Enum (Chỉ thêm mới)"))
        {
            if (ValidateFolder())
            {
                string folderPath = AssetDatabase.GetAssetPath(folder);
                GenerateSceneEnum(folderPath, outputPath, replace: false);
            }
        }
    }

    private bool ValidateFolder()
    {
        if (folder == null)
        {
            EditorUtility.DisplayDialog("Error", "Bạn cần chọn folder chứa scene!", "OK");
            return false;
        }
        return true;
    }

    private void GenerateSceneEnum(string folderPath, string outputPath, bool replace)
    {
        // 🔹 Quét tất cả scene trong folder
        string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { folderPath });
        var sceneNames = guids
            .Select(guid => Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid)))
            .Distinct()
            .ToList();

        if (sceneNames.Count == 0)
        {
            EditorUtility.DisplayDialog("Warning", "Không tìm thấy scene nào trong folder đã chọn!", "OK");
            return;
        }

        List<string> finalNames = new List<string>();

        if (!replace && File.Exists(outputPath))
        {
            // 🔹 Giữ enum cũ, chỉ thêm scene mới
            var oldLines = File.ReadAllLines(outputPath)
                .Where(line => line.Trim().EndsWith(","))
                .Select(line => line.Trim().TrimEnd(','))
                .ToList();

            finalNames.AddRange(oldLines);

            foreach (var name in sceneNames)
            {
                string safeName = name.Replace(" ", "_").Replace("-", "_");
                if (!finalNames.Contains(safeName))
                {
                    finalNames.Add(safeName);
                }
            }
        }
        else
        {
            // 🔹 Replace toàn bộ
            foreach (var name in sceneNames)
            {
                string safeName = name.Replace(" ", "_").Replace("-", "_");
                finalNames.Add(safeName);
            }
        }

        // 🔹 Sinh code enum
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("public enum SceneList");
        sb.AppendLine("{");
        foreach (var n in finalNames)
        {
            sb.AppendLine($"    {n},");
        }
        sb.AppendLine("}");

        File.WriteAllText(outputPath, sb.ToString());
        AssetDatabase.Refresh();

        // 🔹 Đồng bộ với Addressables
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        foreach (var guid in guids)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);

            var entry = settings.FindAssetEntry(guid);
            if (entry == null)
            {
                var group = settings.DefaultGroup;
                entry = settings.CreateOrMoveEntry(guid, group);
            }

            entry.address = sceneName; // 🔑 Address = tên scene (match enum)
        }

        Debug.Log($"✅ SceneList enum updated with {finalNames.Count} entries.\n" +
                  $"📂 Output: {outputPath}\n" +
                  $"🔗 Addressables synced!");
    }
}
#endif
