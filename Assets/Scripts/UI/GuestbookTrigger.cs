using UnityEngine;

public class GuestbookTrigger : MonoBehaviour
{
    private bool isPlayerNear = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            GuestbookManager.Instance.ShowGuestbookPrompt(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            GuestbookManager.Instance.ShowGuestbookPrompt(false);
        }
    }

    private void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.A))
        {
            GuestbookManager.Instance.OpenGuestbookPanel();
        }
    }
}
