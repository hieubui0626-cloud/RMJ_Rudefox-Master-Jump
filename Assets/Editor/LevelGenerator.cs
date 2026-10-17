#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// RMJ Level Generator v3 - đặt trong Assets/Script/Editor/
/// Menu: Tools > RMJ > Level Generator
///
/// v3:
///  - Ground / Trap là LIST prefab (kích thước khác nhau, đo tự động, random mỗi lần đặt)
///  - Mỗi hàng gồm nhiều CỤM (cluster); mỗi cụm gồm nhiều platform LIỀN nhau (sát mép),
///    giữa các cụm có khoảng hở để nhảy lên trên
///  - Khoảng cách giữa các hàng tính theo MÉP (chồng tối đa / hở tối đa) thay vì theo tâm
///  - Mặt trên (top) của mọi platform trong 1 hàng luôn bằng nhau dù độ dày prefab khác nhau
/// </summary>
public class RMJLevelGenerator : EditorWindow
{
    enum Mode { Campaign, EndlessRoom }
    enum Direction { Vertical, Horizontal }

    class Variant { public GameObject prefab; public float w, h; public Vector3 centerOffset; }
    class Plat { public float main, cross; public bool isPath; public Variant v; }

    // ---- General ----
    Mode mode = Mode.Campaign;
    Direction direction = Direction.Vertical;
    string levelName = "W1_1";
    bool randomSeed = true;
    int seed = 1234;

    // ---- Prefab slots ----
    [SerializeField] List<GameObject> groundPrefabs = new List<GameObject>();
    [SerializeField] List<GameObject> trapPrefabs = new List<GameObject>();
    [SerializeField] GameObject tokenPrefab, goalPrefab, playerPrefab;

    // ---- Fallback (khi list Ground trống -> primitive) ----
    Vector2 fallbackWidth = new Vector2(3f, 6f);
    float fallbackThickness = 0.6f;
    float platformDepth = 2f;

    // ---- Layout params ----
    int rowCount = 12;
    Vector2 stepRange = new Vector2(2.5f, 4f);   // khoảng cách giữa 2 hàng (trục chính)
    float maxOverlap = 1f;                       // platform path hàng trên được chồng X tối đa lên platform path hàng dưới
    float maxReach = 2f;                         // hở mép ngang tối đa (path->path giữa 2 hàng, cụm->cụm cùng hàng)
    float rowGap = 1.5f;                         // hở tối thiểu giữa 2 cụm cùng hàng (chỗ nhảy lên)
    int clustersMin = 1, clustersMax = 2;        // số cụm / hàng
    int clusterSizeMin = 1, clusterSizeMax = 3;  // số platform liền nhau / cụm
    float clusterSeam = 0f;                      // khe giữa các platform liền nhau (0 = sát mép)
    float halfWidth = 8f;                        // biên ± trục X
    float trapChance = 0.25f;
    int tokenCandidates = 8;
    int tokensEnabled = 3;
    float roomHeight = 20f;
    string roomSaveFolder = "Assets/Prefabs/Rooms";

    Vector2 scroll;
    GameObject lastRoot;
    SerializedObject so;

    // runtime state
    System.Random rng;
    List<Variant> groundVars, trapVars;
    float _ovl, _reach;
    bool _vertical;

    [MenuItem("Tools/RMJ/Level Generator")]
    static void Open() => GetWindow<RMJLevelGenerator>("RMJ Level Gen");

    void OnEnable() { so = new SerializedObject(this); }

    // =========================================================== GUI
    void OnGUI()
    {
        if (so == null) so = new SerializedObject(this);
        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.LabelField("RMJ Level Generator v3", EditorStyles.boldLabel);
        mode = (Mode)EditorGUILayout.EnumPopup("Mode", mode);
        if (mode == Mode.Campaign) direction = (Direction)EditorGUILayout.EnumPopup("Hướng level", direction);
        levelName = EditorGUILayout.TextField("Level / Room name", levelName);
        randomSeed = EditorGUILayout.Toggle("Random seed mỗi lần", randomSeed);
        using (new EditorGUI.DisabledScope(randomSeed)) seed = EditorGUILayout.IntField("Seed", seed);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Prefabs", EditorStyles.boldLabel);
        so.Update();
        EditorGUILayout.PropertyField(so.FindProperty("groundPrefabs"), new GUIContent("Ground prefabs (random)"), true);
        EditorGUILayout.PropertyField(so.FindProperty("trapPrefabs"), new GUIContent("Trap prefabs (random)"), true);
        EditorGUILayout.PropertyField(so.FindProperty("tokenPrefab"), new GUIContent("Token"));
        if (mode == Mode.Campaign)
        {
            EditorGUILayout.PropertyField(so.FindProperty("goalPrefab"), new GUIContent("Goal"));
            EditorGUILayout.PropertyField(so.FindProperty("playerPrefab"), new GUIContent("Player (optional)"));
        }
        so.ApplyModifiedProperties();
        EditorGUILayout.HelpBox(
            "Kích thước mỗi prefab Ground/Trap được đo tự động từ Renderer (không có thì dùng Collider). " +
            "Pivot nằm ở đâu cũng được. List Ground trống -> dùng cube tạm với bề rộng random trong khoảng Fallback.",
            MessageType.None);

        if (groundPrefabs == null || groundPrefabs.TrueCount() == 0)
        {
            fallbackWidth = EditorGUILayout.Vector2Field("Fallback rộng min/max", fallbackWidth);
            fallbackThickness = EditorGUILayout.FloatField("Fallback độ dày", fallbackThickness);
        }
        platformDepth = EditorGUILayout.FloatField("Depth (Z) cho primitive", platformDepth);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Layout", EditorStyles.boldLabel);
        rowCount = Mathf.Max(3, EditorGUILayout.IntField("Số hàng (row)", rowCount));
        if (mode == Mode.Campaign)
            stepRange = EditorGUILayout.Vector2Field("Step min/max (giữa 2 hàng)", stepRange);

        maxOverlap = Mathf.Max(0f, EditorGUILayout.FloatField("Chồng X tối đa (hàng trên/dưới)", maxOverlap));
        maxReach = Mathf.Max(0f, EditorGUILayout.FloatField("Hở mép ngang tối đa", maxReach));
        EditorGUILayout.HelpBox(
            "Đổi từ bản cũ (theo tâm, platform rộng W): MIN cũ → Chồng X = W − MIN;  MAX cũ → Hở mép = MAX − W.\n" +
            "Ví dụ W=4, MIN 3, MAX 5 → Chồng X 1, Hở mép 1.", MessageType.None);

        using (new EditorGUI.DisabledScope(!(mode == Mode.EndlessRoom || direction == Direction.Vertical)))
        {
            EditorGUILayout.LabelField("Cụm platform liền nhau", EditorStyles.miniBoldLabel);
            clustersMin = Mathf.Max(1, EditorGUILayout.IntField("Số cụm / hàng MIN", clustersMin));
            clustersMax = Mathf.Max(clustersMin, EditorGUILayout.IntField("Số cụm / hàng MAX", clustersMax));
            clusterSizeMin = Mathf.Max(1, EditorGUILayout.IntField("Platform / cụm MIN", clusterSizeMin));
            clusterSizeMax = Mathf.Max(clusterSizeMin, EditorGUILayout.IntField("Platform / cụm MAX", clusterSizeMax));
            clusterSeam = Mathf.Max(0f, EditorGUILayout.FloatField("Khe giữa platform liền", clusterSeam));
            rowGap = Mathf.Max(0.1f, EditorGUILayout.FloatField("Hở tối thiểu giữa 2 cụm", rowGap));
        }
        if (mode == Mode.Campaign && direction == Direction.Horizontal)
            EditorGUILayout.HelpBox("Hướng Horizontal: mỗi hàng chỉ 1 platform (không có cụm).", MessageType.Info);

        halfWidth = EditorGUILayout.FloatField("Biên ± trục X", halfWidth);
        trapChance = EditorGUILayout.Slider("Tỉ lệ Trap / platform", trapChance, 0f, 1f);
        tokenCandidates = Mathf.Max(0, EditorGUILayout.IntField("Số vị trí Token", tokenCandidates));

        if (mode == Mode.Campaign)
        {
            tokensEnabled = Mathf.Max(0, EditorGUILayout.IntField("Token bật (Token_Spawn)", tokensEnabled));
        }
        else
        {
            roomHeight = EditorGUILayout.FloatField("Room height", roomHeight);
            float step = roomHeight / rowCount;
            EditorGUILayout.HelpBox(
                $"Khoảng cách giữa 2 hàng = {step:F2}. Phải nhỏ hơn tầm nhảy tối đa của Player.\n" +
                "Hàng đầu/cuối room dùng prefab Ground HẸP NHẤT để nối room an toàn.",
                step > stepRange.y ? MessageType.Warning : MessageType.Info);
            roomSaveFolder = EditorGUILayout.TextField("Thư mục lưu prefab", roomSaveFolder);
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Generate", GUILayout.Height(30))) Generate();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear")) Clear();
        if (GUILayout.Button("Save as Prefab")) SavePrefab();
        if (GUILayout.Button("Export JSON (LayoutLoader)")) ExportJson();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
    }

    // =========================================================== Variants (đo prefab)
    List<Variant> BuildVariants(List<GameObject> list)
    {
        var res = new List<Variant>();
        if (list == null) return res;
        foreach (var p in list) if (p != null) res.Add(Measure(p));
        return res;
    }

    static Variant Measure(GameObject prefab)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;

        Bounds b = new Bounds(Vector3.zero, Vector3.zero);
        bool has = false;
        foreach (var r in go.GetComponentsInChildren<Renderer>())
        {
            if (r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer) continue;
            if (!has) { b = r.bounds; has = true; } else b.Encapsulate(r.bounds);
        }
        if (!has)
        {
            foreach (var c in go.GetComponentsInChildren<Collider>())
            {
                if (!has) { b = c.bounds; has = true; } else b.Encapsulate(c.bounds);
            }
        }
        if (!has) b = new Bounds(Vector3.zero, Vector3.one);

        var v = new Variant
        {
            prefab = prefab,
            w = Mathf.Max(0.1f, b.size.x),
            h = Mathf.Max(0.05f, b.size.y),
            centerOffset = b.center - go.transform.position
        };
        DestroyImmediate(go);
        return v;
    }

    Variant GetGround(bool narrowest)
    {
        if (groundVars.Count > 0)
        {
            if (!narrowest) return groundVars[rng.Next(0, groundVars.Count)];
            var best = groundVars[0];
            foreach (var v in groundVars) if (v.w < best.w) best = v;
            return best;
        }
        float w = narrowest ? fallbackWidth.x : R(fallbackWidth.x, Mathf.Max(fallbackWidth.x, fallbackWidth.y));
        return new Variant { prefab = null, w = w, h = fallbackThickness };
    }

    Variant GetTrap()
    {
        if (trapVars.Count > 0) return trapVars[rng.Next(0, trapVars.Count)];
        return new Variant { prefab = null, w = 0.8f, h = 0.8f };
    }

    // =========================================================== Layout helpers
    float R(float a, float b) => (float)(a + (b - a) * rng.NextDouble());

    static float Overlap(Plat a, Plat b)
    {
        return Mathf.Min(a.cross + a.v.w * 0.5f, b.cross + b.v.w * 0.5f)
             - Mathf.Max(a.cross - a.v.w * 0.5f, b.cross - b.v.w * 0.5f);
    }

    static float EdgeGap(float c1, float w1, float c2, float w2)
    {
        return Mathf.Abs(c1 - c2) - (w1 + w2) * 0.5f;
    }

    // X của platform path hàng mới, tính theo mép so với path hàng trước
    float PickPathX(Plat prev, float w, bool hasTarget, float tCross, float tW)
    {
        for (int k = 0; k < 150; k++)
        {
            float g = R(-_ovl, _reach);
            float maxOv = Mathf.Min(prev.v.w, w) * 0.9f;
            if (-g > maxOv) g = -maxOv;

            float dist = (prev.v.w + w) * 0.5f + g;
            float x = prev.cross + (rng.NextDouble() < 0.5 ? -1f : 1f) * dist;

            if (Mathf.Abs(x) + w * 0.5f > halfWidth) continue;
            if (hasTarget)
            {
                float eg = EdgeGap(x, w, tCross, tW);
                if (eg < -_ovl || eg > _reach) continue;
            }
            return x;
        }
        float dir = prev.cross > 0 ? -1f : 1f;
        float fx = prev.cross + dir * ((prev.v.w + w) * 0.5f + (_reach - _ovl) * 0.5f);
        return Mathf.Clamp(fx, -halfWidth + w * 0.5f, halfWidth - w * 0.5f);
    }

    float PickHorizontalCross(float prev)
    {
        float range = _ovl + _reach + 1f;
        return Mathf.Clamp(prev + R(-range, range), -halfWidth, halfWidth);
    }

    // Gắn thêm 1 platform sát mép cụm (trái hoặc phải)
    bool TryAttach(List<Plat> cluster, List<Plat> row, Plat prevPath, float main)
    {
        for (int k = 0; k < 25; k++)
        {
            var v = GetGround(false);
            float lo = float.MaxValue, hi = float.MinValue;
            foreach (var p in cluster)
            {
                lo = Mathf.Min(lo, p.cross - p.v.w * 0.5f);
                hi = Mathf.Max(hi, p.cross + p.v.w * 0.5f);
            }
            bool right = rng.NextDouble() < 0.5;
            float cx = right ? hi + clusterSeam + v.w * 0.5f : lo - clusterSeam - v.w * 0.5f;
            if (Mathf.Abs(cx) + v.w * 0.5f > halfWidth) continue;

            var cand = new Plat { main = main, cross = cx, v = v };
            if (prevPath != null && Overlap(cand, prevPath) > _ovl) continue;

            bool clash = false;
            foreach (var p in row)
            {
                if (cluster.Contains(p)) continue;
                if (EdgeGap(cand.cross, v.w, p.cross, p.v.w) < rowGap) { clash = true; break; }
            }
            if (clash) continue;

            cluster.Add(cand); row.Add(cand);
            return true;
        }
        return false;
    }

    // Tạo cụm mới nằm ngoài phạm vi các cụm hiện có, cách 1 khoảng hở nhảy được
    Plat TrySeedCluster(List<Plat> row, Plat prevPath, float main)
    {
        for (int k = 0; k < 30; k++)
        {
            var v = GetGround(false);
            float lo = float.MaxValue, hi = float.MinValue;
            foreach (var p in row)
            {
                lo = Mathf.Min(lo, p.cross - p.v.w * 0.5f);
                hi = Mathf.Max(hi, p.cross + p.v.w * 0.5f);
            }
            float gap = R(rowGap, Mathf.Max(rowGap, _reach));
            bool right = rng.NextDouble() < 0.5;
            float cx = right ? hi + gap + v.w * 0.5f : lo - gap - v.w * 0.5f;
            if (Mathf.Abs(cx) + v.w * 0.5f > halfWidth)
            {
                right = !right;
                cx = right ? hi + gap + v.w * 0.5f : lo - gap - v.w * 0.5f;
                if (Mathf.Abs(cx) + v.w * 0.5f > halfWidth) continue;
            }
            var cand = new Plat { main = main, cross = cx, v = v };
            if (prevPath != null && Overlap(cand, prevPath) > _ovl) continue;

            row.Add(cand);
            return cand;
        }
        return null;
    }

    void BuildRowExtras(List<Plat> row, Plat path, Plat prevPath, float main)
    {
        int clusters = rng.Next(clustersMin, clustersMax + 1);

        var cl = new List<Plat> { path };
        int target = rng.Next(clusterSizeMin, clusterSizeMax + 1);
        for (int s = 1; s < target; s++) if (!TryAttach(cl, row, prevPath, main)) break;

        for (int c = 1; c < clusters; c++)
        {
            var seedP = TrySeedCluster(row, prevPath, main);
            if (seedP == null) break;
            var cl2 = new List<Plat> { seedP };
            int t2 = rng.Next(clusterSizeMin, clusterSizeMax + 1);
            for (int s = 1; s < t2; s++) if (!TryAttach(cl2, row, prevPath, main)) break;
        }
    }

    Vector3 TopPos(Plat p) => _vertical ? new Vector3(p.cross, p.main, 0) : new Vector3(p.main, p.cross, 0);

    // =========================================================== Generate
    void Generate()
    {
        EnsureTag("Ground"); EnsureTag("Trap"); EnsureTag("Goal"); EnsureTag("Token");
        Clear();

        if (randomSeed) seed = Random.Range(0, 99999);
        rng = new System.Random(seed);

        groundVars = BuildVariants(groundPrefabs);
        trapVars = BuildVariants(trapPrefabs);

        bool endless = mode == Mode.EndlessRoom;
        _vertical = endless || direction == Direction.Vertical;
        bool multi = _vertical;
        _ovl = Mathf.Max(0f, maxOverlap);
        _reach = Mathf.Max(0.1f, maxReach);
        int n = Mathf.Max(3, rowCount);

        var root = new GameObject(levelName);
        Undo.RegisterCreatedObjectUndo(root, "Generate RMJ Level");
        lastRoot = root;

        Transform groundT = Child(root, "Ground");
        Transform trapT = Child(root, "Traps");
        Transform tokenT = Child(root, "Layout_Token"); // tên bắt buộc: RoomManager dùng Find("Layout_Token")

        // ---------- 1. Layout ----------
        Variant edgeV = GetGround(true);                       // endless: platform đầu/cuối room luôn cùng kích thước
        float g = (_reach - _ovl) * 0.5f;
        float D = edgeV.w + g;                                 // khoảng cách tâm giữa platform cuối room N và đầu room N+1

        var rows = new List<List<Plat>>();
        Plat prevPath = null;
        float main = 0f;

        for (int i = 0; i < n; i++)
        {
            if (endless) main = (i + 0.5f) * roomHeight / n;
            else if (i > 0) main += R(stepRange.x, stepRange.y);

            bool edge = (i == 0 || i == n - 1);
            Variant pv = (endless && edge) ? edgeV : GetGround(false);

            float x;
            if (i == 0) x = endless ? -D * 0.5f : 0f;
            else if (endless && i == n - 1) x = D * 0.5f;
            else if (!_vertical) x = PickHorizontalCross(prevPath.cross);
            else x = PickPathX(prevPath, pv.w, endless && i == n - 2, D * 0.5f, edgeV.w);

            var path = new Plat { main = main, cross = x, isPath = true, v = pv };
            var row = new List<Plat> { path };
            if (multi && !edge) BuildRowExtras(row, path, prevPath, main);

            rows.Add(row);
            prevPath = path;
        }

        // ---------- 2. Instantiate ----------
        var tops = new List<Vector3>();
        int total = 0;
        for (int i = 0; i < n; i++)
        {
            bool edge = (i == 0 || i == n - 1);
            for (int j = 0; j < rows[i].Count; j++)
            {
                var p = rows[i][j];
                Vector3 top = TopPos(p);
                tops.Add(top); total++;

                // top-aligned: mặt trên platform = top
                Vector3 center = top + new Vector3(0, -p.v.h * 0.5f, 0);
                var go = Place(p.v, "Ground", $"Ground_{i:00}_{j}", center, groundT);
                if (p.v.prefab == null) AddGroundTrigger(go);

                // Trap
                if (!edge && (float)rng.NextDouble() < trapChance)
                {
                    var tv = GetTrap();
                    if (p.v.w >= tv.w + 1.5f)
                    {
                        float side;
                        if (!_vertical) side = 1f;
                        else if (p.isPath) side = rows[i + 1][0].cross - p.cross > 0 ? -1f : 1f; // xa platform kế tiếp
                        else side = rng.NextDouble() < 0.5 ? -1f : 1f;

                        Vector3 tc = top + new Vector3(side * p.v.w * 0.3f, tv.h * 0.5f, 0);
                        Place(tv, "Trap", $"Trap_{i:00}_{j}", tc, trapT);
                    }
                }
            }
        }

        // ---- Token candidates ----
        for (int t = 0; t < tokenCandidates && tops.Count > 0; t++)
        {
            int idx = rng.Next(0, tops.Count);
            // tìm platform tương ứng để lấy bề rộng
            float w = WidthAtIndex(rows, idx);
            Vector3 pos = tops[idx] + new Vector3(R(-w * 0.5f, w * 0.5f), R(1.5f, 2.5f), 0);
            Make(tokenPrefab, PrimitiveType.Sphere, "Token", $"Token_{t:00}", pos, Vector3.one * 0.6f, tokenT, true);
        }

        // ---- Campaign: Goal / Player / Token_Spawn ----
        if (!endless)
        {
            Vector3 gp = TopPos(rows[n - 1][0]) + new Vector3(0, 0.9f, 0);
            Make(goalPrefab, PrimitiveType.Cube, "Goal", "Goal", gp, new Vector3(1f, 1.5f, platformDepth), root.transform, true);

            if (playerPrefab != null)
            {
                var pl = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                pl.transform.position = TopPos(rows[0][0]) + new Vector3(0, 1f, 0);
                pl.transform.SetParent(root.transform, true);
            }

            var ts = root.AddComponent<Token_Spawn>();
            var tso = new SerializedObject(ts);
            tso.FindProperty("layoutToken").objectReferenceValue = tokenT.gameObject;
            tso.FindProperty("numberToEnable").intValue = tokensEnabled;
            tso.ApplyModifiedProperties();
        }

        Selection.activeGameObject = root;
        Debug.Log($"[RMJ Gen] '{levelName}' seed={seed}, rows={n}, platforms={total}, mode={mode}");
    }

    static float WidthAtIndex(List<List<Plat>> rows, int idx)
    {
        int c = 0;
        foreach (var row in rows)
            foreach (var p in row)
            {
                if (c == idx) return p.v.w;
                c++;
            }
        return 2f;
    }

    // =========================================================== Helpers
    static Transform Child(GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        return go.transform;
    }

    // Đặt 1 Variant (đã đo kích thước) sao cho TÂM bounds nằm tại center
    GameObject Place(Variant v, string tag, string name, Vector3 center, Transform parent)
    {
        GameObject go;
        if (v.prefab != null)
        {
            go = (GameObject)PrefabUtility.InstantiatePrefab(v.prefab, parent);
            go.transform.position = center - v.centerOffset;
        }
        else
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.SetParent(parent, false);
            go.transform.position = center;
            go.transform.localScale = new Vector3(v.w, v.h, platformDepth);
            go.tag = tag;
        }
        go.name = name;
        return go;
    }

    static GameObject Make(GameObject prefab, PrimitiveType prim, string tag, string name,
                           Vector3 pos, Vector3 scale, Transform parent, bool trigger)
    {
        GameObject go;
        if (prefab != null)
        {
            go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.transform.position = pos;
        }
        else
        {
            go = GameObject.CreatePrimitive(prim);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.tag = tag;
            if (trigger) go.GetComponent<Collider>().isTrigger = true;
        }
        go.name = name;
        return go;
    }

    // PlayerController.OnTriggerStay kiểm tra tag "Ground" qua trigger -> thêm trigger mỏng phía trên platform primitive
    static void AddGroundTrigger(GameObject ground)
    {
        var t = new GameObject("GroundTrigger");
        t.transform.SetParent(ground.transform, false);
        t.transform.localPosition = new Vector3(0, 0.5f, 0);
        var bc = t.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.size = new Vector3(1f, 1.5f, 1f);
        t.tag = "Ground";
    }

    static void EnsureTag(string tag)
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (assets == null || assets.Length == 0) return;
        var tm = new SerializedObject(assets[0]);
        var tags = tm.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tm.ApplyModifiedProperties();
    }

    void Clear()
    {
        if (lastRoot == null) lastRoot = GameObject.Find(levelName);
        if (lastRoot != null) Undo.DestroyObjectImmediate(lastRoot);
        lastRoot = null;
    }

    // =========================================================== Save / Export
    void SavePrefab()
    {
        if (lastRoot == null) { Debug.LogWarning("[RMJ Gen] Chưa có level để lưu. Bấm Generate trước."); return; }
        string folder = mode == Mode.EndlessRoom ? roomSaveFolder : "Assets/Prefabs/Levels";
        Directory.CreateDirectory(folder);
        string path = $"{folder}/{levelName}.prefab";
        PrefabUtility.SaveAsPrefabAssetAndConnect(lastRoot, path, InteractionMode.UserAction);
        AssetDatabase.Refresh();
        Debug.Log("[RMJ Gen] Saved prefab: " + path);
    }

    void ExportJson()
    {
        if (lastRoot == null) { Debug.LogWarning("[RMJ Gen] Chưa có level để export."); return; }

        var data = new LayoutData();
        foreach (var t in lastRoot.GetComponentsInChildren<Transform>(true))
        {
            var go = t.gameObject;
            if (go == lastRoot || !PrefabUtility.IsOutermostPrefabInstanceRoot(go)) continue;
            var src = PrefabUtility.GetCorrespondingObjectFromSource(go);
            data.objects.Add(new LayoutObjectData
            {
                prefabId = src.name, // phải trùng Addressable address của prefab
                position = t.position,
                rotation = t.rotation,
                scale = t.lossyScale
            });
        }

        if (data.objects.Count == 0)
        {
            Debug.LogWarning("[RMJ Gen] Không có prefab instance nào (primitive tạm không export được). Hãy gán prefab rồi Generate lại.");
            return;
        }

        string dir = Path.Combine(Application.streamingAssetsPath, "Layouts");
        Directory.CreateDirectory(dir);
        string file = Path.Combine(dir, levelName + "_Layout.json");
        File.WriteAllText(file, JsonUtility.ToJson(data, true));
        AssetDatabase.Refresh();
        Debug.Log($"[RMJ Gen] Exported {data.objects.Count} objects -> {file}");
    }
}

static class RMJListExt
{
    public static int TrueCount(this List<GameObject> l)
    {
        int c = 0;
        if (l != null) foreach (var o in l) if (o != null) c++;
        return c;
    }
}
#endif
