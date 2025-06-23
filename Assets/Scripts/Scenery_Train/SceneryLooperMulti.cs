using UnityEngine;

public class SceneryLooperMulti : MonoBehaviour
{
    public float speed = 5f;
    public float loopLength = 90f; // 풍경 한 덩어리의 Z 길이

    void Update()
    {
        // 전체를 뒤로 이동
        transform.Translate(Vector3.back * speed * Time.deltaTime);

        // 만약 너무 뒤로 갔으면 다시 앞으로
        if (transform.position.z <= -loopLength)
        {
            transform.position += new Vector3(0f, 0f, loopLength * 2f); 
        }
    }
}
