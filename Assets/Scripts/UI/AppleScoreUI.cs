using UnityEngine;
using TMPro;
public class AppleScoreUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    private int score = 0;

    void Start()  // 또는 void Awake()
    {
        scoreText.text = "x 0";  // 초기 텍스트 설정
    }

    public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = $"x {score}";
    }
}
