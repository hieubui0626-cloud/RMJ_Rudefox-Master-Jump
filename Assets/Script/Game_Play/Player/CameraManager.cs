using UnityEngine;

public class CameraManager : MonoBehaviour
{

    private Camera cam;
    [Header("Camera Follow Settings")]
    public GameObject player; // Kéo player vào đây trong Inspector
    public Vector3 offset = new Vector3(0f, 5f, -10f); // Khoảng cách camera so với player
    public float smoothSpeed = 0.125f; // Tốc độ mượt khi camera di chuyển

    public float maxZoomIn;
    public float maxZoomOut;

    [Header("Camera Drag Settings")]
    public float dragSpeed = 1.0f;
    private Vector3 lastInputPosition;
    private bool isDragging;
    public bool canDrag = true;

    public Vector2 minLimit = new Vector2(-10, -10);
    public Vector2 maxLimit = new Vector2(10, 10);

    void Start()
    {
        cam = Camera.main;
        
        if(canDrag)
        {
            if (PlayerPrefs.HasKey("Levelmap_CamX") && PlayerPrefs.HasKey("Levelmap_CamY") && PlayerPrefs.HasKey("Levelmap_CamZ"))
            {
                float xPos = PlayerPrefs.GetFloat("Levelmap_CamX");
                //float yPos = PlayerPrefs.GetFloat("Levelmap_CamY");
                float zPos = PlayerPrefs.GetFloat("Levelmap_CamZ");
                cam.transform.position = new Vector3(xPos, cam.transform.position.y, zPos);
            }
        }
        
        
    }   
    void Update()
    {
        float zoomDelta = InputManager.GetZoomDelta();
        if (zoomDelta != 0f)
        {
            Debug.Log("Zoom Delta: " + zoomDelta);
            
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - zoomDelta * 5, maxZoomIn, maxZoomOut);
        }
        if(canDrag)
        {
            DragCamera();
        }
        else
        {
            FollowPlayer();
        }

    }

    public void DragCamera()
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
    public void FollowPlayer()
    {
        player = GameObject.FindWithTag("Player");
        Vector3 desiredPosition = player.transform.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    void ClampCameraPosition()
    {
        Vector3 pos = cam.transform.position;
        pos.x = Mathf.Clamp(pos.x, minLimit.x, maxLimit.x);
        pos.z = Mathf.Clamp(pos.z, minLimit.y, maxLimit.y); // Giới hạn Z thay vì Y
        cam.transform.position = pos;
    }

}