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
    private bool isMovingToSit = false; // 이동 중인지 체크하는 변수
    private Transform currentSitPoint;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 1. 앉은 상태일 때 (정지 + 일어나기 가능)
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

        // I 키 처리 - 중복 제거하고 하나로 통합!
        if (Input.GetKeyDown(KeyCode.I))
        {
            Transform closest = FindClosestSitPoint();
            if (closest != null)
            {
                StartCoroutine(QuickMoveAndSit(closest));
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
    private IEnumerator QuickMoveAndSit(Transform targetSitPoint)
    {
        isMovingToSit = true;

        // 즉시 앉기 애니메이션 시작
        animator.SetBool("IsSit", true);

        // 빠르게 이동 (0.2초 정도)
        float duration = 0.2f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.LookRotation(Vector3.forward);

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float progress = t / duration;
            transform.position = Vector3.Lerp(startPos, targetSitPoint.position, progress);
            transform.rotation = Quaternion.Lerp(startRot, targetRot, progress);
            yield return null;
        }

        // 정확한 위치로 고정
        transform.position = targetSitPoint.position;
        transform.rotation = targetRot;

        // 상태 변경
        isMovingToSit = false;
        isSitting = true;
        animator.SetBool("IsMove", false);
    }
}