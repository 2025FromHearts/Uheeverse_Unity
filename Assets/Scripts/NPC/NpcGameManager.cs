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

    [Tooltip("외부 코드에서 설정됨 (Minigame 타입 NPC가 호출 시 주입)")]
    [HideInInspector] public string minigameSceneName;

    [Tooltip("미니게임 설명 문장들 (랜덤 선택됨)")]
    public string[] promptMessages = {
        "사과를 바구니에 담아보세요! 높은 점수를 노려보세요!",
        "게임을 시작해볼까요? 재미있는 미션이 기다리고 있어요!",
        "집중해서 플레이해보세요. 당신의 반사신경이 중요해요!",
        "축제 한정 미니게임이에요! 한번 도전해보시겠어요?"
    };

    [Tooltip("미니게임 참여 거절 시 보여줄 메시지")]
    public string exitMessage = "괜찮아요. 다음에 또 도전해주세요!";

    private string currentNpcName;

    void Awake()
    {
        buttonO.onClick.RemoveAllListeners();
        buttonO.onClick.AddListener(OnYes);

        buttonX.onClick.RemoveAllListeners();
        buttonX.onClick.AddListener(OnNo);
    }

    public void ShowMinigameDialogue(string npcName, string sceneName)
    {
        currentNpcName = npcName;
        minigameSceneName = sceneName;
        dialoguePanel.SetActive(true);

        if (npcNameText != null)
            npcNameText.text = npcName;

        if (dialogueText != null && promptMessages.Length > 0)
        {
            string selectedPrompt = promptMessages[Random.Range(0, promptMessages.Length)];
            dialogueText.text = selectedPrompt;
        }
    }

    private void OnYes()
    {
        Debug.Log($"✅ {currentNpcName} 미니게임 참여");

        // ✅ 현재 플레이어 위치 저장
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Vector3 pos = player.transform.position;
            Quaternion rot = player.transform.rotation;

            PlayerPrefs.SetFloat("PlayerPosX", pos.x);
            PlayerPrefs.SetFloat("PlayerPosY", pos.y);
            PlayerPrefs.SetFloat("PlayerPosZ", pos.z);
            PlayerPrefs.SetFloat("PlayerRotY", rot.eulerAngles.y);  // 회전값도 저장

            PlayerPrefs.SetInt("ShouldRestorePosition", 1);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogWarning("❌ 'Player' 태그가 있는 오브젝트를 찾지 못했습니다.");
        }

        // ✅ 미니게임 씬으로 이동
        if (!string.IsNullOrEmpty(minigameSceneName))
        {
            SceneManager.LoadScene(minigameSceneName);
        }
        else
        {
            Debug.LogWarning("❌ 미니게임 씬 이름이 설정되지 않았습니다.");
        }
    }

    private void OnNo()
    {
        Debug.Log($"🔚 {currentNpcName} 미니게임 창 닫기");

        if (dialogueText != null)
            dialogueText.text = exitMessage;

        StartCoroutine(ClosePanelAfterDelay(2f));
    }

    private IEnumerator ClosePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        dialoguePanel.SetActive(false);
    }


}
