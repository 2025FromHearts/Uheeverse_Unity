using UnityEngine;

public class SceneryMover : MonoBehaviour
{
    public float speed = 5f;
    public float resetZ = -30f;           // Z가 이보다 작아지면...
    public float repeatDistance = 90f;    // 이동한 만큼 다시 뒤로 보내기

    void Update()
    {
        // Z축으로 뒤로 이동
        transform.Translate(Vector3.back * speed * Time.deltaTime);

        // 너무 뒤로 가면 앞으로 다시 보내기
        if (transform.position.z <= resetZ)
        {
            transform.position += new Vector3(0, 0, repeatDistance);
        }
    }
}
