using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    private InputActions controls;
    private Vector2 moveInput;         // 키보드/컨트롤러 입력
    private Vector2 joypadInput;       // 조이패드(UDP 등) 입력
    public float moveSpeed = 5f;
    public bool canMove = true;
    public Transform cameraTransform;
    private Animator animator;

    void Awake()
    {
        controls = new InputActions();
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        animator = GetComponent<Animator>();
    }
    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Update()
    {
        if (cameraTransform == null || !canMove) return;

        // 조이패드 입력이 있으면 우선 사용, 없으면 키보드/컨트롤러 입력 사용
        Vector2 totalInput = joypadInput != Vector2.zero ? joypadInput : moveInput;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f; right.y = 0f;
        forward.Normalize(); right.Normalize();

        Vector3 inputDirection = forward * totalInput.y + right * totalInput.x;
        bool isMoving = inputDirection.sqrMagnitude > 0.01f;
        animator.SetBool("IsMove", isMoving);

        if (isMoving)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            transform.position += inputDirection * moveSpeed * Time.deltaTime;
        }
    }

    // 외부(UDP/조이패드)에서 호출: 명령어에 따라 joypadInput 값 변경
    public void SetJoypadInput(string direction)
    {
        switch (direction)
        {
            case "UP": joypadInput = Vector2.up; break;
            case "DOWN": joypadInput = Vector2.down; break;
            case "LEFT": joypadInput = Vector2.left; break;
            case "RIGHT": joypadInput = Vector2.right; break;
            case "STOP": joypadInput = Vector2.zero; break;
                // 필요한 만큼 다른 명령도 추가
        }
    }
}
