using UnityEngine;
using Cinemachine;  // CM 2.10.4

public class IntroCutsceneController : MonoBehaviour
{
    [Header("VCams")]
    public CinemachineVirtualCamera introVcam;
    public CinemachineVirtualCamera playerVcam;

    [Header("Dolly (optional)")]
    public CinemachinePathBase path;
    public CinemachineDollyCart cart;
    public float duration = 4f;

    [Header("UI")]
    public CanvasGroup ui;
    public float uiFadeInSeconds = 0.6f;

    [Header("Player Control")]
    public GameObject playerControllerRoot;
    public Behaviour[] extraControlScripts;

    [Header("Skip")]
    public bool canSkip = true;
    public KeyCode skipKey = KeyCode.Space;

    int introPrio = 20, playerPrio = 10;

    void Awake()
    {
        if (ui){ ui.alpha = 0; ui.interactable = false; ui.blocksRaycasts = false; }
        if (playerControllerRoot) playerControllerRoot.SetActive(false);
        if (extraControlScripts != null) foreach (var s in extraControlScripts) if (s) s.enabled = false;

        if (introVcam)  introVcam.Priority  = introPrio;
        if (playerVcam) playerVcam.Priority = playerPrio;

        if (cart) cart.m_Position = 0f;
    }

    void Start(){ StartCoroutine(PlayIntro()); }

    System.Collections.IEnumerator PlayIntro()
    {
        if (cart && path)
        {
            float L = path.PathLength;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                cart.m_Position = Mathf.Lerp(0f, L, t / duration);
                if (IsSkip()) break;
                yield return null;
            }
        }
        else
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                if (IsSkip()) break;
                yield return null;
            }
        }

        if (introVcam)  introVcam.Priority  = playerPrio - 1;
        if (playerVcam) playerVcam.Priority = playerPrio;

        if (playerControllerRoot) playerControllerRoot.SetActive(true);
        if (extraControlScripts != null) foreach (var s in extraControlScripts) if (s) s.enabled = true;

        if (ui) StartCoroutine(FadeUIIn());
    }

    bool IsSkip()
    {
        if (!canSkip) return false;
        if (Input.GetKeyDown(skipKey)) return true;
        return UnityEngine.InputSystem.Keyboard.current != null &&
               UnityEngine.InputSystem.Keyboard.current.anyKey.wasPressedThisFrame;
    }

    System.Collections.IEnumerator FadeUIIn()
    {
        float t = 0f, d = Mathf.Max(0.05f, uiFadeInSeconds);
        while (t < d)
        {
            t += Time.deltaTime;
            ui.alpha = t / d;
            yield return null;
        }
        ui.alpha = 1; ui.interactable = true; ui.blocksRaycasts = true;
    }
}
