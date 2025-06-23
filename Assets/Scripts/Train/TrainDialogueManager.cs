using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class TrainDialogueManager : MonoBehaviour
{
    public TMP_Text dialogueText;
    public Button nextButton;
    public QuizManager quizManager;
    public GameObject dialogueCanvas;
    public GameObject quizCanvas;

    private List<string> dialogueLines = new List<string>()
    {
        "안녕하세요!",
        "이 열차는 경상북도 청송군의 \n<청송 사과 축제>로 향하는 열차입니다.",
        "저는 여러분과 함께 이 여정을 안내할 \n열차 안내 도우미입니다!",
        "열차가 축제로 향하는 동안, \n간단한 퀴즈 미션을 통해 코인을 획득할 수 있어요!",
        "코인은 축제장에서 기념품과 교환하거나, \n마이 스테이션에서 캐릭터 아이템을 구입할 수 있답니다.",
        "모두 준비되셨나요? \n그럼, OX 퀴즈로 출발해볼까요?"
    };

    private int currentLineIndex = 0;
    private bool isReadyToStartQuiz = false;

    void Start()
    {
        ShowCurrentLine();
        quizCanvas.SetActive(false);
    }

    public void OnClickNext()
    {
        if (isReadyToStartQuiz)
        {
            nextButton.gameObject.SetActive(false);
            dialogueText.text = "";

            dialogueCanvas.SetActive(false);
            quizCanvas.SetActive(true);

            // 🔽 퀴즈 요청 시작
            StartCoroutine(FetchQuizFromServer("청송"));
            return;
        }

        currentLineIndex++;
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        dialogueText.text = dialogueLines[currentLineIndex];

        if (currentLineIndex == dialogueLines.Count - 1)
        {
            TMP_Text btnText = nextButton.GetComponentInChildren<TMP_Text>();
            if (btnText != null) btnText.text = "시작";

            isReadyToStartQuiz = true;
        }
    }

    IEnumerator FetchQuizFromServer(string region)
    {
        string url = ServerConfig.baseUrl + "/users/generate_quiz/?region=" + region;

        UnityWebRequest www = new UnityWebRequest(url, "POST");
        www.uploadHandler = new UploadHandlerRaw(new byte[0]);  // 바디 없음
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("퀴즈 요청 실패: " + www.error);
            yield break;
        }

        string json = www.downloadHandler.text;
        QuizData quiz = JsonUtility.FromJson<QuizData>(json);
        List<QuizData> quizList = new List<QuizData> { quiz };

        quizManager.StartQuiz(quizList);
    }
}
