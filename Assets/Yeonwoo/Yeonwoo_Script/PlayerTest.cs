using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerTest : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f; //회전 속도 추가

    private CharacterController controller;
    private Vector3 startPosition;

    private Animator animator; //animator 추가

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>(); // Animator 가져오기
        startPosition = transform.position; // ⭐ 처음 위치 저장
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal"); // A / D
        float v = Input.GetAxis("Vertical");   // W / S

        Vector3 move = Vector3.right * h + Vector3.forward * v;
        move.y = 0; // Y값을 명시적으로 0으로!

        // 애니메이션 전환 조건
        bool isMoving = move.sqrMagnitude > 0.01f;
        animator.SetBool("IsMove", isMoving);

        // 이동 방향 회전 추가
        if (isMoving)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }


        controller.Move(move * moveSpeed * Time.deltaTime);

        //// ⭐ 이동 후에도 Y값은 항상 고정
        //Vector3 fixedPosition = transform.position;
        //fixedPosition.y = startPosition.y;
        //transform.position = fixedPosition;
    }
}
