using UnityEngine;
using UnityEngine.SceneManagement;

public class Apple : MonoBehaviour
{
    public int scoreValue = 10; // 먹으면 오르는 점수
    public Vector3 rotationSpeed = new Vector3(0, 720, 0); // 초당 720도 회전

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Kart"))
        {
            // UI에 있는 AppleScoreUI 찾아서 점수 추가
            AppleScoreUI ui = FindFirstObjectByType<AppleScoreUI>(); //  여기만 수정됨
            if (ui != null)
            {
                ui.AddScore(1);
            }

            LapCounter lapCounter = FindFirstObjectByType<LapCounter>();
            if (lapCounter != null)
            {
                lapCounter.AddApple();
            }

            Debug.Log("사과 획득!");
            Destroy(gameObject);
        }
    }

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
