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

    public GameObject shopUIPanel;

    public string responseMessage = "축제만의 아이템들 구경하고 가세요!";

    private string currentNpcName;
    private string currentNpcId;

    public void ShowShopDialogue(NpcInteract callerNpc)
    {
        currentNpcName = callerNpc.npcName;
        currentNpcId = callerNpc.npcId;

        Debug.Log($"[Shop] NPC 이름: {currentNpcName}");
        Debug.Log($"[Shop] 메시지: {responseMessage}");

        dialoguePanel.SetActive(true);

        if (npcNameText != null)
            npcNameText.text = currentNpcName;

        if (dialogueText != null)
            dialogueText.text = responseMessage;

        // 1.5초 후 상점 열기
        StartCoroutine(ShowShopAfterDelay(1.5f));
    }

    private IEnumerator ShowShopAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        dialoguePanel.SetActive(false);

        NpcTalkTracker.Instance?.MarkNpcAsTalked(currentNpcId);

        if (shopUI != null)
        {
            if (shopUIPanel != null)
                shopUIPanel.SetActive(true);

            shopUI.OpenShop();
        }
    }
}
