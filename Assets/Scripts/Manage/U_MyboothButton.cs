using UnityEngine;
using UnityEngine.SceneManagement;

public class U_MyboothButton : MonoBehaviour
{
    public PadInputEventRouter padInput;
    public bool isFocused = true;

    void OnEnable()
    {
        if (padInput != null)
            padInput.OnAPressed += HandlePadA;
    }

    void OnDisable()
    {
        if (padInput != null)
            padInput.OnAPressed -= HandlePadA;
    }

    void HandlePadA()
    {
        if (padInput.currentMode != PadInputEventRouter.InputMode.UPhone)
            return;

        if (!isFocused)
            return;

        Debug.Log("MYBOOTH PAD A RECEIVED");

        SceneManager.LoadScene("MyBooth");
    }
    public void OnPressA()
    {
        if (padInput.currentMode != PadInputEventRouter.InputMode.UPhone)
            return;

        SceneManager.LoadScene("MyBooth");
    }
}