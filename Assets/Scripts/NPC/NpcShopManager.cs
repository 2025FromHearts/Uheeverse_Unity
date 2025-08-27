using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NpcShopManager : MonoBehaviour
{
    public TMP_Text dialogueText;
    public TMP_Text npcNameText;
    public GameObject dialoguePanel;
    public ShopUI shopUI;

    [Tooltip("상점 UI 오브젝트 (활성화/비활성화로 제어)")]
    public GameObject shopUIPanel;

    [Tooltip("상점 NPC가 처음 건네는 인사말")]
    public string responseMessage = "물건 구경하고 가세요!";

    private string currentNpcName;
    private string currentNpcId;

    public void ShowShopDialogue(string npcName)
    {
        currentNpcName = npcName;

        Debug.Log($"[Shop] NPC 이름: {npcName}");
        Debug.Log($"[Shop] 메시지: {responseMessage}");

        dialoguePanel.SetActive(true);

        if (npcNameText != null)
            npcNameText.text = npcName;

        if (dialogueText != null)
            dialogueText.text = responseMessage;

        // npcId 추출 (npcName 기준으로 탐색)
        var foundNpc = GameObject.FindObjectsOfType<NpcInteract>();
        foreach (var npc in foundNpc)
        {
            if (npc.npcName == npcName)
            {
                currentNpcId = npc.npcId;
                break;
            }
        }

        // 1.5초 후 대화창 닫고 상점 열기
        StartCoroutine(ShowShopAfterDelay(1.5f));
    }

    private IEnumerator ShowShopAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        dialoguePanel.SetActive(false);

        NpcTalkTracker.Instance?.MarkNpcAsTalked();

        // 상점 UI 열기
        if (shopUI != null)
        {
            if (shopUIPanel != null)
                shopUIPanel.SetActive(true);

            shopUI.OpenShop();
        }
    }
}
