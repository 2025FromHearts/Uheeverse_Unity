using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerInputController : MonoBehaviour
{
    private InputActions controls;
    private Vector2 moveInput;
    public float moveSpeed = 5f;
    public bool canMove = true;

    public Transform cameraTransform; // 카메라 방향 따라 이동
    private Animator animator;
    private CharacterController controller;

    private Vector3 velocity;
    public float gravity = -20f;
    public float groundCheckDistance = 0.2f;

    void Awake()
    {
        controls = new InputActions();
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Update()
    {
        // ✅ 채팅창 입력 중 or 대화 중일 때 이동 금지
        if (!canMove)
        {
            animator.SetBool("IsMove", false);
            return;
        }

        // 카메라 기준 전후좌우 방향 계산
        Vector3 fwd = Vector3.forward;
        Vector3 right = Vector3.right;

        if (cameraTransform != null)
        {
            fwd = cameraTransform.forward;
            fwd.y = 0;
            fwd.Normalize();

            right = cameraTransform.right;
            right.y = 0;
            right.Normalize();
        }

        // 입력 방향 벡터 계산
        Vector3 inputDir = (fwd * moveInput.y + right * moveInput.x);
        if (inputDir.sqrMagnitude > 1e-4f)
            inputDir.Normalize();

        // 이동 여부 확인
        bool isMoving = inputDir.sqrMagnitude > 0.01f;
        animator.SetBool("isMove", isMoving);

        // 캐릭터 회전
        if (isMoving)
        {
            var targetRot = Quaternion.LookRotation(inputDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }

        // 지면 감지 및 중력 처리
        bool grounded = controller.isGrounded || GroundCheckSphere();

        if (grounded)
            velocity.y = -2f; // 바닥 스냅
        else
            velocity.y += gravity * Time.deltaTime;

        // 최종 이동 벡터
        Vector3 finalMove = inputDir * moveSpeed + velocity;
        controller.Move(finalMove * Time.deltaTime);
    }

    // ✅ 발 밑에 SphereCast로 지면 판정
    bool GroundCheckSphere()
    {
        float halfH = controller.height * 0.5f;
        float feetOffset = halfH - controller.radius;
        Vector3 feet = transform.position + controller.center + Vector3.down * feetOffset + Vector3.up * 0.05f;

        return Physics.SphereCast(
            feet, controller.radius * 0.95f, Vector3.down,
            out _, 0.15f, ~0, QueryTriggerInteraction.Ignore
        );
    }

    // ✅ Raycast 예비용 (필요 시 사용 가능)
    bool IsGrounded()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        return Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance + 0.1f);
    }
}
