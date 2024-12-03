using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("타겟 설정")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -5);

    [Header("카메라 설정")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private float maxRotationAngle = 80f;
    [SerializeField] private float moveThreshold = 0.1f; // 플레이어 이동 감지 임계값

    private float currentRotationAngle = 0f;
    private Vector3 velocity = Vector3.zero;
    private bool isDragging = false;
    private float lastMouseX;
    private Vector3 lastTargetPosition;

    private SurfaceContactDetector surfaceContactDetector;

    private void Start()
    {
        lastTargetPosition = target.position;
        surfaceContactDetector = target.GetComponent<SurfaceContactDetector>();
    }

    private void Update()
    {
        if (!surfaceContactDetector.GameOver())
        {
            HandleMouseInput();
        }

        HandlePlayerMovement();
        UpdateCameraPosition();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMouseX = Input.mousePosition.x;
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            float deltaX = Input.mousePosition.x - lastMouseX;
            currentRotationAngle -= deltaX * rotationSpeed * Time.deltaTime;
            currentRotationAngle = Mathf.Clamp(currentRotationAngle, -maxRotationAngle, maxRotationAngle);
            lastMouseX = Input.mousePosition.x;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    private void HandlePlayerMovement()
    {
        if (isDragging) return; // 드래그 중에는 방향 전환 무시

        // 플레이어의 이동 방향 계산
        Vector3 moveDirection = target.position - lastTargetPosition;
        moveDirection.y = 0; // 수직 이동은 무시

        // 의미 있는 이동이 있을 경우
        if (moveDirection.magnitude > moveThreshold)
        {
            // 이동 방향의 각도 계산
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            
            // 부드럽게 카메라 회전
            currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, -targetAngle, Time.deltaTime * 2f);
            currentRotationAngle = Mathf.Clamp(currentRotationAngle, -maxRotationAngle, maxRotationAngle);
        }

        lastTargetPosition = target.position;
    }

    private void UpdateCameraPosition()
    {
        // 현재 회전 각도에 따른 회전 행렬 생성
        Quaternion rotation = Quaternion.Euler(0, currentRotationAngle, 0);
        
        // 오프셋에 회전 적용
        Vector3 rotatedOffset = rotation * offset;
        
        // 타겟 위치 계산
        Vector3 desiredPosition = target.position + rotatedOffset;
        
        // 부드러운 이동 적용
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);
        
        // 항상 타겟을 바라보도록 설정
        transform.LookAt(target.position + Vector3.up * offset.y * 0.5f);
    }
}