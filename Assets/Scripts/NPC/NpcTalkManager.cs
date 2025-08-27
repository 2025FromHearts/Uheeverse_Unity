using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;

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
    private string BASE_URL;
    private string currentNpcId = "";
    private string currentNpcName = "";

    private string initialNpcGreeting = "안녕하세요! 축제와 관련된 궁금한 사항을 말씀해 주세요.";

    [System.Serializable]
    public class FestivalResponse
    {
        public string reply;
        public string info;
    }

    void Start()
    {
        if (sendButton != null)
            sendButton.onClick.AddListener(OnSendMessage);
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseDialogue);
    }

    // NPC 대화 시작(첫 인사 고정)
    public void TalkToNpc(string npcId, string npcName)
    {
        Debug.Log($"🗨️ {npcId}({npcName})에게 대화 요청");
        dialoguePanel.SetActive(true);
        playerController.canMove = false;
        if (npcNameText != null)
            npcNameText.text = npcName;
        if (dialogueText != null)
            dialogueText.text = initialNpcGreeting;   // 처음엔 고정 안내문 표출
        currentNpcId = npcId;
        currentNpcName = npcName;
        // 최초에는 서버 요청하지 않음!
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
        StartCoroutine(SendTalkRequest(currentNpcName, message));
    }

    IEnumerator SendTalkRequest(string festivalName, string message)
    {
        BASE_URL = ServerConfig.baseUrl;
        string url = BASE_URL + "/llm/festival_info/";
        Dictionary<string, string> payload = new Dictionary<string, string>
        {
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
            FestivalResponse res = JsonUtility.FromJson<FestivalResponse>(www.downloadHandler.text);
            Debug.Log($"🧠 GPT 응답: {res.reply}");
            if (dialogueText != null)
            {
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
    [System.Serializable]
    public class JsonWrapper
    {
        public string message;
        public JsonWrapper(Dictionary<string, string> dict)
        {
            message = dict["message"];
        }
    }
}
