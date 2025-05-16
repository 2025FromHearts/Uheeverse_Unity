using UnityEngine;
using TMPro;

public class PlayerScoreUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public string playerName = "Player"; // Inspector에서 이름 설정
    private int score = 0;

    public int GetScore()
    {
        return score;
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScore();
    }

    public void SetScore(int value)
    {
        score = value;
        UpdateScore();
    }

    private void UpdateScore()
    {
        scoreText.text = score.ToString();
    }
}
