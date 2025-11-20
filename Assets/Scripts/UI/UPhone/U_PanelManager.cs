using System.Collections;
using UnityEngine;

public class U_PanelManager : MonoBehaviour
{
    [Header("ÆË¾÷ Canvas/Panel")]
    public GameObject popupPanel;

    void Start()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }

    public void OpenPopup()
    {
        popupPanel.SetActive(true);
    }

    // Ãë¼Ò ¹öÆ° ¡æ ÆÐ³Î ²ô±â
    public void Cancel()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }

    public void Confirm()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }
}
