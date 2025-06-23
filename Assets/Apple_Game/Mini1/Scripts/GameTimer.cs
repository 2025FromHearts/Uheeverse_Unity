using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

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
    public ScoreUploader scoreUploader;
    public Button quitButton;



    private bool gameEnded = false;

    void Start()
    {
        currentTime = totalTime;
        gameEnded = false;
        timerText.text = FormatTime(currentTime);
        gameOverPanel.SetActive(false);
        winnerPanel.SetActive(false);
        restartText.text = "";
        quitButton.onClick.AddListener(QuitGame);
        quitButton.gameObject.SetActive(false); // 게임 시작할 때 꺼두기
    }

    void Update()
    {
        if (!gameEnded)
        {
            currentTime -= Time.deltaTime;
            currentTime = Mathf.Max(currentTime, 0);
            timerText.text = FormatTime(currentTime);

            if (currentTime <= 0)
            {
                gameEnded = true;
                timerText.text = "00:00";
                gameOverPanel.SetActive(true);
                ShowWinner();

                // 최신 Unity API 사용
                AppleSpawner spawner = FindAnyObjectByType<AppleSpawner>();
                if (spawner != null) spawner.isGameOver = true;
            }
        }

        if (gameEnded && Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(RestartGameAfterScoreSubmit());
        }
    }

    private IEnumerator RestartGameAfterScoreSubmit()
    {
        if (scoreUploader != null)
        {
            yield return StartCoroutine(scoreUploader.SubmitScore()); // 점수 전송이 끝날 때까지 대기
        }
        else
        {
            Debug.LogWarning("ScoreUploader가 할당되지 않았습니다.");
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public int GetTotalScore()
    {
        int total = 0;
        foreach (PlayerScoreUI player in allPlayers)
        {
            if (player != null)
            {
                Debug.Log($"GameTimer에서 {player.name} 인스턴스ID:{player.GetInstanceID()}의 GetScore: {player.GetScore()}");
                total += player.GetScore();
            }
            else
            {
                Debug.LogError("GameTimer allPlayers에 null 참조 있음!");
            }
        }
        Debug.Log($"GameTimer 최종 합산 점수: {total}");
        return total;
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
        quitButton.gameObject.SetActive(true); 
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
    private void QuitGame()
    {
        SceneManager.LoadScene("Django_FestivalMainScene");
    }
}
