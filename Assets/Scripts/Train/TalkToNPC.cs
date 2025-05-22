using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Text;

public class TalkToNPC : MonoBehaviour
{
    [Header("입력 필드")]
    public TMP_InputField userInputField;

    [Header("출력 텍스트")]
    public TextMeshProUGUI replyText;

    [Header("서버 URL")]
    public string apiUrl = "http://127.0.0.1:8000/api/talk_to_npc/";

    public void OnSendButtonClicked()
    {
        string message = userInputField.text;
        if (!string.IsNullOrEmpty(message))
        {
            StartCoroutine(SendMessageToServer(message));
        }
    }

    IEnumerator SendMessageToServer(string userMessage)
    {
        string jsonBody = "{\"message\": \"" + userMessage + "\"}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        request.method = UnityWebRequest.kHttpVerbPOST;

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("서버 요청 실패: " + request.error);
            replyText.text = "서버에 연결할 수 없습니다.";
        }
        else
        {
            string response = request.downloadHandler.text;
            string reply = ExtractReplyFromJson(response);
            replyText.text = reply;
        }
    }

    string ExtractReplyFromJson(string json)
    {
        int index = json.IndexOf("reply");
        if (index >= 0)
        {
            int start = json.IndexOf(":", index) + 2;
            int end = json.LastIndexOf("\"");
            return json.Substring(start, end - start);
        }
        return "응답 형식 오류";
    }
}
