using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
    private CharacterController controller;

    [Header("기본 설정")]
    public Transform cameraTransform;
    public float moveSpeed = 5f;
    public bool canMove = true;
    public float gravity = -20f;
    public float groundCheckDistance = 0.2f;
    public float sitRange = 2f;
    public Transform[] chairPoints;
    private bool isSitting = false;
    private Transform nearestChair;

    [Header("외부 입력 (JoyPad 등)")]
    public bool useExternalInput = false;          // true면 조이패드 같은 외부 입력 사용
    [HideInInspector] public Vector2 externalMoveInput = Vector2.zero; // (-1~1, x=좌우, y=앞뒤)

    private Vector3 velocity;

    // 현재 활성화된 캐릭터 애니메이션 핸들러
    private CharacterAnimHandler animHandler;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private Transform GetNearestChair()
    {
        float minDist = Mathf.Infinity;
        Transform nearest = null;

        foreach (var chair in chairPoints)
        {
            if (chair == null) continue;

            float dist = Vector3.Distance(transform.position, chair.position);

            if (dist < minDist)
            {
                minDist = dist;
                nearest = chair;
            }
        }

        return nearest;
    }

    public void SetActiveCharacter(GameObject character)
    {
        animHandler = character.GetComponent<CharacterAnimHandler>();
    }

    void Update()
    {
        HandleSitToggle();

        if (!isSitting)
            HandleMovement();   // 앉아있지 않을 때만 이동 가능
    }

    private void HandleSitToggle()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            // 토글
            if (!isSitting)
                TrySit();
            else
                StandUp();
        }
    }

    private void HandleMovement()
    {
        if (cameraTransform == null) return;
        if (!canMove || controller == null || cameraTransform == null)
            return;

        // -------- 1. 입력값 얻기 (키보드 or 외부 입력) --------
        Vector2 moveInput;

        if (useExternalInput)
        {
            // 조이패드 등에서 넘겨주는 값 사용
            moveInput = externalMoveInput;  // x=좌우, y=앞뒤
        }
        else
        {
            // 기존 키보드 / 패드 입력
            float horizontal = Input.GetAxis("Horizontal");
            float vertical   = Input.GetAxis("Vertical");
            moveInput = new Vector2(horizontal, vertical);
        }

        // 카메라 기준 방향 벡터 계산
        Vector3 fwd = cameraTransform.forward;
        fwd.y = 0f;
        fwd.Normalize();

        Vector3 right = cameraTransform.right;
        right.y = 0f;
        right.Normalize();

        // 입력 방향 (카메라 기준)
        Vector3 inputDir = fwd * moveInput.y + right * moveInput.x;

        bool isMoving = inputDir.sqrMagnitude > 0.01f;
        if (isMoving)
            inputDir.Normalize();

        // -------- 2. 애니메이션 처리 --------
        animHandler?.SetMoveState(isMoving);

        // -------- 3. 회전 처리 --------
        if (isMoving)
        {
            Quaternion targetRot = Quaternion.LookRotation(inputDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }

        // -------- 4. 중력 처리 --------
        bool grounded = controller.isGrounded;
        if (grounded)
            velocity.y = -2f; // 살짝 아래로 눌러붙게
        else
            velocity.y += gravity * Time.deltaTime;

        // -------- 5. 실제 이동 --------
        Vector3 move = inputDir * moveSpeed + velocity;
        controller.Move(move * Time.deltaTime);
    }

    private void TrySit()
    {
        nearestChair = GetNearestChair();
        if (nearestChair == null) return;

        float dist = Vector3.Distance(transform.position, nearestChair.position);
        if (dist > sitRange) return;

        isSitting = true;
        controller.enabled = false;
        transform.position = nearestChair.position;
        transform.rotation = nearestChair.rotation;
        controller.enabled = true;

        animHandler?.SetSitState(true);
    }


    private void StandUp()
    {
        animHandler?.SetSitState(false);
        canMove = true;
        isSitting = false;
    }
}
