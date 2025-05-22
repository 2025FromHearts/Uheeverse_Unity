using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class APIManager : MonoBehaviour
{
    // 게임 세션 생성
    public IEnumerator CreateGameSession(string gameId, string mapId)
    {
        string url = "http://localhost:8000/map/create_session/";

        var requestBody = new
        {
            game_id = gameId,
            map_id = mapId
        };

        string jsonData = JsonUtility.ToJson(requestBody); // 단순한 구조일 때만 사용 가능

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new UTF8Encoding().GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Game session created: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error: " + request.error + " - " + request.downloadHandler.text);
        }
    }
}
