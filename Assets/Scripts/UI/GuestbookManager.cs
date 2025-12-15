using UnityEngine;

public class GuestbookManager : MonoBehaviour
{
    public static GuestbookManager Instance;

    public GameObject guestbookPrompt;
    public GameObject guestbookPanel;

    private void Awake()
    {
        Instance = this;
        guestbookPrompt.SetActive(false);
        guestbookPanel.SetActive(false);
    }

    public void ShowGuestbookPrompt(bool show)
    {
        guestbookPrompt.SetActive(show);
    }

    public void OpenGuestbookPanel()
    {
        guestbookPrompt.SetActive(false);
        guestbookPanel.SetActive(true);
    }

    public void CloseGuestbookPanel()
    {
        guestbookPanel.SetActive(false);
    }
}
