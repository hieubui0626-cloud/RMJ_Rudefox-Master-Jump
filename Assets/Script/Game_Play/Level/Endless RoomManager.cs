using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    [System.Serializable]
    public struct Endless_RoomDifficulty_Setup
    {
        public string difficultyType;
        public int startFloor;
        public int endFloor;

        [Tooltip("Nếu bật, difficulty này áp dụng từ startFloor trở đi, không giới hạn tầng kết thúc.")]
        public bool isInfinite;

        public List<GameObject> prefabs;
        public int NumberTokenSpawn;
    }


    [Header("Spawn Token")]
    [SerializeField] private GameObject layoutToken;

    [Header("Difficulty Configurations")]
    [SerializeField] private List<Endless_RoomDifficulty_Setup> difficultySetup;
    [SerializeField] private List<GameObject> defaultRoomPrefabs; // Phòng hờ nếu không tìm thấy tầng cấu hình

    [Header("References")]
    public Transform playerTransform;
    public Transform cameraTransform;
    public Transform killZoneTransform; // Kéo GameObject KillZone vào đây

    [Header("Configuration")]
    [Min(1)]
    public float roomHeight = 20f; // Chiều cao cố định của mỗi Room

    [Header("Kill Zone")]
    [Tooltip("Khoảng đệm giữa Kill Zone và điểm cao nhất Player đã đạt được, tránh chết đột ngột.")]
    public float killZoneOffset = 60f;

    // Quản lý các room đang hoạt động trong Scene theo số tầng (Index)
    private Dictionary<int, GameObject> activeRooms = new Dictionary<int, GameObject>();

    // Object Pool chung lưu trữ theo tên Prefab để tái sử dụng tối ưu hiệu năng
    private Dictionary<string, Queue<GameObject>> roomPool = new Dictionary<string, Queue<GameObject>>();

    private int lastPlayerFloor = -1;

    // Buffer tái sử dụng để tránh cấp phát List mới mỗi lần đổi tầng (giảm GC)
    private readonly List<GameObject> _combinedPrefabsBuffer = new List<GameObject>();
    private readonly List<GameObject> _shuffleBuffer = new List<GameObject>();
    private readonly HashSet<int> _floorsToActivateBuffer = new HashSet<int>();
    private readonly List<int> _floorsToReturnBuffer = new List<int>();

    void Awake()
    {
        // Đồng bộ pattern singleton với các Manager khác trong project (tránh duplicate instance khi reload scene)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Khởi tạo tầng ban đầu (Tầng 0)
        UpdateRooms(0);
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Tính toán xem Player đang ở tầng (Floor) thứ mấy dựa trên trục Y
        int currentPlayerFloor = Mathf.FloorToInt(playerTransform.position.y / roomHeight);

        // Nếu Player đổi tầng (nhảy lên hoặc rơi xuống), cập nhật lại trạng thái các Room và Vị trí Kill Zone
        if (currentPlayerFloor != lastPlayerFloor)
        {
            lastPlayerFloor = currentPlayerFloor;
            UpdateRooms(currentPlayerFloor);
            UpdateKillZonePosition();
        }
    }

    private int GetTokenCountForFloor(int floor)
    {
        foreach (var range in difficultySetup)
        {
            if (floor >= range.startFloor && (range.endFloor == 0 || floor <= range.endFloor))
            {
                return range.NumberTokenSpawn;
            }
        }

        return 0;
    }

    private void RandomizeTokens(GameObject room, int numberToEnable)
    {
        Transform tokenLayout = room.transform.Find("Layout_Token");

        if (tokenLayout == null)
        {
            Debug.LogWarning($"⚠️ Room '{room.name}' không có child 'Layout_Token' (cấp 1) - bỏ qua random token.");
            return;
        }

        _shuffleBuffer.Clear();
        foreach (Transform child in tokenLayout)
        {
            _shuffleBuffer.Add(child.gameObject);
        }

        int totalChildren = _shuffleBuffer.Count;

        numberToEnable = Mathf.Clamp(numberToEnable, 0, totalChildren);

        // Fisher-Yates Shuffle
        for (int i = 0; i < _shuffleBuffer.Count; i++)
        {
            int randomIndex = Random.Range(i, _shuffleBuffer.Count);

            GameObject temp = _shuffleBuffer[i];
            _shuffleBuffer[i] = _shuffleBuffer[randomIndex];
            _shuffleBuffer[randomIndex] = temp;
        }

        for (int i = 0; i < _shuffleBuffer.Count; i++)
        {
            _shuffleBuffer[i].SetActive(i < numberToEnable);
        }
    }

    void UpdateRooms(int currentFloor)
    {
        // Xác định các tầng cần phải hiển thị xung quanh Player (Ít nhất 2 tầng dưới, 1 tầng trên)
        _floorsToActivateBuffer.Clear();

        _floorsToActivateBuffer.Add(currentFloor);     // Tầng hiện tại người chơi đang đứng
        _floorsToActivateBuffer.Add(currentFloor + 1); // 1 Tầng phía trên để chuẩn bị nhảy lên

        // Giữ ít nhất 1 tầng bên dưới nếu người chơi đã nhảy đủ cao
        if (currentFloor - 1 >= 0) _floorsToActivateBuffer.Add(currentFloor - 1);

        // 1. Sinh hoặc kích hoạt các Room thuộc các tầng cần thiết
        foreach (int floor in _floorsToActivateBuffer)
        {
            if (!activeRooms.ContainsKey(floor))
            {
                SpawnRoomAtFloor(floor);
            }
        }

        // 2. Thu hồi (Ẩn) các Room nằm ngoài danh sách hiển thị
        _floorsToReturnBuffer.Clear();
        foreach (var kvp in activeRooms)
        {
            if (!_floorsToActivateBuffer.Contains(kvp.Key))
            {
                _floorsToReturnBuffer.Add(kvp.Key);
            }
        }

        foreach (int floor in _floorsToReturnBuffer)
        {
            GameObject roomToDeactivate = activeRooms[floor];
            roomToDeactivate.SetActive(false); // Ẩn room đi để tiết kiệm hiệu năng

            // Đưa vào pool tương ứng với tên bản mẫu gốc (lưu qua RoomIdentity, không suy ra từ tên GameObject)
            string prefabName = GetPrefabKey(roomToDeactivate);
            if (!roomPool.ContainsKey(prefabName))
            {
                roomPool[prefabName] = new Queue<GameObject>();
            }
            roomPool[prefabName].Enqueue(roomToDeactivate);

            activeRooms.Remove(floor);
        }
    }

    void SpawnRoomAtFloor(int floor)
    {
        // Lấy danh sách Prefabs phù hợp với số tầng (Độ cao hiện tại)
        List<GameObject> availablePrefabs = GetPrefabsForFloor(floor);

        if (availablePrefabs == null || availablePrefabs.Count == 0)
        {
            Debug.LogWarning($"Không tìm thấy cấu hình Room cho tầng {floor}, sử dụng danh sách mặc định.");
            availablePrefabs = defaultRoomPrefabs;
        }

        // Guard: nếu vẫn không có prefab nào khả dụng (kể cả default) -> không spawn, tránh crash
        if (availablePrefabs == null || availablePrefabs.Count == 0)
        {
            Debug.LogError($"❌ Không có Room Prefab nào khả dụng cho tầng {floor} (cả cấu hình lẫn danh sách mặc định đều rỗng). Bỏ qua spawn.");
            return;
        }

        // Chọn ngẫu nhiên một mẫu room từ danh sách thỏa mãn độ khó
        int randomIndex = Random.Range(0, availablePrefabs.Count);
        GameObject selectedPrefab = availablePrefabs[randomIndex];

        if (selectedPrefab == null)
        {
            Debug.LogError($"❌ Prefab tại index {randomIndex} bị null trong danh sách cấu hình tầng {floor}. Bỏ qua spawn.");
            return;
        }

        string prefabName = selectedPrefab.name;

        GameObject room;

        // Kiểm tra xem trong Pool có sẵn Room mẫu này đang rảnh không
        if (roomPool.TryGetValue(prefabName, out var pool) && pool.Count > 0)
        {
            room = pool.Dequeue();
        }
        else
        {
            room = Instantiate(selectedPrefab);
            // Gắn định danh gốc để việc pooling không phụ thuộc vào tên GameObject lúc runtime
            var identity = room.GetComponent<RoomIdentity>();
            if (identity == null) identity = room.AddComponent<RoomIdentity>();
            identity.prefabName = prefabName;
        }

        // Đặt lại vị trí của Room theo trục Y tương ứng với số tầng
        Vector3 targetPosition = new Vector3(0, floor * roomHeight, 0);
        room.transform.position = targetPosition;
        room.SetActive(true);
        int tokenCount = GetTokenCountForFloor(floor);
        RandomizeTokens(room, tokenCount);

        // Lưu vào danh sách các Room đang chạy
        activeRooms[floor] = room;

        // Bạn có thể gọi Reset trạng thái quái vật/vật phẩm ở đây:
        // room.GetComponent<RoomController>()?.ResetRoom();
    }

    /// <summary>
    /// Lấy key định danh prefab gốc của 1 room, ưu tiên component RoomIdentity thay vì suy luận từ tên GameObject
    /// (tên có thể bị đổi bởi Instantiate hoặc code khác, gây lệch key khi pooling).
    /// </summary>
    private string GetPrefabKey(GameObject room)
    {
        var identity = room.GetComponent<RoomIdentity>();
        if (identity != null && !string.IsNullOrEmpty(identity.prefabName))
            return identity.prefabName;

        // Fallback cho các room cũ chưa có RoomIdentity (ví dụ prefab đã tồn tại trước khi thêm component này)
        return room.name.Replace("(Clone)", "").Trim();
    }

    /// <summary>
    /// Tìm kiếm và GỘP tất cả các Room Prefabs phù hợp nếu có sự chồng chập mốc tầng.
    /// LƯU Ý: nếu nhiều difficulty range chồng lấn tầng, prefab của TẤT CẢ range khớp sẽ được gộp chung để random.
    /// Nếu không muốn hành vi này (chỉ muốn ưu tiên range khai báo trước), hãy thêm "break;" sau khi đã match.
    /// </summary>
    private List<GameObject> GetPrefabsForFloor(int floor)
    {
        _combinedPrefabsBuffer.Clear();

        if (difficultySetup == null || difficultySetup.Count == 0) return null;

        foreach (var range in difficultySetup)
        {
            // Điều kiện: tầng hiện tại phải >= startFloor, và (là vô hạn HOẶC nằm trong endFloor)
            if (floor >= range.startFloor && (range.endFloor == 0 || floor <= range.endFloor))
            {
                if (range.prefabs != null && range.prefabs.Count > 0)
                {
                    _combinedPrefabsBuffer.AddRange(range.prefabs);
                }
            }
        }

        // Nếu tìm thấy map, trả về danh sách tổng hợp để thực hiện Roll Random
        return _combinedPrefabsBuffer.Count > 0 ? _combinedPrefabsBuffer : null;
    }

    /// <summary>
    /// Cập nhật vị trí của Kill Zone dựa trên điểm cao nhất Player đã đạt được (currentHeightest),
    /// trừ đi một khoảng đệm để tránh chết đột ngột khi vừa chạm mép dưới vùng hiển thị.
    /// </summary>
    void UpdateKillZonePosition()
    {
        if (killZoneTransform == null) return;
        if (GameManager.Instance == null) return;

        float playerHeightestY = GameManager.Instance.currentHeightest;
        float killZoneY = playerHeightestY - killZoneOffset;

        Vector3 newKillZonePos = killZoneTransform.position;
        newKillZonePos.y = killZoneY;
        killZoneTransform.position = newKillZonePos;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (roomHeight < 1f) roomHeight = 1f;
        if (killZoneOffset < 0f) killZoneOffset = 0f;
    }
#endif
}