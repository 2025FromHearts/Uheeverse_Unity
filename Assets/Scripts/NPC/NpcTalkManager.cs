using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Text;

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

    // ✅ 서버 응답 구조
    [System.Serializable]
    public class FestivalResponse
    {
        public string reply;
        public string info;
        public string intent;
        public string festival;
    }

    // ✅ 서버 요청 구조
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

    // ✅ NPC 대화 시작 (첫 인사 표시)
    public void TalkToNpc(string npcId, string npcName)
    {
        Debug.Log($"🗨️ {npcId}({npcName})에게 대화 요청");
        dialoguePanel.SetActive(true);
        playerController.canMove = false;

        if (npcNameText != null)
            npcNameText.text = npcName;
        if (dialogueText != null)
            dialogueText.text = initialNpcGreeting;

        currentNpcId = npcId;
        currentNpcName = npcName;
    }

    // ✅ 전송 버튼 눌렀을 때
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

    // ✅ Django 서버와 통신
    IEnumerator SendTalkRequest(string festivalName, string message)
    {
        BASE_URL = ServerConfig.baseUrl;
        string url = BASE_URL + "/llm/festival_info/";

        // 전송할 데이터 구조화
        TalkPayload payload = new TalkPayload
        {
            message = message,
            festival_name = festivalName
        };

        string jsonData = JsonUtility.ToJson(payload);
        Debug.Log("📨 보낸 JSON: " + jsonData);

        UnityWebRequest www = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        // ✅ 에러 처리
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"❌ Talk request failed: {www.responseCode} | {www.error}\n서버응답: {www.downloadHandler.text}");
            if (dialogueText != null)
                dialogueText.text = "서버와 연결할 수 없습니다. 잠시 후 다시 시도해주세요.";
            yield break;
        }

        // ✅ 정상 응답 처리
        Debug.Log("✅ Talk request success!");
        Debug.Log("📩 서버 응답: " + www.downloadHandler.text);

        FestivalResponse res = JsonUtility.FromJson<FestivalResponse>(www.downloadHandler.text);
        if (res == null || string.IsNullOrEmpty(res.reply))
        {
            Debug.LogWarning("⚠️ 서버 응답을 파싱할 수 없습니다.");
            if (dialogueText != null)
                dialogueText.text = "정확한 답변을 받지 못했습니다. 다시 시도해주세요.";
            yield break;
        }

        // ✅ 자연스러운 줄바꿈 처리
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

    // ✅ 대화 종료 버튼
    public void OnCloseDialogue()
    {
        if (dialogueText != null)
            dialogueText.text = "감사합니다. 또 궁금한 게 있으면 물어봐주세요!";
        StartCoroutine(CloseDialogueAfterDelay(1.5f));
        NpcTalkTracker.Instance?.MarkNpcAsTalked();
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
