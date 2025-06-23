using UnityEngine;

public class SceneryLooper : MonoBehaviour
{
    public float speed = 5f;              // 이동 속도
    public float loopLength = 90f;        // 반복될 거리

    private Vector3 startPos;             // 시작 위치 저장

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // 뒤로 이동 (Z축 방향)
        transform.Translate(Vector3.back * speed * Time.deltaTime);

        // loopLength만큼 이동하면 처음 위치로 리셋
        if (transform.position.z <= startPos.z - loopLength)
        {
            transform.position = startPos;
        }
    }
}
