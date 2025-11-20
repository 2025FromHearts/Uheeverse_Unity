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

    private string currentNpcId;
    private string currentNpcName;

    public void ShowPhotoDialogue(NpcInteract caller)
    {
        currentNpcId = caller.npcId;
        currentNpcName = caller.npcName;

        dialoguePanel?.SetActive(true);
        titleText.text = caller.npcName;
        bodyText.text = "사진 촬영을 시작할까요?";

        startButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        startButton.onClick.AddListener(() =>
        {
            dialoguePanel?.SetActive(false);

            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null)
            {
                Debug.LogWarning("❌ Player 태그가 없습니다.");
                return;
            }

            string characterStyle = PlayerPrefs.GetString("character_style", "");
            if (string.IsNullOrEmpty(characterStyle))
                Debug.LogWarning("❌ PlayerPrefs에서 character_style을 찾을 수 없습니다.");

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
        });

        cancelButton.onClick.AddListener(() =>
        {
            dialoguePanel?.SetActive(false);
            caller?.ResetTalkState();
        });
    }
}
