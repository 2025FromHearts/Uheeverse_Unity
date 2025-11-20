using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class TicketReveal : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup alertCg;
    public TextMeshProUGUI alertText;

    [Header("Timing")]
    public float fadeTime = 0.2f;
    public float showTime = 2f;

    Coroutine _running;

    void Awake()
    {
        if (alertCg)
        {
            alertCg.alpha = 0f;
            alertCg.gameObject.SetActive(false);
        }
    }

    public void ShowAlert(string message = "티켓이 발급되었습니다!")
    {
        if (_running != null) StopCoroutine(_running);
        _running = StartCoroutine(CoShowAlert(message));
    }

    IEnumerator CoShowAlert(string message)
    {
        if (alertText) alertText.text = message;

        alertCg.gameObject.SetActive(true);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeTime;
            alertCg.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        yield return new WaitForSeconds(showTime);

        t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeTime;
            alertCg.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        alertCg.alpha = 0f;
        alertCg.gameObject.SetActive(false);
    }

    public void OnDialogueCompleted(string unused)
    {
        StartCoroutine(TryIssueTicket());
    }

    IEnumerator TryIssueTicket()
    {
        string mapId = "a62122b8-7d5d-45cf-a660-ddc75a30dfc28";
        string url = $"{ServerConfig.baseUrl}/tickets/issue/";

        var payload = new { map_id = mapId };
        string jsonToSend = JsonConvert.SerializeObject(payload);

        UnityWebRequest www = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonToSend);

        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();

        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("access_token"));

        yield return www.SendWebRequest();

        string response = www.downloadHandler.text;
        Debug.Log("응답: " + response);

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ 티켓 발급 실패: " + www.error);
            Debug.LogError("서버 응답 본문: " + response);
            yield break;
        }

        // 이미 발급된 티켓
        if (response.Contains("이미 발급된 티켓"))
        {
            ShowAlert("이미 발급된 티켓입니다!");
            yield break;
        }

        // 성공적으로 새 티켓이 생성됨
        try
        {
            TicketUIManager.TicketData newTicket =
                JsonConvert.DeserializeObject<TicketUIManager.TicketData>(response);

            var uiManager = FindObjectOfType<TicketUIManager>();
            if (uiManager != null)
            {
                uiManager.AddTicket(newTicket);
                ShowAlert("티켓이 발급되었습니다!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ 티켓 응답 파싱 실패: " + e.Message);
        }
    }
}
