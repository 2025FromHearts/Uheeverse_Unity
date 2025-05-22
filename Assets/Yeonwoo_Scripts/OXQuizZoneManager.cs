using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class OXQuizZoneManager : MonoBehaviour
{
    [SerializeField] private Transform quizCenter;
    [SerializeField] private TMP_Text resultText;
    private QuizManager currentQuizManager;
    private string character_id;

    public void StartOXQuiz(string correctAnswer, int quizId, QuizManager quizManager)
    {
        currentQuizManager = quizManager;

        if (CharacterManager.Instance != null && !string.IsNullOrEmpty(CharacterManager.Instance.character_id))
        {
            character_id = CharacterManager.Instance.character_id;
            Debug.Log("✅ 캐릭터 ID 확보: " + character_id);
        }
        else
        {
            Debug.LogError("❌ 캐릭터 ID를 가져올 수 없습니다.");
            return;
        }

        StartCoroutine(JudgeAfterDelay(correctAnswer, quizId));
    }

    private IEnumerator JudgeAfterDelay(string correctAnswer, int quizId)
    {
        yield return new WaitForSeconds(5f);

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogError("❌ 'Player' 태그 오브젝트 없음!");
            yield break;
        }

        Vector3 pos = playerObj.transform.position;
        string choice = pos.x < quizCenter.position.x ? "O" : "X";
        bool isCorrect = (choice == correctAnswer);

        resultText.text = isCorrect
            ? $"정답! 선택: {choice}"
            : $"오답! 선택: {choice} (정답: {correctAnswer})";

        if (!string.IsNullOrEmpty(character_id))
        {
            yield return StartCoroutine(SendResultToServer(character_id, quizId, choice, correctAnswer));
        }
    }

    private IEnumerator SendResultToServer(string characterId, int quizId, string choice, string correctAnswer)
    {
        WWWForm form = new WWWForm();
        form.AddField("character_id", characterId);
        form.AddField("quiz_id", quizId);
        form.AddField("selected", choice);
        form.AddField("correct", correctAnswer);

        string submitUrl = "http://127.0.0.1:8000/users/submit_result/";
        using (UnityWebRequest request = UnityWebRequest.Post(submitUrl, form))
        {
            string token = PlayerPrefs.GetString("access_token", "");
            if (!string.IsNullOrEmpty(token))
            {
                request.SetRequestHeader("Authorization", "Bearer " + token);
            }

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                Debug.Log("✅ 서버 전송 성공");
            else
                Debug.LogError($"❌ 서버 전송 실패: {request.error}");
        }
    }
}
