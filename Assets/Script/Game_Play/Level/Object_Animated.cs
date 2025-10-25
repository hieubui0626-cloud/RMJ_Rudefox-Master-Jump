using UnityEngine;

public class Object_Animated : MonoBehaviour
{
    [Header("Movement Settings")]
    public float movingPingPongLength = 1f;
    public float movingSpeed = 1f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 50f;
    public float Maximum_Angles;

    [Header("Behavior Toggles")]
    public bool moveBackground;
    public bool moveGround;
    public bool rotate;
    public bool rotateWheel;
    public bool rotatePingPong;
    public bool moveX;
    public bool moveY;

    private float _initialY;
    private float _initialX;
    private float _direction;
    private float yRotation;
    private float zRotation;


    void Start()
    {
        _direction = Random.value < 0.5f ? 1f : -1f;
        _initialY = transform.position.y;
        _initialX = transform.position.x;
    }

    void Update()
    {
        if (moveBackground)
            AnimateBackground();

        if (moveGround)
            AnimateGround();

        if (rotate)
            RotateObject();

        if (rotateWheel)
            RotateWheel();

        if (rotatePingPong)
            RotatePingPong();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && moveX)
        {
            collision.transform.SetParent(transform);
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && (moveX || moveY))
        {
            // chỉ gỡ parent nếu player đang là child của platform
            if (collision.transform.parent == transform)
                collision.transform.SetParent(null);
        }
    }

    private void AnimateBackground()
    {
        float offset = (Mathf.PingPong(Time.time * movingSpeed, movingPingPongLength) - movingPingPongLength) * _direction;
        transform.position = new Vector3(transform.position.x, _initialY + offset, transform.position.z);
    }

    private void AnimateGround()
    {
        Vector3 pos = transform.position;

        if (moveY)
            pos.y = _initialY + Mathf.PingPong(Time.time * movingSpeed, movingPingPongLength) - movingPingPongLength;

        if (moveX)

            pos.x = _initialX + Mathf.PingPong(Time.time * movingSpeed, movingPingPongLength) - movingPingPongLength;

        transform.position = pos;
    }

    private void RotateObject()
    {

        yRotation += rotationSpeed * Time.deltaTime;
        yRotation = Mathf.Repeat(yRotation, 360f); // Giới hạn góc 0–360

        // Giữ nguyên X, Z
        Vector3 currentRotation = transform.eulerAngles;
        currentRotation.y = yRotation;
        transform.rotation = Quaternion.Euler(currentRotation);
    }

    private void RotateWheel()
    {

        zRotation += rotationSpeed * Time.deltaTime;
        zRotation = Mathf.Repeat(zRotation, 360f); // reset sau mỗi 360 độ
        transform.rotation = Quaternion.Euler(0f, 0f, zRotation);
    }
    private void RotatePingPong()
    {

        float angle = Mathf.Sin(Time.time * rotationSpeed) * Maximum_Angles;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
