using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerInputController : MonoBehaviour
{
    private InputActions controls;
    private Vector2 moveInput;
    public float moveSpeed = 5f;

    public Transform cameraTransform; // 여기 연결 필요!
    private Animator animator; //animator 추가
    private CharacterController controller;

    private Vector3 velocity;                // 중력 벡터
    public float gravity = -20f;            // 중력 가속도 (더 빠르게 떨어지게 설정)
    public float groundCheckDistance = 0.2f; // 지면 감지 거리

    void Awake()
    {
        controls = new InputActions();
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        animator = GetComponent<Animator>(); // Animator 컴포넌트 할당
        controller = GetComponent<CharacterController>();
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Update()
    {
        if (cameraTransform == null) return;

        // 카메라 기준 방향 설정
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        // 이동 방향 계산
        Vector3 inputDirection = forward * moveInput.y + right * moveInput.x;

        // 애니메이터에 파라미터 전달 (움직이는지 여부)
        bool isMoving = inputDirection.sqrMagnitude > 0.01f;
        animator.SetBool("IsMove", isMoving);

        if (isMoving)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // 지면 감지 → velocity.y 리셋 or 중력 적용
        if (IsGrounded())
        {
            velocity.y = -1f; // 착지 후 약간의 음수값 유지 (바닥에 달라붙게)
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        // 이동 + 중력 반영
        Vector3 finalMove = inputDirection * moveSpeed + velocity;
        controller.Move(finalMove * Time.deltaTime);
    }

    // Raycast로 지면 감지
    bool IsGrounded()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        return Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance + 0.1f);
    }
}
