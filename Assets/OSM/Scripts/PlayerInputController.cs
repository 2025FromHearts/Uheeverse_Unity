using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
    private CharacterController controller;
    public Transform cameraTransform;
    public float moveSpeed = 5f;
    public bool canMove = true;
    public float gravity = -20f;
    public float groundCheckDistance = 0.2f;

    private Vector3 velocity;
    private Vector2 moveInput;

    // 현재 활성화된 캐릭터 애니메이션 핸들러
    private CharacterAnimHandler animHandler;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public void SetActiveCharacter(GameObject character)
    {
        animHandler = character.GetComponent<CharacterAnimHandler>();
    }

    void Update()
    {
        if (!canMove) return;

        Vector3 fwd = cameraTransform.forward;
        fwd.y = 0; fwd.Normalize();
        Vector3 right = cameraTransform.right;
        right.y = 0; right.Normalize();

        Vector3 inputDir = fwd * Input.GetAxis("Vertical") + right * Input.GetAxis("Horizontal");
        bool isMoving = inputDir.sqrMagnitude > 0.01f;
        inputDir.Normalize();

        // 👉 애니메이션은 핸들러에게 위임
        animHandler?.SetMoveState(isMoving);

        if (isMoving)
        {
            Quaternion targetRot = Quaternion.LookRotation(inputDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }

        bool grounded = controller.isGrounded;
        if (grounded)
            velocity.y = -2f;
        else
            velocity.y += gravity * Time.deltaTime;

        controller.Move((inputDir * moveSpeed + velocity) * Time.deltaTime);
    }
}
