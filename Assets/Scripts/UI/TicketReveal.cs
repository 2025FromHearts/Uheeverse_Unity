using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class TicketReveal : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup alertCg;          // 알람 패널(CanvasGroup)
    public TextMeshProUGUI alertText;    // 알람 텍스트 (TMP)

    [Header("Timing")]
    public float fadeTime = 0.2f;   // 페이드 인/아웃 시간
    public float showTime = 2f;     // 표시 유지 시간

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

        // 캔버스 활성화
        alertCg.gameObject.SetActive(true);

        // Fade In
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeTime;
            alertCg.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
        alertCg.alpha = 1f;

        // 유지
        yield return new WaitForSeconds(showTime);

        // Fade Out
        t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeTime;
            alertCg.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }
        alertCg.alpha = 0f;

        // 사라지면 비활성화
        alertCg.gameObject.SetActive(false);
    }

    public void OnDialogueCompleted(string festivalId)
    {
        StartCoroutine(TryIssueTicket(festivalId));
    }

    IEnumerator TryIssueTicket(string festivalId)
    {
        string url = $"{ServerConfig.baseUrl}/tickets/issue/";
        WWWForm form = new WWWForm();
        form.AddField("map_id", festivalId);
        form.AddField("character_id", PlayerPrefs.GetString("characterId")); // 필요하다면 포함

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        www.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("accessToken"));

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ 티켓 발급 실패: " + www.error);
            yield break;
        }

        string json = www.downloadHandler.text;
        Debug.Log($"✅ 티켓 발급 응답: {json}");

        // "이미 발급된 티켓입니다." 처리
        if (json.Contains("이미 발급된 티켓"))
        {
            ShowAlert("이미 발급된 티켓입니다!");
            yield break;
        }

        try
        {
            // JSON → TicketData 역직렬화
            TicketUIManager.TicketData newTicket =
                JsonConvert.DeserializeObject<TicketUIManager.TicketData>(json);

            // UI에 티켓 추가
            var uiManager = FindObjectOfType<TicketUIManager>();
            if (uiManager != null)
            {
                uiManager.AddTicket(newTicket);
                ShowAlert("티켓이 발급되었습니다!");
            }
            else
            {
                Debug.LogWarning("⚠️ TicketUIManager를 찾을 수 없음");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ 티켓 응답 파싱 실패: " + e.Message);
        }
    }
}
