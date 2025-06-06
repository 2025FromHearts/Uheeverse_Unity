using UnityEngine;

public class ItemBoxRotator : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(0, 90, 0); // 초당 90도 회전

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
