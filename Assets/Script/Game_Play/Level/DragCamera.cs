using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragCamera : MonoBehaviour
{
    public float dragSpeed = 1.0f;

    // Giới hạn di chuyển (theo trục X và Z trong không gian World)
    public Vector2 minLimit = new Vector2(-10, -10);
    public Vector2 maxLimit = new Vector2(10, 10);
    public float maxZoomIn;
    public float maxZoomOut;

    private Camera cam;
    private Vector3 lastInputPosition;
    private bool isDragging;
    public bool canDrag = true; // Biến để kiểm soát khả năng kéo camera

    void Start()
    {
        cam = Camera.main;
        if (PlayerPrefs.HasKey("Levelmap_CamX") && PlayerPrefs.HasKey("Levelmap_CamY") && PlayerPrefs.HasKey("Levelmap_CamZ"))
        {
            float xPos = PlayerPrefs.GetFloat("Levelmap_CamX");
            float yPos = PlayerPrefs.GetFloat("Levelmap_CamY");
            float zPos = PlayerPrefs.GetFloat("Levelmap_CamZ");
            cam.transform.position = new Vector3(xPos, yPos, zPos);
        }
    }

    void Update()
    {
        if (canDrag)
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
                Vector3 move = new Vector3(delta.x * dragSpeed, 0, delta.y * dragSpeed);
                cam.transform.position += move;
                ClampCameraPosition();

                lastInputPosition = currentInputPosition;

                PlayerPrefs.SetFloat("Levelmap_CamX", cam.transform.position.x);
                PlayerPrefs.SetFloat("Levelmap_CamY", cam.transform.position.y);
                PlayerPrefs.SetFloat("Levelmap_CamZ", cam.transform.position.z);
            }
            else if (InputManager.IsInputUp())
            {
                isDragging = false;
            }
        }
        
        float zoomDelta = InputManager.GetZoomDelta();
        if (zoomDelta != 0f)
        {
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - zoomDelta * 5, maxZoomIn, maxZoomOut);
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

