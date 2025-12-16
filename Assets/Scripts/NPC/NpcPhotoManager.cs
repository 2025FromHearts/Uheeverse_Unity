using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NpcPhotoManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TMP_Text titleText;
    public TMP_Text bodyText;
    public Button startButton;
    public Button cancelButton;

    public PhotoModeController photoMode;
    public PadInputEventRouter padInput;

    private string currentNpcId;
    private NpcInteract currentCaller;

    void OnEnable()
    {
        if (padInput == null) return;

        padInput.OnYPressed += StartPhoto;
        padInput.OnXPressed += CancelPhoto;
    }

    void OnDisable()
    {
        if (padInput == null) return;

        padInput.OnYPressed -= StartPhoto;
        padInput.OnXPressed -= CancelPhoto;
    }

    public void ShowPhotoDialogue(NpcInteract caller)
    {
        currentCaller = caller;
        currentNpcId = caller.npcId;

        dialoguePanel.SetActive(true);

        padInput.currentMode = PadInputEventRouter.InputMode.Dialogue;

        titleText.text = caller.npcName;
        bodyText.text = "사진 촬영을 시작할까요?";

        startButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        startButton.onClick.AddListener(StartPhoto);
        cancelButton.onClick.AddListener(CancelPhoto);
    }

    void StartPhoto()
    {
        if (!dialoguePanel.activeSelf) return;

        dialoguePanel.SetActive(false);

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) return;

        string characterStyle = PlayerPrefs.GetString("character_style", "");
        photoMode.EnterPhotoMode(characterStyle, playerObj);

        Transform spawn = photoMode.photoSpawnPoint;
        if (spawn != null)
        {
            var cc = playerObj.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            playerObj.transform.position = spawn.position;

            if (cc != null) cc.enabled = true;
        }

        NpcTalkTracker.Instance?.MarkNpcAsTalked(currentNpcId);
    }
    void CancelPhoto()
    {
        if (!dialoguePanel.activeSelf) return;

        dialoguePanel.SetActive(false);
        currentCaller?.ResetTalkState();
        padInput.currentMode = PadInputEventRouter.InputMode.Player;
    }
}
