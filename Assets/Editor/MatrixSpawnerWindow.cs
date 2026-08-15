using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class MatrixSpawnerWindow : EditorWindow
{
    [System.Serializable]
    public class MatrixData
    {
        public int width;
        public int height;
        public int max_jump_height;
        public List<List<int>> matrix;
    }

    private TextAsset jsonFile;
    private GameObject prefab;
    private Transform parent;

    private float cellSize = 1f;
    private int spawnValue = 1;

    private Vector2 originOffset = Vector2.zero;
    private bool centerMap = true;
    private bool rotate90 = true;

    [MenuItem("Tools/Matrix Spawner")]
    static void Open()
    {
        GetWindow<MatrixSpawnerWindow>("Matrix Spawner");
    }

    void OnGUI()
    {
        GUILayout.Label("Matrix Spawner", EditorStyles.boldLabel);
        centerMap = EditorGUILayout.Toggle("Center Map", centerMap);
        rotate90 = EditorGUILayout.Toggle("Rotate 90°", rotate90);

        originOffset = EditorGUILayout.Vector2Field("Offset", originOffset);

        jsonFile = (TextAsset)EditorGUILayout.ObjectField(
            "JSON File",
            jsonFile,
            typeof(TextAsset),
            false);

        prefab = (GameObject)EditorGUILayout.ObjectField(
            "Prefab",
            prefab,
            typeof(GameObject),
            false);

        parent = (Transform)EditorGUILayout.ObjectField(
            "Parent",
            parent,
            typeof(Transform),
            true);

        cellSize = EditorGUILayout.FloatField("Cell Size", cellSize);

        spawnValue = EditorGUILayout.IntField("Spawn Value", spawnValue);

        GUILayout.Space(10);

        if (GUILayout.Button("Generate"))
        {
            Generate();
        }

        if (GUILayout.Button("Clear Children"))
        {
            ClearChildren();
        }
    }

    void Generate()
    {
        if (jsonFile == null || prefab == null || parent == null)
        {
            Debug.LogError("Missing references.");
            return;
        }

        MatrixData data = JsonConvert.DeserializeObject<MatrixData>(jsonFile.text);

        Undo.RegisterFullObjectHierarchyUndo(parent.gameObject, "Generate Matrix");

        int rows = data.matrix.Count;
        int cols = data.matrix[0].Count;

        float startX = 0;
        float startY = 0;

        if (centerMap)
        {
            startX = -(cols - 1) * cellSize * 0.5f;
            startY = (rows - 1) * cellSize * 0.5f;
        }

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (data.matrix[y][x] != spawnValue)
                    continue;

                GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

                Undo.RegisterCreatedObjectUndo(obj, "Spawn");

                obj.transform.SetParent(parent, false);

                Vector3 pos = new Vector3(
                    startX + x * cellSize + originOffset.x,
                    startY - y * cellSize + originOffset.y,
                    0
                );

                if (rotate90)
                {
                    pos = new Vector3(-pos.y, pos.x, 0);
                }

                obj.transform.localPosition = pos;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;
            }
        }

        Debug.Log("Generate Complete");
    }

    void ClearChildren()
    {
        if (parent == null)
            return;

        while (parent.childCount > 0)
        {
            Undo.DestroyObjectImmediate(parent.GetChild(0).gameObject);
        }
    }
}