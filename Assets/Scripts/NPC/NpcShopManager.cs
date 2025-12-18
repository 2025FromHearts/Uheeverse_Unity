using UnityEngine;
using TMPro;

public class NpcShopManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    public ShopUI shopUI;
    public GameObject shopUIPanel;

    //public PadInputEventRouter padInput;

    private string currentNpcId;
    private NpcInteract currentCaller;

    /*void OnEnable()
    {
        if (padInput == null) return;

        padInput.OnAPressed += OpenShop;
        padInput.OnXPressed += CancelShop;
    }

    void OnDisable()
    {
        if (padInput == null) return;

        padInput.OnAPressed -= OpenShop;
        padInput.OnXPressed -= CancelShop;
    }*/

    public void ShowShopDialogue(NpcInteract caller)
    {
        currentCaller = caller;
        currentNpcId = caller.npcId;

        dialoguePanel.SetActive(true);

        //padInput.currentMode = PadInputEventRouter.InputMode.Dialogue;

        if (dialogueText != null)
            dialogueText.text = "축제 전용 아이템을 구경해볼까요?";

        OpenShop();
    }

    void OpenShop()
    {
        if (!dialoguePanel.activeSelf) return;

        dialoguePanel.SetActive(false);

        if (shopUIPanel != null)
            shopUIPanel.SetActive(true);

        if (shopUI != null)
            shopUI.OpenShop();

        //padInput.currentMode = PadInputEventRouter.InputMode.Popup;

        NpcTalkTracker.Instance?.MarkNpcAsTalked(currentNpcId);
    }

    void CancelShop()
    {
        if (!dialoguePanel.activeSelf) return;

        dialoguePanel.SetActive(false);
        currentCaller?.ResetTalkState();
        //padInput.currentMode = PadInputEventRouter.InputMode.Player;
    }
}