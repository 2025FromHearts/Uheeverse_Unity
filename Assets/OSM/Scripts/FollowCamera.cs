using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 3, -5);     // 위에서 뒤로 보는 시점
    public float followSpeed = 5f;
    public float rotationSpeed = 5f;

    // 🔒 고정하고 싶은 X축 회전 값 (예: 10도 ~ 20도 정도)
    public float fixedXRotation = 10f;

    void LateUpdate()
    {
        if (!target) return;

        // 위치 따라가기 (회전된 방향 고려해서 뒤에 위치)
        Vector3 desiredPosition = target.position + target.rotation * offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // 회전 따라가기 (Y축만 대상 따라가고, X는 고정)
        Quaternion desiredRotation = Quaternion.Euler(fixedXRotation, target.eulerAngles.y, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
    }
}
