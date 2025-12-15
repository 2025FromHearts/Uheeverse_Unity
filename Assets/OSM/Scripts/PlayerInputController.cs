using FishNet.Object;
using UnityEngine;

public class PlayerInputController : NetworkBehaviour
{
    private CharacterController controller;
    public Transform cameraTransform;
    public float moveSpeed = 5f;
    public bool canMove = true;
    public float gravity = -20f;
    public float groundCheckDistance = 0.2f;
    public float sitRange = 2f;
    public Transform[] chairPoints;
    private bool isSitting = false;
    private Transform nearestChair;

    private Vector3 velocity;
    private Vector2 moveInput;

    // 현재 활성화된 캐릭터 애니메이션 핸들러
    private CharacterAnimHandler animHandler;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        
        if (!IsOwner)
        {
            enabled = false;
            return;
        }


        Camera cam = Camera.main;
        if (cam == null) return;

        cam.transform.SetParent(transform);
        cam.transform.localPosition = new Vector3(0f, 3f, -3f);
        cam.transform.localRotation = Quaternion.identity;

        cameraTransform = cam.transform;
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

        Vector3 fwd = cameraTransform.forward;
        fwd.y = 0; fwd.Normalize();

        Vector3 right = cameraTransform.right;
        right.y = 0; right.Normalize();

        Vector3 inputDir = fwd * Input.GetAxis("Vertical") + right * Input.GetAxis("Horizontal");
        bool isMoving = inputDir.sqrMagnitude > 0.01f;
        inputDir.Normalize();

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
