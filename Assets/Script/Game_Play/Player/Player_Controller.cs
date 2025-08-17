using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("Jump Settings")]
    public float maxForce;
    public float DistanveDivide;
    public float forceMultiplier;
    public float targetForce;
    //public float maxCharge;
    public float forecAmount;
    //public float chargeSpeed;

    [Header("State")]
    public bool isGrounded;
    public bool isCharging = false;
    public bool Disableplayer;

    [Header("UI & Effects")]
    public LineRenderer lineRenderer;
    public GameObject FX_Death_Prefab;

    [Header("Scene Control")]
    public float MaxcountLoadscene;
    public SkinnedMeshRenderer meshRenderer;

    private float lastYposition;
    private Rigidbody rb;
    private Animator myAnimator;
    
    private Transform rudeTransform;
    private Quaternion rudeOriginalRotation;
    private Vector3 lastSafePosition;
    private Vector2 startTouchPos;
    private Vector2 currentTouchPos;
    private Vector3 smoothEndPos;
    private Vector3 jumpDirection;
    private Coroutine loadSceneCoroutine;


    // ========== Force Settings ==========
         
    [SerializeField] private float forceLerpSpeed = 8f;  // Tốc độ lerp để cảm giác "gồng lực"
    [SerializeField] private float minDragDistance = 1f; // Khoảng drag nhỏ nhất để tính hướng
    

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        smoothEndPos = transform.position;
        rb = GetComponent<Rigidbody>();
        lastYposition = transform.position.y;

        // Cache Rude & Animator
        rudeTransform = transform.Find("Rude");
        if (rudeTransform != null)
        {
            myAnimator = rudeTransform.GetComponent<Animator>();
            rudeOriginalRotation = rudeTransform.localRotation;
        }
        else
        {
            Debug.LogError("Không tìm thấy object con 'Rude'");
        }

        // Cache Mesh Renderer trong mọi object con
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>(true);
        if (meshRenderer == null)
        {
            Debug.LogWarning("Không tìm thấy SkinnedMeshRenderer trong các object con.");
        }

        Disableplayer = false;
        SetAnimatorState(isHold: false, isJump: false, isIdle: true);
    }

    private void Update()
    {
        if (!isGrounded || Disableplayer) return;

        bool inputDown = InputManager.IsInputDown();
        bool inputHeld = InputManager.IsInputHeld();
        bool inputUp = InputManager.IsInputUp();

        /*if (inputDown) StartCharging();
        else if (inputHeld) ChargeJump();
        else if (inputUp) ReleaseJump();
        */

        if (inputDown) StartCharging();
        else if (inputHeld) DragJump();
        else if (inputUp) ReleaseJump();
    }
    /*  Charge force Jump
      private void StartCharging()
      {

          isCharging = true;
          forecAmount = 0f;
          lineRenderer.enabled = true;
          SetAnimatorState(isHold: true, isIdle: false);

          startTouchPos = InputManager.GetInputPosition();

          // Ghi lại vị trí an toàn trước khi nhảy


          if (ReviveManager.Instance != null)
          {
              lastSafePosition = transform.position;
              ReviveManager.Instance.RecordSafePosition(lastSafePosition);
          }
      }

      private void ChargeJump()
      {
          forecAmount += chargeSpeed * Time.deltaTime;
          forecAmount = Mathf.Clamp(forecAmount, 0, maxForce);

          currentTouchPos = InputManager.GetInputPosition();
          Vector2 dragVector = currentTouchPos - startTouchPos;

          if (dragVector.magnitude >= 5f)
          {
              jumpDirection = new Vector3(dragVector.x, Mathf.Abs(dragVector.y), 0f).normalized;
          }
          else
          {
              Vector3 inputPos = currentTouchPos;
              inputPos.z = Camera.main.transform.position.y - transform.position.y;
              Vector3 worldPos = Camera.main.ScreenToWorldPoint(inputPos);
              jumpDirection = new Vector3(worldPos.x - transform.position.x, 1, 0).normalized;
          }

          // Giới hạn góc xoay
          float rawAngle = Mathf.Atan2(jumpDirection.x, Mathf.Abs(jumpDirection.y)) * Mathf.Rad2Deg;
          float clampedAngle = Mathf.Clamp(rawAngle, -45f, 45f);
          Quaternion targetRotation = Quaternion.Euler(0, 180 - clampedAngle, 0);
          rudeTransform.localRotation = Quaternion.Slerp(rudeTransform.localRotation, targetRotation, Time.deltaTime * 5f);

          // Cập nhật đường nhảy
          Vector3 centerPosition = transform.position + Vector3.up;
          Vector3 targetEndPos = centerPosition + jumpDirection * 3;
          smoothEndPos = Vector3.Lerp(smoothEndPos, targetEndPos, Time.deltaTime * 10f);

          lineRenderer.SetPosition(0, centerPosition);
          lineRenderer.SetPosition(1, smoothEndPos);
      }

      private void ReleaseJump()
      {
          isCharging = false;
          rb.AddForce(jumpDirection * forecAmount, ForceMode.Impulse);
          lineRenderer.enabled = false;
          isGrounded = false;
          SetAnimatorState(isJump: true, isHold: false);
      }
     */

    private void StartCharging()
    {
        isCharging = true;
        forecAmount = 0f;
        lineRenderer.enabled = true;
        SetAnimatorState(isHold: true, isIdle: false);

        startTouchPos = InputManager.GetInputPosition();

        if (ReviveManager.Instance != null)
        {
            lastSafePosition = transform.position;
            ReviveManager.Instance.RecordSafePosition(lastSafePosition);
        }
    }

    private void DragJump()
    {
        currentTouchPos = InputManager.GetInputPosition();

        // 1. Tính khoảng cách drag → targetForce
        float dragDistance = ((currentTouchPos - startTouchPos).magnitude)/ DistanveDivide;
        targetForce = Mathf.Clamp(dragDistance, 0, maxForce);

        // 2. Lerp lực cho cảm giác mượt
        forecAmount = Mathf.Lerp(forecAmount, targetForce, Time.deltaTime * forceLerpSpeed);
        

        // 3. Tính hướng nhảy
        Vector2 dragVector = currentTouchPos - startTouchPos;
        if (dragVector.magnitude >= minDragDistance)
        {
            jumpDirection = new Vector3(dragVector.x, Mathf.Abs(dragVector.y), 0f).normalized;
        }
        else
        {
            jumpDirection = Vector3.up;
        }

        // 4. Giới hạn góc xoay
        float rawAngle = Mathf.Atan2(jumpDirection.x, Mathf.Abs(jumpDirection.y)) * Mathf.Rad2Deg;
        float clampedAngle = Mathf.Clamp(rawAngle, -45f, 45f);
        Quaternion targetRotation = Quaternion.Euler(0, 180 - clampedAngle, 0);
        rudeTransform.localRotation = Quaternion.Slerp(rudeTransform.localRotation, targetRotation, Time.deltaTime * 5f);

        // 5. LineRenderer theo lực
        Vector3 centerPosition = transform.position + Vector3.up;
        Vector3 targetEndPos = centerPosition + jumpDirection * (forecAmount * 0.35f);
        smoothEndPos = Vector3.Lerp(smoothEndPos, targetEndPos, Time.deltaTime * forceLerpSpeed);

        lineRenderer.SetPosition(0, centerPosition);
        lineRenderer.SetPosition(1, smoothEndPos);
    }

    private void ReleaseJump()
    {
        
        isCharging = false;
        rb.AddForce(jumpDirection * forecAmount, ForceMode.Impulse);
        Vector3 centerPosition = transform.position + Vector3.up;
        lineRenderer.SetPosition(0, centerPosition);
        lineRenderer.SetPosition(1, centerPosition);
        lineRenderer.enabled = false;
        isGrounded = false;
        SetAnimatorState(isJump: true, isHold: false);
        forecAmount = 0f;
        targetForce = 0f;
    }

    private void OnCollisionStay(Collision collision)
    {
        SetAnimatorState(isIdle: true, isJump: false);
        isGrounded = true;

        if (rudeTransform != null && !isCharging)
        {
            rudeTransform.localRotation = Quaternion.Slerp(rudeTransform.localRotation, rudeOriginalRotation, Time.deltaTime * 10f);
        }

        if (collision.collider.CompareTag("Trap"))
        {
            HandleTrapCollision();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
            HandleGoalTrigger();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        SetAnimatorState(isIdle: false, isJump: true);
        isGrounded = false;
    }

    public bool IsGrounded() => isGrounded;

    public void ReviveAt(Vector3 position)
    {
        // Reset vị trí
        transform.position = position;

        // Reset Rigidbody
        rb.isKinematic = true;
        //rb.velocity = Vector3.zero;
        //rb.angularVelocity = Vector3.zero;
        //rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

        // Bật mesh và điều khiển
        Disableplayer = false;
        if (meshRenderer != null) meshRenderer.enabled = true;

        // Animator về idle
        SetAnimatorState(isJump: false, isHold: false, isIdle: true);

        Debug.Log("Đã revive tại vị trí an toàn");
        StartCoroutine(EnablePhysicsNextFrame());
    }


    private void HandleTrapCollision()
    {
        if (Disableplayer) return;

        // FX chết
        Instantiate(FX_Death_Prefab, transform.position + Vector3.up, Quaternion.identity);

        // Tắt điều khiển và mesh
        Disableplayer = true;
        if (meshRenderer != null) meshRenderer.enabled = false;

        // Gọi UI revive nếu có và hợp lệ
        if (UIManager.Instance != null && UIManager.Instance.revivePanel != null)
        {
            StartCoroutine(WailTime());
            
        }
        else
        {
            if (loadSceneCoroutine == null)
            {
                loadSceneCoroutine = StartCoroutine(LoadSceneAfterDelay(() =>
                {
                    Debug.LogWarning("Không tìm thấy UIManager hoặc revivePanel - reload scene trực tiếp.");
                    GameManager.Instance.RestartLevel();
                }));
            }
        }
    }





    private void HandleGoalTrigger()
    {
        Disableplayer = true;
        Debug.Log("Chạm đích!");

        if (loadSceneCoroutine == null)
        {
            loadSceneCoroutine = StartCoroutine(LoadSceneAfterDelay(() =>
            {
                GameManager.Instance.LoadNextLevel();
            }));
        }
    }

    private IEnumerator LoadSceneAfterDelay(System.Action onComplete)
    {
        float timer = 0f;
        while (timer < MaxcountLoadscene)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        onComplete?.Invoke();
    }

    IEnumerator WailTime()
    {
        yield return new WaitForSeconds(1f);
        UIManager.Instance.ShowReviveOption();
    }

    private IEnumerator EnablePhysicsNextFrame()
    {
        yield return null; // chờ 1 frame
        rb.isKinematic = false;
    }

    private void SetAnimatorState(bool? isHold = null, bool? isJump = null, bool? isIdle = null)
    {
        if (myAnimator == null) return;
        if (isHold.HasValue) myAnimator.SetBool("isHold", isHold.Value);
        if (isJump.HasValue) myAnimator.SetBool("isJump", isJump.Value);
        if (isIdle.HasValue) myAnimator.SetBool("isIdel", isIdle.Value);
    }
}
