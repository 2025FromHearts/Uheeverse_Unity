using UnityEngine;
using System.Collections;

public class AppleSelectManager : MonoBehaviour
{
    [Header("Input")]
    public UdpPadReceiver pad;
    public PadInputEventRouter padInput;

    [Header("Apple Items")]
    public ApplePickItem[] apples;

    [Header("Input Settings")]
    public float inputThreshold = 0.6f;
    public float inputDelay = 0.25f;

    [Header("Fade In")]
    public CanvasGroup canvasGroup;
    public float fadeDuration = 0.4f;

    [Header("TMI")]
    public LocalTmiManager tmiManager;

    int currentIndex = 0;
    float lastInputTime;
    bool canInput = false;

    void OnEnable()
    {
        lastInputTime = 0f;
        currentIndex = 0;

        for (int i = 0; i < apples.Length; i++)
            apples[i].Init(i, this);

        UpdateHighlight();
        StartCoroutine(FadeIn());

        if (padInput != null)
            padInput.OnAPressed += SelectCurrent;
    }

    void OnDisable()
    {
        UIManager.IsUIBlocking = false;
    }

    IEnumerator FadeIn()
    {
        if (canvasGroup == null)
        {
            canInput = true;
            yield break;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        canInput = true;
    }

    void Update()
    {
        if (!canInput) return;
        if (pad == null || pad.latest == null) return;
        if (Time.unscaledTime - lastInputTime < inputDelay) return;

        float x = pad.latest.lx;

        if (x > inputThreshold)
        {
            Move(1);
            lastInputTime = Time.unscaledTime;
        }
        else if (x < -inputThreshold)
        {
            Move(-1);
            lastInputTime = Time.unscaledTime;
        }
    }

    void Move(int dir)
    {
        currentIndex = (currentIndex + dir + apples.Length) % apples.Length;
        UpdateHighlight();
    }

    void UpdateHighlight()
    {
        for (int i = 0; i < apples.Length; i++)
            apples[i].SetSelected(i == currentIndex);
    }

    void SelectCurrent()
    {
        if (!canInput) return;

        canInput = false;
        apples[currentIndex].Select();
    }

    // ApplePickItem에서 호출됨
    public void OnAppleSelected(int index)
    {
        // 사과 선택 패널만 닫기
        gameObject.SetActive(false);

        // 결과 카드 보여주기
        tmiManager.ShowResult();
    }
}
