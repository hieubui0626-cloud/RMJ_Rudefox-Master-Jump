#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class SceneEnumGenerator
{
    [MenuItem("Tools/Generate Scene Enum")]
    public static void GenerateEnum()
    {
        string path = "Assets/Script/Game_Manager/Scene/SceneList.cs";

        string[] sceneNames = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => Path.GetFileNameWithoutExtension(s.path))
            .ToArray();

        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.WriteLine("public enum SceneList");
            writer.WriteLine("{");

            foreach (string name in sceneNames)
            {
                string safeName = name.Replace(" ", "_").Replace("-", "_");
                writer.WriteLine($"    {safeName},");
            }

            writer.WriteLine("}");
        }

        AssetDatabase.Refresh();
        Debug.Log("Scene enum generated.");
    }
}
#endif