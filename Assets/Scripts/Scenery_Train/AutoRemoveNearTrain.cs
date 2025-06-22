using UnityEngine;

public class AutoRemoveNearTrain : MonoBehaviour
{
    public Transform trainTransform;
    public float removeDistance = 10f;

    void Update()
    {
        if (trainTransform == null) return;

        float distance = Vector3.Distance(transform.position, trainTransform.position);
        if (distance < removeDistance)
        {
            Destroy(gameObject);
        }
    }
}
