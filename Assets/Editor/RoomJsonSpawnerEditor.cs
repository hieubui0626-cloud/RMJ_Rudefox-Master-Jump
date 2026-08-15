using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoomJsonSpawner))]
public class RoomJsonSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Vẽ các tham số mặc định của RoomJsonSpawner
        DrawDefaultInspector();

        RoomJsonSpawner spawner = (RoomJsonSpawner)target;

        GUILayout.Space(15);

        // Tạo nút bấm lớn trên Inspector
        if (GUILayout.Button("Generate Room từ JSON", GUILayout.Height(40)))
        {
            spawner.SpawnRoomFromJSON();
        }
    }
}