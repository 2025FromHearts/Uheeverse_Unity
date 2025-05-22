using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerTest : MonoBehaviour
{
    public float moveSpeed = 5f;
    private CharacterController controller;
    private Vector3 startPosition;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        startPosition = transform.position; // ⭐ 처음 위치 저장
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal"); // A / D
        float v = Input.GetAxis("Vertical");   // W / S

        Vector3 move = Vector3.right * h + Vector3.forward * v;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // ⭐ 이동 후에도 Y값은 항상 고정
        Vector3 fixedPosition = transform.position;
        fixedPosition.y = startPosition.y;
        transform.position = fixedPosition;
    }
}
