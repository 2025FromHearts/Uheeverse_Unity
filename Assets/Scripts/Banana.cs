using UnityEngine;

public class Banana : MonoBehaviour
{
    public float lifeTime = 5f;
    public float rotateSpeed = 180f;
    public bool isOrbiting = false;

    void Start()
    {
        if (!isOrbiting)
        {
            Destroy(gameObject, lifeTime);
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Kart"))
        {
            if (!isOrbiting)
            {
                Debug.Log("¹Ù³ª³ª¿¡ ºÎµúÈû!");
                Destroy(gameObject);
            }
        }
    }
}
