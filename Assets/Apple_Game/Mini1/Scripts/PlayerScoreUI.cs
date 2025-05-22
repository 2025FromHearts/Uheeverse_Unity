using UnityEngine;
using TMPro;

public class PlayerScoreUI : MonoBehaviour
{
    public static PlayerScoreUI Instance; // 싱글톤 선언

    [Header("UI 설정")]
    [SerializeField] private TextMeshProUGUI scoreText;
    public string playerName = "Player";

    private int score = 0;

    void Awake()
    {
        // 싱글톤 인스턴스 할당
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 필요한 경우 씬 전환 시 유지
        }
        else
        {
            Destroy(gameObject); // 중복 방지
        }
    }

    public int GetScore()
    {
        return score;
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }
}
