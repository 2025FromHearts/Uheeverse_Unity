using UnityEngine;

public class SpinnerRotation : MonoBehaviour
{
    public float rotationSpeed = 200f;

    void Update()
    {
        transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
    }
}
