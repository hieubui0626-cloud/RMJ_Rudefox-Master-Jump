using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class MusicEnumGenerator : EditorWindow
{
    private const string enumPath = "Assets/Script/Game_Manager/Scene/MusicTrack.cs";
    private const string musicFolder = "Assets/Sound/";

    [MenuItem("Tools/Generate Music Enum")]
    public static void GenerateMusicEnum()
    {
        if (!Directory.Exists(musicFolder))
        {
            Debug.LogError("Không tìm thấy folder: " + musicFolder);
            return;
        }

        string[] files = Directory.GetFiles(musicFolder, "*.*", SearchOption.AllDirectories);
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("public enum MusicTrack");
        sb.AppendLine("{");
        sb.AppendLine("    None,");

        foreach (var file in files)
        {
            if (file.EndsWith(".meta")) continue;

            string fileName = Path.GetFileNameWithoutExtension(file);

            // Loại bỏ ký tự không hợp lệ trong enum
            string enumName = fileName.Replace(" ", "_")
                                      .Replace("-", "_")
                                      .Replace(".", "_");

            sb.AppendLine("    " + enumName + ",");
        }

        sb.AppendLine("}");

        File.WriteAllText(enumPath, sb.ToString(), Encoding.UTF8);
        AssetDatabase.Refresh();

        Debug.Log("MusicTrack enum đã được cập nhật!");
    }
}
