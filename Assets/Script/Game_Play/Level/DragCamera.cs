using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragCamera : MonoBehaviour
{
    public float dragSpeed = 1.0f;

    // Giới hạn di chuyển (theo trục X và Z trong không gian World)
    public Vector2 minLimit = new Vector2(-10, -10);
    public Vector2 maxLimit = new Vector2(10, 10);

    private Camera cam;
    private Vector3 lastInputPosition;
    private bool isDragging;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (InputManager.IsInputDown())
        {
            lastInputPosition = InputManager.GetInputPosition();
            isDragging = true;
        }
        else if (InputManager.IsInputHeld() && isDragging)
        {
            Vector3 currentInputPosition = InputManager.GetInputPosition();
            Vector3 delta = cam.ScreenToViewportPoint(lastInputPosition - currentInputPosition);

            // Di chuyển theo trục X và Z
            Vector3 move = new Vector3(delta.x * dragSpeed, 0 , delta.y * dragSpeed);
            cam.transform.position += move;
            ClampCameraPosition();

            lastInputPosition = currentInputPosition;
        }
        else if (InputManager.IsInputUp())
        {
            isDragging = false;
        }
    }

    void ClampCameraPosition()
    {
        Vector3 pos = cam.transform.position;
        pos.x = Mathf.Clamp(pos.x, minLimit.x, maxLimit.x);
        pos.z = Mathf.Clamp(pos.z, minLimit.y, maxLimit.y); // Giới hạn Z thay vì Y
        cam.transform.position = pos;
    }
}

