using UnityEngine;
using Newtonsoft.Json;

// Class chứa dữ liệu được thiết kế lại để khớp chính xác với file 1.json mới
[System.Serializable]
public class RoomData
{
    // Đổi grid_width thành width để khớp với JSON mới
    public int width;

    // Đổi grid_height thành height để khớp với JSON mới
    public int height;

    // Giữ lại max_jump_height nếu sau này game RMJ cần dùng logic tính toán khoảng cách nhảy
    public int max_jump_height;

    // Ma trận dữ liệu phòng dạng mảng 2 chiều
    public int[][] matrix;

    // Các trường bổ sung nếu file JSON không có để tránh log bị lỗi hiển thị rỗng
    [HideInInspector] public string room_name = "Room_Endless_Generated";
    [HideInInspector] public string difficulty = "Dynamic";
}

public class RoomJsonSpawner : MonoBehaviour
{
    [Header("Cấu hình đầu vào")]
    public TextAsset jsonFile;
    public GameObject platformPrefab;
    public float cellSize = 1f;

    public void SpawnRoomFromJSON()
    {
        if (jsonFile == null || platformPrefab == null)
        {
            Debug.LogError("RMJ Error: Thiếu file JSON hoặc Platform Prefab!");
            return;
        }

        // Dọn dẹp các khối cũ trước khi sinh khối mới
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(transform.GetChild(i).gameObject);
            else
                DestroyImmediate(transform.GetChild(i).gameObject);
        }

        // Đọc dữ liệu JSON mới
        RoomData data = JsonConvert.DeserializeObject<RoomData>(jsonFile.text);

        // Kiểm tra an toàn dữ liệu sau khi parse
        if (data == null || data.matrix == null || data.width == 0 || data.height == 0)
        {
            Debug.LogError("RMJ Error: Không thể đọc ma trận hoặc Kích thước file JSON bằng 0x0! Hãy kiểm tra lại file JSON.");
            return;
        }

        // Duyệt ma trận 2 chiều dựa theo dữ liệu width và height mới
        for (int y = 0; y < data.height; y++)
        {
            for (int x = 0; x < data.width; x++)
            {
                // Kiểm tra nếu phần tử ma trận bằng 1 thì sinh platform
                if (data.matrix[y][x] == 1)
                {
                    // Công thức tính tọa độ chuẩn cho game nhảy cao trục Y
                    Vector3 spawnPos = new Vector3(x * cellSize, (data.height - 1 - y) * cellSize, 0);

                    GameObject platform;
#if UNITY_EDITOR
                    // Giữ liên kết Prefab gốc trong Scene Editor
                    platform = UnityEditor.PrefabUtility.InstantiatePrefab(platformPrefab) as GameObject;
                    if (platform != null) platform.transform.position = spawnPos;
#else
                    // Chạy trong game thật
                    platform = Instantiate(platformPrefab, spawnPos, Quaternion.identity);
#endif
                    if (platform != null)
                    {
                        platform.transform.SetParent(this.transform);
                    }
                }
            }
        }

        // Log báo cáo trực quan chính xác thông số phòng
        Debug.Log($"✅ Sinh thành công: {data.room_name} | Max Jump: {data.max_jump_height} | Kích thước lưới: {data.width}x{data.height}");
    }
}