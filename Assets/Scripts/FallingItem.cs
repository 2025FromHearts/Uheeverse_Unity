using UnityEngine;

public class FallingItem : MonoBehaviour
{
    public GameObject dustEffect;
    public int scoreValue = 1; // 아이템마다 점수 설정 가능하게

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Basket"))
        {
            // Basket에 있는 PlayerScoreUI 찾기
            PlayerScoreUI playerScore = other.GetComponentInParent<PlayerScoreUI>();
            if (playerScore != null)
            {
                playerScore.AddScore(scoreValue);
            }

            //Destroy(gameObject);
        }

        if (other.CompareTag("Ground"))
        {
            if (dustEffect != null)
                Instantiate(dustEffect, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}
