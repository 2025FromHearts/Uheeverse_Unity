using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Text;
using Newtonsoft.Json; // TicketReveal과 일관성을 위해 추가

public class NpcTalkManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text dialogueText;
    public TMP_Text npcNameText;
    public GameObject dialoguePanel;
    public TMP_InputField npcInputField;
    public Button sendButton;
    public Button closeButton;
    public PlayerInputController playerController;

    [Header("NPC Info")]
    public string npcId;
    public string npcName;

    private string BASE_URL;
    private string currentNpcId = "";
    private string currentNpcName = "";

    private string initialNpcGreeting = "안녕하세요! 축제와 관련된 궁금한 사항을 말씀해 주세요.";

    [System.Serializable]
    public class FestivalResponse
    {
        public string reply;
        public string info;
        public string intent;
        public string festival;
    }

    [System.Serializable]
    public class TalkPayload
    {
        public string message;
        public string festival_name;
    }

    void Start()
    {
        if (sendButton != null)
            sendButton.onClick.AddListener(OnSendMessage);

        if (closeButton != null)
        {
        }
        closeButton.onClick.AddListener(OnCloseDialogue);
        npcInputField.onSelect.AddListener((_) =>
        {
            if (playerController != null)
                playerController.canMove = false;
        });

        npcInputField.onDeselect.AddListener((_) =>
        {
            if (playerController != null)
                playerController.canMove = true;
        });
    }

    public void TalkToNpc(NpcInteract callerNpc)
    {
        currentNpcId = callerNpc.npcId;
        currentNpcName = callerNpc.npcName;

        dialoguePanel.SetActive(true);

        npcNameText.text = currentNpcName;
        dialogueText.text = initialNpcGreeting;
    }

    public void OnSendMessage()
    {
        if (npcInputField == null || string.IsNullOrWhiteSpace(npcInputField.text))
        {
            Debug.LogWarning("입력한 메시지가 없습니다.");
            return;
        }

        string message = npcInputField.text.Trim();
        npcInputField.text = "";

        if (string.IsNullOrWhiteSpace(currentNpcName))
        {
            Debug.LogError("❌ currentNpcName이 비어 있습니다. TalkToNpc()에서 NPC 이름이 전달되지 않았습니다.");
            return;
        }

        StartCoroutine(SendTalkRequest(currentNpcName, message));
    }

    IEnumerator SendTalkRequest(string npcName, string message)
    {
        BASE_URL = ServerConfig.baseUrl;
        string endpoint;

        if (npcName.Contains("청송"))
        {
            // 서버에 '/llm/tmi_info/' 엔드포인트를 등록해야 합니다.
            endpoint = "/llm/tmi_answer/";
        }
        else
        {
            // 기존 축제 안내 NPC일 경우 기존 API 사용
            endpoint = "/llm/festival_info/";
        }

        string url = BASE_URL + endpoint;

        // 전송할 데이터 구조화
        TalkPayload payload = new TalkPayload
        {
            message = message,
            festival_name = npcName // LLM에게 NPC 이름을 전달하여 역할 설정에 사용
        };

        // JsonUtility 대신 Newtonsoft.Json을 사용하는 것이 더 안전할 수 있지만, 
        // 기존 코드를 유지하고 JsonUtility.ToJson 사용
        string jsonData = JsonUtility.ToJson(payload);
        Debug.Log($"📨 보낸 JSON ({endpoint}): " + jsonData);

        UnityWebRequest www = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        // 에러 처리
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"❌ Talk request failed: {www.responseCode} | {www.error}\n서버응답: {www.downloadHandler.text}");
            if (dialogueText != null)
                dialogueText.text = "서버와 연결할 수 없습니다. 잠시 후 다시 시도해주세요.";
            yield break;
        }

        // 정상 응답 처리
        Debug.Log("✅ Talk request success!");
        Debug.Log("📩 서버 응답: " + www.downloadHandler.text);

        // 응답은 reply, intent 필드를 포함하는 FestivalResponse 구조체를 사용합니다.
        FestivalResponse res = JsonUtility.FromJson<FestivalResponse>(www.downloadHandler.text);
        if (res == null || string.IsNullOrEmpty(res.reply))
        {
            Debug.LogWarning("⚠️ 서버 응답을 파싱할 수 없습니다.");
            if (dialogueText != null)
                dialogueText.text = "정확한 답변을 받지 못했습니다. 다시 시도해주세요.";
            yield break;
        }

        // 자연스러운 줄바꿈 처리
        if (dialogueText != null)
        {
            string[] split = Regex.Split(res.reply, @"(?<=[.?!])\s+");
            StringBuilder formatted = new StringBuilder();
            foreach (string line in split)
            {
                string trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    formatted.AppendLine(trimmed);
            }
            dialogueText.text = formatted.ToString();
        }
    }

    // 대화 종료 버튼
    public void OnCloseDialogue()
    {
        if (dialogueText != null)
            dialogueText.text = "감사합니다. 또 궁금한 게 있으면 물어봐주세요!";
        StartCoroutine(CloseDialogueAfterDelay(1.5f));
        NpcTalkTracker.Instance?.MarkNpcAsTalked(currentNpcId);
    }

    private IEnumerator CloseDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
            playerController.canMove = true;
        }
        currentNpcId = "";
        currentNpcName = "";
    }
}