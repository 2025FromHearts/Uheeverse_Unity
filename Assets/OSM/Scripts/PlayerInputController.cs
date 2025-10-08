using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    private InputActions controls;
    private Vector2 moveInput;
    public float moveSpeed = 5f;
    public bool canMove = true;

    public Transform cameraTransform; // 여기 연결 필요!
    private Animator animator; //animator 추가

    void Awake()
    {
        controls = new InputActions();
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        animator = GetComponent<Animator>(); // Animator 컴포넌트 할당
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Update()
    {
        if (cameraTransform == null || !canMove) return;

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

        if (inputDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

            transform.position += inputDirection * moveSpeed * Time.deltaTime;
        }
    }
}
