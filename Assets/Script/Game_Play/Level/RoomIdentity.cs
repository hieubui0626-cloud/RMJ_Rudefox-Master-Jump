using UnityEngine;

/// <summary>
/// Gắn tự động lên mỗi Room được Instantiate bởi RoomManager để lưu lại tên prefab gốc.
/// Giúp việc Object Pooling không phụ thuộc vào tên GameObject lúc runtime (vốn có thể
/// bị đổi bởi code khác, gây lệch key khi trả room về pool).
/// Không cần gắn tay trong Inspector - RoomManager tự thêm component này khi spawn room mới.
/// </summary>
public class RoomIdentity : MonoBehaviour
{
    public string prefabName;
}
