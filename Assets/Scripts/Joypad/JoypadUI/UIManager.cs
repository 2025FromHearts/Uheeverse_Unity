using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public PadInputEventRouter padInput;
    public PlayerInputController playerInput;

    [Header("UI")]
    public GameObject inventoryUI;   // R 버튼
    public GameObject uPhoneUI;      // L 버튼

    private Stack<GameObject> uiStack = new Stack<GameObject>();
    public InventoryUI inventoryUIController;
    public static bool IsUIBlocking = false;

    void Start()
    {
        padInput.OnRPressed += () => ToggleUI(inventoryUI);
        padInput.OnLPressed += () => ToggleUI(uPhoneUI);
    }

    void ToggleUI(GameObject ui)
    {
        if (ui == null) return;

        // 1이미 열려 있으면 → 무조건 닫기
        if (ui.activeSelf)
        {
            CloseSpecificUI(ui);
            UpdatePlayerMoveState();
            return;
        }

        // 다른 UI가 열려 있으면 → 새 UI 열기 금지
        if (uiStack.Count > 0)
            return;

        // 열려있지 않고, 스택 비어있으면 → 열기
        OpenUI(ui);
        UpdatePlayerMoveState();
    }


    void OpenUI(GameObject ui)
    {
        ui.SetActive(true);
        uiStack.Push(ui);

        if (padInput != null)
        {
            if (ui == inventoryUI)
                padInput.currentMode = PadInputEventRouter.InputMode.UPhone;
            else if (ui == uPhoneUI)
                padInput.currentMode = PadInputEventRouter.InputMode.UPhone;
        }

        if (ui == inventoryUI && inventoryUIController != null)
        {
            inventoryUIController.OpenInventory();
        }
        UIManager.IsUIBlocking = true;
    }

    void CloseSpecificUI(GameObject ui)
    {
        if (!ui.activeSelf) return;

        EventSystem.current.SetSelectedGameObject(null);

        if (ui == inventoryUI && inventoryUIController != null)
        {
            inventoryUIController.CloseInventory();
        }

        if (ui == uPhoneUI)
        {
            var friendList = ui.GetComponentInChildren<U_FriendList>(true);
            if (friendList != null && friendList.listNavigation != null)
                friendList.listNavigation.ResetToTop();

            var search = ui.GetComponentInChildren<U_SearchFriend>(true);
            if (search != null)
                search.ResetState();
        }

        ui.SetActive(false);

        if (padInput != null)
            padInput.currentMode = PadInputEventRouter.InputMode.Player;

        // 스택 정리
        Stack<GameObject> temp = new Stack<GameObject>();
        while (uiStack.Count > 0)
        {
            GameObject top = uiStack.Pop();
            if (top != ui)
                temp.Push(top);
        }
        while (temp.Count > 0)
            uiStack.Push(temp.Pop());

        UIManager.IsUIBlocking = false;
    }

    void UpdatePlayerMoveState()
    {
        if (playerInput == null) return;

        playerInput.canMove = (uiStack.Count == 0);
    }
}
