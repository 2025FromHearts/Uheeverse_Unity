using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Suntail;
using UnityEngine.EventSystems;

public class NpcTalkManager : MonoBehaviour
{
    public TMP_Text dialogueText; 
    public TMP_Text npcNameText; 
    public GameObject dialoguePanel;    
    public TMP_InputField npcInputField;
    public Button sendButton;
    public Button closeButton;
    public PlayerInputController playerController;
    public string npcId;
    public string npcName;
    public NpcTalkManager talkManager;

    private string BASE_URL;
    private string currentNpcId = "";
    private string currentNpcName = "";

    [System.Serializable]
    public class TalkResponse
    {
        public string reply;
    }

    void Start()
    {
        if (sendButton != null)
            sendButton.onClick.AddListener(OnSendMessage);

        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseDialogue);
    }

    // 외부에서 호출되는 대화 시작 함수
    public void TalkToNpc(string npcId, string npcName)
    {
        Debug.Log($"🗨️ {npcId}({npcName})에게 대화 요청");

        dialoguePanel.SetActive(true);
        playerController.canMove = false;

        // 이름 표시
        if (npcNameText != null)
            npcNameText.text = npcName;

        // 대화 내용 초기화
        if (dialogueText != null)
            dialogueText.text = "...";

        // 현재 NPC 정보 저장
        currentNpcId = npcId;
        currentNpcName = npcName;

        // 기본 인사 메시지 전송
        StartCoroutine(SendTalkRequest(npcId, "안녕하세요"));
    }

    // InputField에서 입력한 메시지를 전송
    public void OnSendMessage()
    {
        if (npcInputField == null || string.IsNullOrWhiteSpace(npcInputField.text))
        {
            Debug.LogWarning("입력한 메시지가 없습니다.");
            return;
        }

        string message = npcInputField.text.Trim();
        npcInputField.text = ""; // 입력창 비우기

        // 메시지 서버로 전송
        StartCoroutine(SendTalkRequest(currentNpcId, message));
    }

    // 서버로 메시지를 보내고 응답을 받는 코루틴
    IEnumerator SendTalkRequest(string npcId, string message)
    {
        BASE_URL = ServerConfig.baseUrl;
        string url = BASE_URL + "/map/npc/talk/";

        Dictionary<string, string> payload = new Dictionary<string, string>
    {
        { "npc_id", npcId },
        { "message", message }
    };

        string jsonData = JsonUtility.ToJson(new JsonWrapper(payload));

        UnityWebRequest www = new UnityWebRequest(url, "POST");
        www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Talk request failed: " + www.error);
        }
        else
        {
            TalkResponse res = JsonUtility.FromJson<TalkResponse>(www.downloadHandler.text);
            Debug.Log($"🧠 GPT 응답: {res.reply}");

            if (dialogueText != null)
            {
                // 마침표 기준으로 줄바꿈 처리
                string[] split = Regex.Split(res.reply, @"(?<=[.?!])\s+");
                string formatted = "";
                foreach (string line in split)
                {
                    string trimmed = line.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                        formatted += trimmed + "\n";
                }

                dialogueText.text = formatted;
            }
        }
    }
    public void OnCloseDialogue()
    {
        if (dialogueText != null)
            dialogueText.text = "감사합니다. 또 궁금한 게 있으면 물어봐주세요!";

        StartCoroutine(CloseDialogueAfterDelay(1.5f));
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

        NpcInteract npcInteract = FindObjectOfType<NpcInteract>();
        if (npcInteract != null)
        {
            npcInteract.ResetTalkState(); // isTalking = false
        }
    }

    // JSON 변환용 래퍼 클래스
    [System.Serializable]
    public class JsonWrapper
    {
        public string npc_id;
        public string message;

        public JsonWrapper(Dictionary<string, string> dict)
        {
            npc_id = dict["npc_id"];
            message = dict["message"];
        }
    }
}
