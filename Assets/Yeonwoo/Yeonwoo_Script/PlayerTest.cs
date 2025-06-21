using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerTest : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float gravity = -9.81f;

    [Header("Sit Settings")]
    public Transform[] sitPoints; // 의자 위치들
    public float sitMoveSpeed = 3f;
    public float sitRotationSpeed = 8f; // 앉을 때 회전 속도

    private CharacterController controller;
    private Animator animator;
    private float verticalVelocity = 0f;
    private bool isSitting = false;
    private bool isMovingToSit = false; // 이동 중인지 체크하는 변수 추가
    private Transform currentSitPoint;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isSitting)
        {
            controller.Move(Vector3.down * Time.deltaTime); // 앉아있을 때는 그냥 중력만 적용

            // 일어서기 (다시 I키 누르면)
            if (Input.GetKeyDown(KeyCode.I))
            {
                isSitting = false;
                animator.SetBool("IsSit", false);
            }
            return;
        }

        // 앉으러 이동 중일 때는 입력 무시
        if (isMovingToSit)
        {
            // 이동 중에도 중력 적용
            if (controller.isGrounded)
            {
                verticalVelocity = -1f;
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }
            controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
            return;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 moveInput = new Vector3(h, 0f, v);
        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        animator.SetBool("IsMove", isMoving);

        if (isMoving)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveInput);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // 중력 처리
        if (controller.isGrounded)
        {
            verticalVelocity = -1f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        // 이동 처리
        Vector3 move = moveInput.normalized * moveSpeed;
        move.y = verticalVelocity;
        controller.Move(move * Time.deltaTime);

        // I 키로 가장 가까운 좌석으로 이동 + 앉기
        if (Input.GetKeyDown(KeyCode.I))
        {
            Transform closest = FindClosestSitPoint();
            if (closest != null)
            {
                StartCoroutine(MoveAndSit(closest));
            }
        }
    }

    // 가장 가까운 SitPoint 찾기
    private Transform FindClosestSitPoint()
    {
        if (sitPoints == null || sitPoints.Length == 0)
        {
            return null;
        }

        float minDist = float.MaxValue;
        Transform closest = null;

        foreach (var sp in sitPoints)
        {
            if (sp == null)
            {
                continue;
            }

            float dist = Vector3.Distance(transform.position, sp.position);


            if (dist < minDist)
            {
                minDist = dist;
                closest = sp;
            }
        }


        return closest;
    }

    // 이동 후 앉는 코루틴
    private IEnumerator MoveAndSit(Transform targetSitPoint)
    {

        isMovingToSit = true;

        animator.SetBool("IsSit", true);

        Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward);
        float moveStartTime = Time.time;

        while (Vector3.Distance(transform.position, targetSitPoint.position) > 0.05f)
        {
            if (Time.time - moveStartTime > 10f)
            {
                break;
            }

            // 이동 계산
            Vector3 dir = targetSitPoint.position - transform.position;
            dir.y = 0f;
            Vector3 move = dir.normalized * sitMoveSpeed;

            // 중력 적용
            verticalVelocity = controller.isGrounded ? -1f : verticalVelocity + gravity * Time.deltaTime;
            move.y = verticalVelocity;

            controller.Move(move * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, sitRotationSpeed * Time.deltaTime);

            yield return null;
        }

        // 최종 위치/회전 고정
        transform.position = targetSitPoint.position;
        transform.rotation = targetRotation;

        // 상태 전환 (중요: 이동 상태 먼저 해제)
        isMovingToSit = false;
        isSitting = true;

        // 애니메이션 전환 (즉시 적용)
        animator.SetBool("IsMove", false);
        animator.SetBool("IsSit", true);

        // 애니메이터 강제 업데이트
        animator.Update(0f);

    }
}