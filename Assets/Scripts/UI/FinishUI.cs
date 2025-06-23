using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FinishUI : MonoBehaviour
{
    public GameObject finishPanel;
    public TextMeshProUGUI finishText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI appleCountText;
    public Button restartButton;
    public Button quitButton;
    public GameObject finishTextOnly;

    private void Start()
    {
        finishPanel.SetActive(false);
        finishTextOnly.SetActive(false);

        restartButton.onClick.AddListener(RestartGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    public void ShowFinishPanel(float time, int appleCount)
    {
        StartCoroutine(FinishSequence(time, appleCount));
    }

    private IEnumerator FinishSequence(float time, int appleCount)
    {
        // Finish 텍스트 보여주기
        finishTextOnly.SetActive(true);
        yield return new WaitForSeconds(2f);
        finishTextOnly.SetActive(false);

        // 결과 UI 표시
        timeText.text = $"Time: {time:F2}s";
        appleCountText.text = $"Apples: x{appleCount}";
        finishPanel.SetActive(true);

        KartController kart = FindFirstObjectByType<KartController>();

        if (kart != null)
        {
            kart.StopKartGradually();
        }
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void QuitGame()
    {
        SceneManager.LoadScene("Django_FestivalMainScene");
    }
}