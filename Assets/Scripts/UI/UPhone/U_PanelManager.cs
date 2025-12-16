using System;
using System.Collections;
using UnityEngine;

public class U_PanelManager : MonoBehaviour
{
    [Header("ÆË¾÷ Canvas/Panel")]
    public GameObject popupPanel;

    public PadInputEventRouter padInput;
    private Action onConfirm;

    void Start()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }

    public void OpenPopup(Action confirmAction)
    {
        Debug.Log("POPUP OPENED");

        popupPanel.SetActive(true);
        onConfirm = confirmAction;

        if (padInput != null)
            padInput.currentMode = PadInputEventRouter.InputMode.Popup;
    }

    void OnEnable()
    {
        if (padInput == null) return;

        padInput.OnAPressed += OnAPressed;
        padInput.OnBPressed += OnBPressed;
    }

    void OnDisable()
    {
        if (padInput == null) return;

        padInput.OnAPressed -= OnAPressed;
        padInput.OnBPressed -= OnBPressed;
    }

    void OnAPressed()
    {
        Debug.Log("POPUP A PRESSED");

        if (!popupPanel.activeInHierarchy)
        {
            Debug.Log("BUT POPUP IS NOT ACTIVE");
            return;
        }

        Debug.Log("CALLING CONFIRM ACTION");
        onConfirm?.Invoke();
    }


    void OnBPressed()
    {
        if (!popupPanel.activeInHierarchy) return;

        Cancel();
    }

    // Ãë¼Ò ¹öÆ° ¡æ ÆÐ³Î ²ô±â
    public void Cancel()
    {
        popupPanel.SetActive(false);

        if (padInput != null)
            padInput.currentMode = PadInputEventRouter.InputMode.UPhone;
    }

    public void Confirm()
    {
        popupPanel.SetActive(false);

        if (padInput != null)
            padInput.currentMode = PadInputEventRouter.InputMode.UPhone;
    }
}
