using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerInputController : MonoBehaviour
{
    private InputActions controls;
    private Vector2 moveInput;
    public float moveSpeed = 5f;
    public bool canMove = true;

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
        // 카메라 없어도 동작
        Vector3 fwd = Vector3.forward, right = Vector3.right;
        if (cameraTransform != null)
        {
            fwd = cameraTransform.forward; fwd.y = 0; fwd.Normalize();
            right = cameraTransform.right; right.y = 0; right.Normalize();
        }

        Vector3 inputDir = (fwd * moveInput.y + right * moveInput.x);
        if (inputDir.sqrMagnitude > 1e-4f) inputDir.Normalize();

        bool isMoving = inputDir.sqrMagnitude > 0.01f;
        animator.SetBool("IsMove", isMoving);
        if (isMoving)
        {
            var targetRot = Quaternion.LookRotation(inputDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }

        // ---- 지면 판정 개선 ----
        bool grounded = controller.isGrounded || GroundCheckSphere();

        if (grounded)
            velocity.y = -2f;     // 살짝 더 강하게 바닥 스냅
        else
            velocity.y += gravity * Time.deltaTime;

        Vector3 finalMove = inputDir * moveSpeed + velocity;
        controller.Move(finalMove * Time.deltaTime);
    }

    // 발 위치에서 짧게 SphereCast (CharacterController 치수 기반)
    bool GroundCheckSphere()
    {
        // CC 발 위치
        float halfH = controller.height * 0.5f;
        float feetOffset = halfH - controller.radius;
        Vector3 feet = transform.position + controller.center + Vector3.down * feetOffset + Vector3.up * 0.05f;

        return Physics.SphereCast(
            feet, controller.radius * 0.95f, Vector3.down,
            out _, 0.15f, ~0, QueryTriggerInteraction.Ignore
        );
    }

    // Raycast로 지면 감지
    bool IsGrounded()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        return Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance + 0.1f);
    }
}
