using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class NpcGameManager : MonoBehaviour
{
    public TMP_Text dialogueText;
    public TMP_Text npcNameText;
    public GameObject dialoguePanel;
    public Button buttonO;
    public Button buttonX;

    [Header("Pad Input")]
    public PadInputEventRouter padInput;

    [HideInInspector] public string minigameSceneName;

    public string[] promptMessages = {
        "게임을 시작해볼까요? 재미있는 미션이 기다리고 있어요!",
        "집중해서 플레이해보세요. 당신의 반사신경이 중요해요!",
        "축제 한정 미니게임이에요! 한번 도전해보시겠어요?",
        "한 번 해보면 멈출 수 없어요! 지금 바로 플레이해볼까요?",
    };

    public string exitMessage = "괜찮아요, 다음에 다시 도전해주세요!";

    private string currentNpcName;
    private string currentNpcId;

    void Awake()
    {
        buttonO.onClick.RemoveAllListeners();
        buttonO.onClick.AddListener(OnYes);

        buttonX.onClick.RemoveAllListeners();
        buttonX.onClick.AddListener(OnNo);
    }

    void OnEnable()
    {
        if (padInput == null) return;

        padInput.OnYPressed += OnPadY;
        padInput.OnXPressed += OnPadX;
    }

    void OnDisable()
    {
        if (padInput == null) return;

        padInput.OnYPressed -= OnPadY;
        padInput.OnXPressed -= OnPadX;
        padInput.currentMode = PadInputEventRouter.InputMode.Player;
    }

    void OnPadY()
    {
        if (!dialoguePanel.activeSelf) return;
        OnYes();
    }

    void OnPadX()
    {
        if (!dialoguePanel.activeSelf) return;
        OnNo();
    }

    public void ShowMinigameDialogue(NpcInteract callerNpc)
    {
        currentNpcName = callerNpc.npcName;
        currentNpcId = callerNpc.npcId;
        minigameSceneName = callerNpc.minigameSceneName;

        dialoguePanel.SetActive(true);
        padInput.currentMode = PadInputEventRouter.InputMode.UPhone;

        if (npcNameText != null)
            npcNameText.text = currentNpcName;

        if (dialogueText != null && promptMessages.Length > 0)
        {
            string selectedPrompt = promptMessages[Random.Range(0, promptMessages.Length)];
            dialogueText.text = selectedPrompt;
        }
    }

    void OnYes()
    {
        Debug.Log($"{currentNpcName} 미니게임 참여");
        padInput.currentMode = PadInputEventRouter.InputMode.Player;

        NpcTalkTracker.Instance?.MarkNpcAsTalked(currentNpcId);

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Vector3 pos = player.transform.position;
            Quaternion rot = player.transform.rotation;

            PlayerPrefs.SetFloat("PlayerPosX", pos.x);
            PlayerPrefs.SetFloat("PlayerPosY", pos.y);
            PlayerPrefs.SetFloat("PlayerPosZ", pos.z);
            PlayerPrefs.SetFloat("PlayerRotY", rot.eulerAngles.y);

            PlayerPrefs.SetInt("ShouldRestorePosition", 1);
            PlayerPrefs.Save();
        }

        if (!string.IsNullOrEmpty(minigameSceneName))
            SceneManager.LoadScene(minigameSceneName);
        else
            Debug.LogWarning("❌ 미니게임 씬 이름이 설정되지 않았습니다.");
    }

    void OnNo()
    {
        Debug.Log($"{currentNpcName} 미니게임 거절");

        if (dialogueText != null)
            dialogueText.text = exitMessage;

        StartCoroutine(ClosePanelAfterDelay(1.5f));
    }

    IEnumerator ClosePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        dialoguePanel.SetActive(false);

        padInput.currentMode = PadInputEventRouter.InputMode.Player;
    }
}
