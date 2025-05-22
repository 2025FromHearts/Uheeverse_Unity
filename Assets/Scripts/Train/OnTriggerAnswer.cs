using UnityEngine;


public class OnTriggerAnswer : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("OZone"))
        {
            FindFirstObjectByType<QuizManager>().CheckAnswer("O");
        }
        else if (other.CompareTag("XZone"))
        {
            FindFirstObjectByType<QuizManager>().CheckAnswer("1");
        }
    }
}
