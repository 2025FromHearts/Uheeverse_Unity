using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class ScoreUploader : MonoBehaviour
{
    [Header("서버 설정")]
    public string serverUrl = "http://localhost:8000/map/submit_score/";

    // 버튼 관련 코드 완전히 제거

    [System.Serializable]
    public class ScoreRequest
    {
        public string game;
        public string character;
        public int score;

        public ScoreRequest(string game, string character, int score)
        {
            this.game = game;
            this.character = character;
            this.score = score;
        }
    }

    public IEnumerator SubmitScore()
    {
        if (CharacterManager.Instance == null)
        {
            Debug.LogError("캐릭터 매니저 미초기화");
            yield break;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("게임 매니저 미초기화");
            yield break;
        }

        string token = PlayerPrefs.GetString("access_token", "");
        string characterId = CharacterManager.Instance.character_id;
        string gameId = GameManager.Instance.CurrentGameID;

        int currentScore = 0;
        GameTimer gameTimer = FindAnyObjectByType<GameTimer>();
        if (gameTimer != null)
        {
            currentScore = gameTimer.GetTotalScore();
            Debug.Log($"⚠️ 계산된 스코어: {currentScore}");
        }
        else
        {
            Debug.LogError("게임 타이머를 찾을 수 없음");
            yield break;
        }

        var data = new ScoreRequest(gameId, characterId, currentScore);
        string json = JsonUtility.ToJson(data);
        Debug.Log("전송 JSON: " + json);

        UnityWebRequest request = new UnityWebRequest(serverUrl, "POST");
        byte[] jsonToSend = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        if (request.responseCode == 201 || request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ 점수 전송 성공: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("❌ 점수 전송 실패: " + request.error
                + "\n상태 코드: " + request.responseCode
                + "\n응답: " + request.downloadHandler.text);
        }
    }
}
