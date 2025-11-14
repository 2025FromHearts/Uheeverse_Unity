using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NpcPhotoManager : MonoBehaviour
{
    [Header("대화 패널")]
    public GameObject dialoguePanel;
    public TMP_Text titleText;
    public TMP_Text bodyText;
    public Button startButton;
    public Button cancelButton;

    [Header("포토 모드")]
    public PhotoModeController photoMode;

    private NpcInteract caller;

    public void ShowPhotoDialogue(string npcName, NpcInteract callerNpc)
    {
        caller = callerNpc;

        dialoguePanel?.SetActive(true);
        titleText.text = npcName;
        bodyText.text = "사진 촬영을 시작할까요?";

        // 버튼 리스너 초기화
        startButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        startButton.onClick.AddListener(() =>
        {
            dialoguePanel?.SetActive(false);

            // 1) 플레이어 찾기
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null)
            {
                Debug.LogWarning("❌ Player 태그가 없습니다.");
                return;
            }

            // 2) character_style 가져오기
            string characterStyle = PlayerPrefs.GetString("character_style", "");
            if (string.IsNullOrEmpty(characterStyle))
                Debug.LogWarning("❌ PlayerPrefs에서 character_style을 찾을 수 없습니다.");

            // 3) 포토 모드 진입
            photoMode.EnterPhotoMode(characterStyle, playerObj);

            // 4) 플레이어 이동
            Transform spawn = photoMode.photoSpawnPoint;
            if (spawn != null)
            {
                var cc = playerObj.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;

                playerObj.transform.position = spawn.position;

                if (cc != null) cc.enabled = true;
            }

            NpcTalkTracker.Instance?.MarkNpcAsTalked();
        });

        cancelButton.onClick.AddListener(() =>
        {
            dialoguePanel?.SetActive(false);
            caller?.ResetTalkState();
        });
    }
}
