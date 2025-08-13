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
