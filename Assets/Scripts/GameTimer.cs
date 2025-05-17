using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameTimer : MonoBehaviour
{
    public float totalTime = 30f;
    private float currentTime;

    public TextMeshProUGUI timerText;
    public GameObject gameOverPanel;

    public PlayerScoreUI[] allPlayers;

    public GameObject winnerPanel;
    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI restartText;

    private bool gameEnded = false;

    void Start()
    {
        currentTime = totalTime;
        gameEnded = false;
        timerText.text = FormatTime(currentTime);
        gameOverPanel.SetActive(false);
        winnerPanel.SetActive(false);
        restartText.text = "";
    }

    void Update()
    {
        if (!gameEnded)
        {
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                if (currentTime < 0) currentTime = 0;
                timerText.text = FormatTime(currentTime);
            }
            else
            {
                gameEnded = true;
                timerText.text = "00:00";
                gameOverPanel.SetActive(true);
                ShowWinner();

                // 스폰 멈추게 만들기
                AppleSpawner spawner = FindObjectOfType<AppleSpawner>();
                if (spawner != null)
                {
                    spawner.isGameOver = true;
                }
            }
        }

        if (gameEnded && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void ShowWinner()
    {
        if (allPlayers.Length == 0) return;

        PlayerScoreUI winner = allPlayers[0];
        foreach (var p in allPlayers)
        {
            if (p.GetScore() > winner.GetScore())
                winner = p;
        }

        winnerPanel.SetActive(true);
        winnerText.text = $" Winner: {winner.playerName}!";
        restartText.text = "Press R to Restart";

        Animator anim = winnerPanel.GetComponent<Animator>();
        if (anim != null)
        {
            anim.Play("WinnerAppear");
        }
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
