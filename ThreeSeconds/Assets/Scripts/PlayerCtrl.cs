using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    // Move
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float groundCheckDistance = 0.1f;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private Vector3 smoothVelocity;
    private float smoothTime = 0.1f;

    // Jump
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float doubleJumpForce = 6f;  // 이단 점프의 힘
    [SerializeField] private float maxJumpCooldown = 0.1f;  // 연속 점프 방지용 쿨다운
    private float jumpCooldown;
    private bool canDoubleJump = false;  // 이단 점프 가능 여부
    private bool hasDoubleJumped = false;  // 이단 점프 사용 여부

    // ... existing code ...
    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 리지드바디 설정 추가
        //rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.mass = 1f;
        rb.drag = 0.5f;
        rb.angularDrag = 0.05f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        if (moveDirection != Vector3.zero)
        {
            // SmoothDamp를 사용한 부드러운 이동
            Vector3 targetVelocity = moveDirection * moveSpeed;
            Vector3 smoothedVelocity = Vector3.SmoothDamp(
                rb.velocity,
                targetVelocity,
                ref smoothVelocity,
                smoothTime
            );

            // y축 속도는 유지
            smoothedVelocity.y = rb.velocity.y;
            rb.velocity = smoothedVelocity;
        }
        else
        {
            // 부드러운 정지
            Vector3 stopVelocity = Vector3.SmoothDamp(
                rb.velocity,
                new Vector3(0f, rb.velocity.y, 0f),
                ref smoothVelocity,
                smoothTime
            );
            rb.velocity = stopVelocity;
        }
    }

    private void Update()
    {
        HandleMovementInput();
        HandleJumpInput();
        UpdateJumpCooldown();
        CheckGroundedState();
    }

    private void HandleMovementInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
    }

    // ... existing code ...

    private void HandleJumpInput()
    {
        if (!Input.GetKeyDown(KeyCode.Space)) return;

        if (IsGrounded() && jumpCooldown <= 0 && !hasDoubleJumped)
        {
            // 첫 번째 점프
            Jump();
            canDoubleJump = true;
            jumpCooldown = maxJumpCooldown;
        }
        else if (!IsGrounded() && canDoubleJump && !hasDoubleJumped)
        {
            // 이단 점프
            DoubleJump();
            canDoubleJump = false;
            hasDoubleJumped = true;
        }
    }

    private void CheckGroundedState()
    {
        if (IsGrounded())
        {
            // 착지했을 때만 모든 점프 관련 상태 초기화
            canDoubleJump = false;
            hasDoubleJumped = false;
            jumpCooldown = 0f;
        }
    }

    private void UpdateJumpCooldown()
    {
        if (jumpCooldown > 0)
        {
            jumpCooldown -= Time.deltaTime;
        }
    }


    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void DoubleJump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * doubleJumpForce, ForceMode.Impulse);
    }

    private bool IsGrounded()
    {
        // 지면 체크 개선
        return Physics.SphereCast(
            transform.position + Vector3.up * 0.5f,
            0.4f,  // 구의 반지름
            Vector3.down,
            out RaycastHit hit,
            0.6f   // 체크 거리
        );
    }
}