using UnityEngine;

public class MyBoothInventoryHotkey : MonoBehaviour
{
    public PadInputEventRouter padInput;
    public MyBoothUI boothUI;

    bool opened = false;

    void OnEnable()
    {
        if (padInput != null)
            padInput.OnRPressed += Toggle;
    }

    void OnDisable()
    {
        if (padInput != null)
            padInput.OnRPressed -= Toggle;
    }

    void Toggle()
    {
        // 다른 UI 떠있으면 무시하고 싶으면 유지
        // (MyBoothUI 내부에서만 막고 싶으면 제거)
        // if (UIManager.IsUIBlocking && !opened) return;

        if (!opened)
        {
            opened = true;
            UIManager.IsUIBlocking = true;
            if (padInput != null) padInput.currentMode = PadInputEventRouter.InputMode.Popup;

            boothUI.OpenInventory();
        }
        else
        {
            opened = false;
            boothUI.CloseInventory();

            UIManager.IsUIBlocking = false;
            if (padInput != null) padInput.currentMode = PadInputEventRouter.InputMode.Player;
        }
    }


}
