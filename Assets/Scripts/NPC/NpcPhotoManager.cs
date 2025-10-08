using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NpcPhotoManager : MonoBehaviour
{
    [Header("대화 패널")]
    public GameObject dialoguePanel;     // "사진 촬영을 시작할까요?" 패널
    public TMP_Text titleText;
    public TMP_Text bodyText;
    public Button startButton;
    public Button cancelButton;

    [Header("포토 모드")]
    public PhotoModeController photoMode; // 실제 촬영/카메라 전환 담당

    private NpcInteract caller;

    public void ShowPhotoDialogue(string npcName, NpcInteract callerNpc)
    {
        caller = callerNpc;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (titleText != null) titleText.text = $"{npcName}";
        if (bodyText != null) bodyText.text = "사진 촬영을 시작할까요?";

        // 중복 리스너 제거 후 등록
        startButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        startButton.onClick.AddListener(() =>
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(false);

            // 포토 모드 진입
            photoMode.EnterPhotoMode();

            // 플레이어 위치 이동
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                Transform player = playerObj.transform;
                Transform photoSpawnPoint = photoMode.photoSpawnPoint;

                if (photoSpawnPoint != null)
                {
                    var cc = player.GetComponent<CharacterController>();
                    if (cc != null) cc.enabled = false;

                    player.position = photoSpawnPoint.position;

                    if (cc != null) cc.enabled = true;
                }
                else
                {
                    Debug.LogWarning("❌ photoSpawnPoint가 null입니다.");
                }
            }
            else
            {
                Debug.LogWarning("❌ Player 태그가 있는 오브젝트를 찾을 수 없습니다.");
            }

            NpcTalkTracker.Instance?.MarkNpcAsTalked();
        });

        cancelButton.onClick.AddListener(() =>
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            if (caller != null) caller.ResetTalkState();
        });
    }
}
