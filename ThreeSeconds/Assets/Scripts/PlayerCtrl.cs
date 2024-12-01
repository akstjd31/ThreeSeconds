using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCtrl : MonoBehaviour
{
    // Move
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float groundCheckDistance = 0.1f;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private Vector3 smoothVelocity;
    private float smoothTime = 0.1f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private float groundCheckOffset = 0.1f;
    [SerializeField] private LayerMask groundLayer;  // 인스펙터에서 Ground 레이어 설정 필요

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float doubleJumpForce = 6f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private bool canDoubleJump = false;
    [SerializeField] private bool hasDoubleJumped = false;

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
        CheckGroundedState();
        HandleMovementInput();
        HandleJumpInput();
    }


    private void HandleMovementInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
    }

    private void HandleJumpInput()
    {
        // 점프 실행
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (IsGrounded() && !canDoubleJump && !hasDoubleJumped)
            {
                // 첫 번째 점프
                ExecuteJump(jumpForce);
                canDoubleJump = true;
                hasDoubleJumped = false;
            }
            else if (canDoubleJump && !hasDoubleJumped)
            {
                // 이단 점프
                ExecuteJump(doubleJumpForce);
                canDoubleJump = false;
                hasDoubleJumped = true;
            }
        }
    }
    private void ExecuteJump(float force)
    {
        // 수직 속도만 초기화
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * force, ForceMode.Impulse);
    }

    private void CheckGroundedState()
    {
        if (IsGrounded() && hasDoubleJumped)
        {
            canDoubleJump = false;
            hasDoubleJumped = false;
        }
    }
    private bool IsGrounded()
    {
        // 발 위치 계산
        Vector3 spherePosition = new Vector3(
            transform.position.x,
            transform.position.y - groundCheckOffset,
            transform.position.z
        );

        // OverlapSphere를 사용하여 더 정확한 지면 체크
        return Physics.OverlapSphere(
            spherePosition,
            groundCheckRadius,
            groundLayer
        ).Length > 0;
    }

    private void OnDrawGizmos()
    {
        // 지면 체크 범위 시각화
        Gizmos.color = IsGrounded() ? Color.green : Color.red;
        Vector3 spherePosition = new Vector3(
            transform.position.x,
            transform.position.y - groundCheckOffset,
            transform.position.z
        );
        Gizmos.DrawWireSphere(spherePosition, groundCheckRadius);
    }
}