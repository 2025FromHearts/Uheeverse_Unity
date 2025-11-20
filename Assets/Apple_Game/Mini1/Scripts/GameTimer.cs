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
        quitButton.gameObject.SetActive(false);
        quitButton.onClick.AddListener(QuitGame);
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
            yield return StartCoroutine(scoreUploader.SubmitScore());
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
        Debug.Log("ShowWinner 메서드 호출됨");

        // allPlayers 배열 검증
        if (allPlayers == null || allPlayers.Length == 0)
        {
            Debug.LogError("allPlayers 배열이 null이거나 비어 있습니다.");
            // 그래도 UI는 표시하자
            ShowFallbackUI();
            return;
        }

        // 유효한 플레이어 찾기
        PlayerScoreUI winner = null;
        int highestScore = -1;

        foreach (var player in allPlayers)
        {
            if (player == null)
            {
                Debug.LogWarning("allPlayers에 null 플레이어가 있습니다.");
                continue;
            }

            try
            {
                int playerScore = player.GetScore();
                Debug.Log($"플레이어 {player.playerName}의 점수: {playerScore}");

                if (playerScore > highestScore)
                {
                    highestScore = playerScore;
                    winner = player;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"플레이어 {player.name}의 점수를 가져오는 중 오류: {e.Message}");
            }
        }

        // UI 표시
        if (winner != null)
        {
            Debug.Log($"승자: {winner.playerName}, 점수: {highestScore}");
            winnerPanel.SetActive(true);
            winnerText.text = $"Winner: {winner.playerName}!";
        }
        else
        {
            Debug.LogWarning("승자를 찾을 수 없습니다. 기본 UI를 표시합니다.");
            ShowFallbackUI();
        }

        // 항상 restart 텍스트와 quit 버튼은 표시
        restartText.text = "Press R to Restart";
        quitButton.gameObject.SetActive(true);

        Debug.Log("ShowWinner 완료 - UI 표시됨");
    }

    void ShowFallbackUI()
    {
        Debug.Log("ShowFallbackUI 호출됨");
        winnerPanel.SetActive(true);
        if (winnerText != null)
            winnerText.text = "Game Over!";
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
        SceneManager.LoadScene("Django_FestivalMainScene", LoadSceneMode.Single);
    }
}