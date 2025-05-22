using UnityEngine;
using UnityEngine.InputSystem;

public class Character : MonoBehaviour
{
    public float rotationSpeed = 100f;
    private Vector3 lastMousePosition;
    private bool isDragging = false;

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isDragging = true;
            lastMousePosition = Mouse.current.position.ReadValue();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector2 currentPos = Mouse.current.position.ReadValue();
            Vector2 delta = currentPos - (Vector2)lastMousePosition;
            float rotX = delta.x * rotationSpeed * Time.deltaTime;

            transform.Rotate(Vector3.up, -rotX, Space.Self);
            lastMousePosition = currentPos;
        }
    }
}
