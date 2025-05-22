using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerMove : MonoBehaviour
{
    private Rigidbody rigidbody;                   // 플레이어 이동용 Rigidbody
    public float speed = 10f;                      // 이동 속도
    //public float jumpHeight = 3f;                  // 점프 높이
    public float rotSpeed = 3f;                    // 회전 속도
    private Vector3 dir = Vector3.zero;            // 이동 방향 저장

    private bool ground = false;                   // 바닥 여부
    public LayerMask layer;                        // 바닥 레이어

    public List<Transform> manualApplePositions = new List<Transform>(); // 사과 쌓일 위치 리스트
    private int stackIndex = 0;                    // 현재 몇 번째 위치에 쌓고 있는지 추적
    public int score = 0;                          // 점수

    private bool controlsInverted = false; // 반전 상태 여부
    private float invertDuration = 5f;     // 반전 지속 시간
    private Coroutine invertCoroutine = null;


    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");

        // 🧠 반전 처리
        if (controlsInverted)
        {
            inputX = -inputX;
            inputZ = -inputZ;
        }

        dir.x = inputX;
        dir.z = inputZ;
        dir.Normalize();

        CheckGround();
    }

    private void FixedUpdate()
    {
        if (dir != Vector3.zero)
        {
            transform.forward = Vector3.Lerp(transform.forward, dir, rotSpeed * Time.deltaTime);
        }

        rigidbody.MovePosition(transform.position + dir * speed * Time.deltaTime);
    }

    void CheckGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out hit, 0.4f, layer))
        {
            ground = true;
        }
        else
        {
            ground = false;
        }
    }
    void RemoveLastApple()
    {
        if (stackIndex > 0)
        {
            stackIndex--; // 하나 제거

            foreach (Transform child in transform)
            {
                AppleTag tag = child.GetComponent<AppleTag>();
                if (tag != null && tag.stackIndex == stackIndex)
                {
                    Destroy(child.gameObject); // 🎯 해당 사과 제거
                    score = Mathf.Max(0, score - 1);
                    Debug.Log($"❌ 바나나 맞음! 사과 {stackIndex + 1} 제거됨");
                    return;
                }
            }

            Debug.Log("❌ 사과 못 찾음! 위치는 존재하지만 사과는 못 찾음");
        }
        else
        {
            Debug.Log("🍌 바나나 맞았지만 사과 없음!");
        }
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Apple"))
        {
            // ✅ 중복 수거 방지
            if (other.GetComponent<AppleTag>()?.isCollected == true) return;

            // ✅ AppleTag 없으면 붙여주기
            AppleTag tag = other.GetComponent<AppleTag>();
            if (tag == null) tag = other.gameObject.AddComponent<AppleTag>();
            tag.isCollected = true;

            // ✅ 점수 증가
            score++;

            // ✅ 물리 꺼주기
            Rigidbody appleRb = other.GetComponent<Rigidbody>();
            if (appleRb != null)
            {
                appleRb.isKinematic = true;
                appleRb.linearVelocity = Vector3.zero;
                appleRb.angularVelocity = Vector3.zero;
            }

            // ✅ 위치 남아있으면 붙이고, 없으면 파괴
            if (stackIndex < manualApplePositions.Count)
            {
                Transform target = manualApplePositions[stackIndex];
                other.transform.SetParent(transform);

                other.transform.position = target.position; 
                other.transform.localRotation = Quaternion.Euler(
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f)
                );

                // ✅ stackIndex 저장해두기
                tag.stackIndex = stackIndex;

                Debug.Log($"사과 {stackIndex + 1}번째 위치에 놓음");
                stackIndex++;
            }

            else
            {
                Destroy(other.gameObject);
                Debug.Log("사과 위치 다 찼음 → 파괴");
            }
        }
        else if (other.CompareTag("Banana"))
        {
            // ✅ 바나나 충돌 시 사과 제거
            RemoveLastApple();
            Destroy(other.gameObject); // 바나나는 파괴
        }
        else if (other.CompareTag("Bomb"))
        {
            Debug.Log("💣 썩은 사과 맞음! 방향키 반전 시작");
            if (invertCoroutine != null)
                StopCoroutine(invertCoroutine);
            invertCoroutine = StartCoroutine(InvertControls());
            Destroy(other.gameObject);
        }
    }
    // 🧠 방향 반전 코루틴
    IEnumerator InvertControls()
    {
        controlsInverted = true;
        yield return new WaitForSeconds(invertDuration);
        controlsInverted = false;
        Debug.Log("↩️ 방향키 정상 복구됨");
    }
}
