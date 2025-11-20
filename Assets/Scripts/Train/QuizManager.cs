using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Networking;

public class QuizManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text questionText;
    public TMP_Text explanationText;
    public OXQuizZoneManager oxQuizZoneManager;

    private List<QuizData> quizList = new List<QuizData>();
    private QuizData currentQuiz;
    private int correctCount = 0;
    private int lastIndex = -1;
    private int currentRound = 0;
    [SerializeField] private int maxRounds = 4;
    [SerializeField] private TMP_Text resultText;

    // 서버에서 받은 퀴즈 리스트 로드

    public void StartQuiz(List<QuizData> data)
    {
        if (data == null || data.Count == 0)
        {
            Debug.LogError("퀴즈 데이터가 비어 있습니다.");
            return;
        }

        quizList = data;
        ShowRandomQuiz();
    }

    // 무작위 퀴즈 1개 출력 (중복 방지)
    public void ShowRandomQuiz()
    {
        if (quizList == null || quizList.Count == 0)
        {
            Debug.LogWarning("퀴즈 리스트가 비어 있음");
            return;
        }

        int index;
        do
        {
            index = Random.Range(0, quizList.Count);
        } while (index == lastIndex && quizList.Count > 1);

        lastIndex = index;
        QuizData quiz = quizList[index];

        questionText.text = quiz.question;
        explanationText.text = ""; // 정답 선택 전까지 해설 숨김

        // 퀴즈 표시 후 OX 판정 시작
        if (oxQuizZoneManager != null)
        {
            oxQuizZoneManager.StartOXQuiz(quiz.answer, quiz.id, this);
        }
        else
        {
            Debug.LogError("OXQuizZoneManager 연결이 안 되어 있음!");
        }
    }

    public void ReceiveAnswerResult(bool isCorrect, string choice)
    {
        Debug.Log(isCorrect ? "정답입니다!" : "오답입니다.");

        if (isCorrect) correctCount++;
        currentRound++;

        // 해설은 텀 주고 보여줌 + 다음 흐름도 거기서 처리
        StartCoroutine(ShowExplanationAfterDelay(1.5f));
    }

    private IEnumerator ShowExplanationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 1. 해설 보여줌 (정답에 대한 해설만 표시)
        if (explanationText != null && currentQuiz != null)
        {
            explanationText.text = currentQuiz.explanation;
        }

        // 2. 해설 감상 시간 확보
        yield return new WaitForSeconds(2f);

        // 3. 마지막 문제일 경우 → 해설 지우고 결과 표시
        if (currentRound >= maxRounds)
        {
            // 해설 지우기
            if (explanationText != null)
            {
                explanationText.text = "";
            }

            // 결과 출력 (다른 텍스트 필드에)
            if (resultText != null)
            {
                resultText.text = $"총 {maxRounds}문제 중 {correctCount}개 정답!\n{correctCount * 100}코인을 드릴게요.";
            }

            StartCoroutine(ShowArrivalMessageAndMoveScene());
        }
        else
        {
            // 다음 퀴즈 요청
            StartCoroutine(RequestQuizWithDelay());
        }
    }



    private IEnumerator RequestQuizWithDelay()
    {
        yield return new WaitForSeconds(2.5f);
        RequestNextQuiz("청송"); // 다음 퀴즈 요청
    }

    private IEnumerator NextQuizWithDelay()
    {
        yield return new WaitForSeconds(3f); //해설 보여준 뒤 대기
        ShowRandomQuiz();
    }

    // 정답 확인 로직
    public void CheckAnswer(string selectedAnswer)
    {
        if (lastIndex < 0 || lastIndex >= quizList.Count)
        {
            Debug.LogError("퀴즈 인덱스 오류");
            return;
        }

        QuizData currentQuiz = quizList[lastIndex];
        bool isCorrect = selectedAnswer == currentQuiz.answer;

        Debug.Log(isCorrect ? "정답입니다!" : "오답입니다.");
        explanationText.text = currentQuiz.explanation;
    }

    public void RequestNextQuiz(string region)
    {
        StartCoroutine(FetchQuizFromServer(region));
    }

    public void ShowQuiz(QuizData quiz)
    {
        if (quiz == null)
        {
            Debug.LogError("퀴즈 데이터 null");
            return;
        }

        currentQuiz = quiz;
        questionText.text = quiz.question;
        explanationText.text = "";

        if (oxQuizZoneManager != null)
        {
            oxQuizZoneManager.StartOXQuiz(quiz.answer, quiz.id, this);
        }
    }

    private IEnumerator FetchQuizFromServer(string region)
    {
        string url = ServerConfig.baseUrl + "/users/generate_quiz/";

        // POST 요청을 위해 form 사용
        WWWForm form = new WWWForm();
        form.AddField("region", region);

        UnityWebRequest www = UnityWebRequest.Post(url, form);  // POST 요청

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("퀴즈 요청 실패: " + www.error);
            yield break;
        }

        string json = www.downloadHandler.text;
        QuizData quiz = JsonUtility.FromJson<QuizData>(json);
        ShowQuiz(quiz);
    }

    private IEnumerator ShowArrivalMessageAndMoveScene()
    {
        yield return new WaitForSeconds(2.5f); // 결과 멘트 잠깐 보여줌

        // 축제장 안내 멘트 UI 표시
        resultText.text = "이제 곧 축제장에 도착합니다! 이동 중...";

        yield return new WaitForSeconds(3f);

        UnityEngine.SceneManagement.SceneManager.LoadScene("Django_FestivalMainScene"); // 축제 맵 씬 이름
    }

    // 다음 퀴즈 요청
    public void NextQuiz()
    {
        ShowRandomQuiz();
    }
}
