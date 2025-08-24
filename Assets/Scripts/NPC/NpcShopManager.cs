using System;
using UnityEngine;
using TMPro;

public class NpcShopManager : MonoBehaviour
{
    public TMP_Text dialogueText;
    public TMP_Text npcNameText;
    public GameObject dialoguePanel;
    public ShopUI shopUI;
    public GameObject shopUIPanel;

    [Tooltip("상점 NPC가 처음 건네는 인사말")]
    public string responseMessage = "물건 구경하고 가세요!";

    private string currentNpcName;
    private bool shopOpen = false;   // 재진입 제어
    private NpcInteract currentNpc;  // ⭐ 말 걸었던 NPC 기억

    private void Awake()
    {
        if (shopUI != null)
            shopUI.OnShopClosed += HandleShopClosed; // 닫힘 콜백 등록
    }

    private void OnDestroy()
    {
        if (shopUI != null)
            shopUI.OnShopClosed -= HandleShopClosed;
    }

    // ⭐ 말 걸었던 NPC를 함께 넘기도록 수정
    public void ShowShopDialogue(string npcName, NpcInteract npc)
    {
        // 이미 열려 있으면 무시 (대화 중/상점 중복 오픈 방지)
        if (shopOpen) return;

        currentNpc = npc;  // ⭐ 저장
        currentNpcName = npcName;
        shopOpen = true; // 열림 표시

        dialoguePanel.SetActive(true);
        if (npcNameText) npcNameText.text = npcName;
        if (dialogueText) dialogueText.text = responseMessage;

        StartCoroutine(ShowShopAfterDelay(1.2f));
    }

    private System.Collections.IEnumerator ShowShopAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (dialoguePanel) dialoguePanel.SetActive(false);

        if (shopUIPanel) shopUIPanel.SetActive(true);
        if (shopUI) shopUI.OpenShop();  // 입력 잠금/커서 처리 등
    }

    // ShopUI에서 닫힐 때 호출
    private void HandleShopClosed()
    {
        if (shopUIPanel) shopUIPanel.SetActive(false);
        if (dialoguePanel) dialoguePanel.SetActive(false);
        shopOpen = false;

        if (currentNpc != null)
        {
            currentNpc.ResetTalkState();  // ⭐ 다시 말 걸 수 있게
            currentNpc = null; // ⭐ 참조 해제
        }
    }
}
