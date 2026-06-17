using UnityEngine;

public static class InputManager
{
    public static bool IsInputDown()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        
        return Input.GetMouseButtonDown(0);
#elif UNITY_ANDROID || UNITY_IOS
        return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
#else
        return false;
#endif
    }

    public static bool IsInputHeld()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return Input.GetMouseButton(0);
#elif UNITY_ANDROID || UNITY_IOS
        return Input.touchCount > 0 &&
            (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary);
#else
        return false;
#endif
    }

    public static bool IsInputUp()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        
        return Input.GetMouseButtonUp(0);
#elif UNITY_ANDROID || UNITY_IOS
        return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended;
#else
        return false;
#endif
    }
    public static float GetZoomDelta()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        // Trả về trực tiếp giá trị cuộn chuột (-1 đến 1) 
        return Input.GetAxis("Mouse ScrollWheel");
#elif UNITY_ANDROID || UNITY_IOS
    // Kiểm tra có đủ 2 ngón tay chạm màn hình
    if (Input.touchCount == 2)
    {
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        // Tính khoảng cách hiện tại giữa 2 ngón tay
        float currentDist = Vector2.Distance(touch0.position, touch1.position);
        
        // Tính khoảng cách ở khung hình trước
        Vector2 prevPos0 = touch0.position - touch0.deltaPosition;
        Vector2 prevPos1 = touch1.position - touch1.deltaPosition;
        float prevDist = Vector2.Distance(prevPos0, prevPos1);

        // Trả về độ chênh lệch (Dương = Phóng to, Âm = Thu nhỏ)
        // Nhân với hệ số nhỏ (vd: 0.01f) để cân bằng tốc độ với chuột PC
        return (currentDist - prevDist) * 0.01f; 
    }
    return 0f;
#else
    return 0f;
#endif
    }

    public static Vector3 GetInputPosition()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return Input.mousePosition;
#elif UNITY_ANDROID || UNITY_IOS
        return Input.GetTouch(0).position;
#else
        return Vector3.zero;
#endif
    }
    

    public static Vector3 GetWorldInputPosition(Camera cam, float depth)
    {
        Vector3 screenPos = GetInputPosition();
        screenPos.z = depth;
        return cam.ScreenToWorldPoint(screenPos);
    }
}
