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
    public float roomHeight = 20f; // Chiều cao cố định của mỗi Room

    // Quản lý các room đang hoạt động trong Scene theo số tầng (Index)
    private Dictionary<int, GameObject> activeRooms = new Dictionary<int, GameObject>();

    // Object Pool chung lưu trữ theo tên Prefab để tái sử dụng tối ưu hiệu năng
    private Dictionary<string, Queue<GameObject>> roomPool = new Dictionary<string, Queue<GameObject>>();

    private int lastPlayerFloor = -1;

    void Awake()
    {
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
            UpdateKillZonePosition(currentPlayerFloor);
            
            
        }
        
    }

    private int GetTokenCountForFloor(int floor)
    {
        foreach (var range in difficultySetup)
        {
            if (floor >= range.startFloor && floor <= range.endFloor)
            {
                return range.NumberTokenSpawn;
            }
        }

        return 0;
    }

    private void RandomizeTokens(GameObject room, int numberToEnable)
    {
        Transform layoutToken = room.transform.Find("Layout_Token");

        if (layoutToken == null)
            return;

        List<GameObject> childList = new List<GameObject>();

        foreach (Transform child in layoutToken)
        {
            childList.Add(child.gameObject);
        }

        int totalChildren = childList.Count;

        numberToEnable = Mathf.Clamp(numberToEnable, 0, totalChildren);

        // Fisher-Yates Shuffle
        for (int i = 0; i < childList.Count; i++)
        {
            int randomIndex = Random.Range(i, childList.Count);

            GameObject temp = childList[i];
            childList[i] = childList[randomIndex];
            childList[randomIndex] = temp;
        }

        for (int i = 0; i < childList.Count; i++)
        {
            childList[i].SetActive(i < numberToEnable);
        }
    }
    void UpdateRooms(int currentFloor)
    {
        
        // Xác định các tầng cần phải hiển thị xung quanh Player (Ít nhất 2 tầng dưới, 1 tầng trên)
        HashSet<int> floorsToActivate = new HashSet<int>();

        floorsToActivate.Add(currentFloor);     // Tầng hiện tại người chơi đang đứng
        floorsToActivate.Add(currentFloor + 1); // 1 Tầng phía trên để chuẩn bị nhảy lên
        layoutToken = gameObject.GetComponentInChildren<Transform>().Find("Layout_Token")?.gameObject;

        // Giữ ít nhất 1 tầng bên dưới nếu người chơi đã nhảy đủ cao
        if (currentFloor - 1 >= 0) floorsToActivate.Add(currentFloor - 1);

        // 1. Sinh hoặc kích hoạt các Room thuộc các tầng cần thiết
        foreach (int floor in floorsToActivate)
        {
            if (!activeRooms.ContainsKey(floor))
            {
                SpawnRoomAtFloor(floor);
            }
        }

        // 2. Thu hồi (Ẩn) các Room nằm ngoài danh sách hiển thị
        List<int> floorsToReturn = new List<int>();
        foreach (var kvp in activeRooms)
        {
            if (!floorsToActivate.Contains(kvp.Key))
            {
                floorsToReturn.Add(kvp.Key);
            }
        }

        foreach (int floor in floorsToReturn)
        {
            GameObject roomToDeactivate = activeRooms[floor];
            roomToDeactivate.SetActive(false); // Ẩn room đi để tiết kiệm hiệu năng

            // Đưa vào pool tương ứng với tên bản mẫu của nó để tái sử dụng
            string prefabName = roomToDeactivate.name.Replace("(Clone)", "").Trim();
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

        // Chọn ngẫu nhiên một mẫu room từ danh sách thỏa mãn độ khó
        int randomIndex = Random.Range(0, availablePrefabs.Count);
        GameObject selectedPrefab = availablePrefabs[randomIndex];
        string prefabName = selectedPrefab.name;

        GameObject room;

        // Kiểm tra xem trong Pool có sẵn Room mẫu này đang rảnh không
        if (roomPool.ContainsKey(prefabName) && roomPool[prefabName].Count > 0)
        {
            room = roomPool[prefabName].Dequeue();
        }
        else
        {
            room = Instantiate(selectedPrefab);
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
    /// Tìm kiếm và GỘP tất cả các Room Prefabs phù hợp nếu có sự chồng chập mốc tầng
    /// </summary>
    private List<GameObject> GetPrefabsForFloor(int floor)
    {
        // Tạo một danh sách tạm thời để chứa tất cả các Prefab hợp lệ
        List<GameObject> combinedPrefabs = new List<GameObject>();

        if (difficultySetup == null || difficultySetup.Count == 0) return null;

        foreach (var range in difficultySetup)
        {
            // Điều kiện 1: Tầng hiện tại phải lớn hơn hoặc bằng tầng bắt đầu cấu hình
            if (floor >= range.startFloor && (floor <= range.endFloor || range.endFloor == 0))
            {
                // Điều kiện 2: Nếu cấu hình là vô hạn, HOẶC tầng hiện tại nằm trong khoảng kết thúc
                if (range.prefabs != null && range.prefabs.Count > 0)
                {
                    combinedPrefabs.AddRange(range.prefabs);
                }
                
                
            }
        }

        // Nếu tìm thấy map, trả về danh sách tổng hợp để thực hiện Roll Random
        return combinedPrefabs.Count > 0 ? combinedPrefabs : null;
    }

    /// <summary>
    /// Cập nhật vị trí của Kill Zone dựa trên tầng hiện tại của Player
    /// </summary>
    void UpdateKillZonePosition(int currentFloor)
    {
        if (killZoneTransform == null) return;

        // Tầng thấp nhất đang hiển thị sẽ là (currentFloor - 2)
        int lowestActiveFloor = currentFloor - 1;
        if (lowestActiveFloor < 0) lowestActiveFloor = 0;
        float playerHeightestY = GameManager.Instance.currentHeightest;

        // Đặt vị trí Y của Kill Zone nằm ngay dưới đáy của tầng thấp nhất đang hiển thị
        // Trừ bớt đi một khoảng đệm nhỏ (ví dụ: 3 đơn vị) để người chơi không chết quá đột ngột khi vừa chạm mép dưới
        float killZoneY = playerHeightestY - 60f;

        Vector3 newKillZonePos = killZoneTransform.position;
        newKillZonePos.y = killZoneY;
        killZoneTransform.position = newKillZonePos;
    }
    
}