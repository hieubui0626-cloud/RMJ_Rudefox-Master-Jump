using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // Kéo player vào đây trong Inspector
    public Vector3 offset = new Vector3(0f, 5f, -10f); // Khoảng cách camera so với player
    public float smoothSpeed = 0.125f; // Tốc độ mượt khi camera di chuyển

    void LateUpdate()
    {
        // Vị trí mong muốn của camera
        Vector3 desiredPosition = player.position + offset;

        // Di chuyển camera mượt đến vị trí mong muốn
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Gán vị trí cho camera
        transform.position = smoothedPosition;

        // (Tùy chọn) Giữ camera luôn nhìn vào player
        // transform.LookAt(player);
    }
}