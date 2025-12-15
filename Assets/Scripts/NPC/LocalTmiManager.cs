using System.Collections;
using TMPro;
using UnityEngine;

public class LocalTmiManager : MonoBehaviour
{
    [Header("Intro UI")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public string introMessage = "청송에 대한 재미있는 이야기를 하나 뽑아볼까요?";

    [Header("Apple Select UI")]
    public GameObject cardUIPanel;

    [Header("Result UI")]
    public GameObject resultCardPanel;
    public CanvasGroup resultCanvasGroup;
    public TMP_Text tmiText;

    [Header("Pad")]
    public PadInputEventRouter padInput;

    public float fadeDuration = 0.4f;
    public float textDelay = 0.25f;

    enum State
    {
        None,
        Intro,
        Select,
        Result
    }

    State state = State.None;

    void OnEnable()
    {
        padInput.OnAPressed += OnAPressed;
        padInput.OnXPressed += OnXPressed;
    }

    void OnDisable()
    {
        padInput.OnAPressed -= OnAPressed;
        padInput.OnXPressed -= OnXPressed;
    }

    public void Open(string region)
    {
        if (state != State.None) return;

        UIManager.IsUIBlocking = true;
        padInput.currentMode = PadInputEventRouter.InputMode.Popup;

        state = State.Intro;

        dialoguePanel.SetActive(true);
        dialogueText.text = introMessage;

        StartCoroutine(ToAppleSelect());
    }

    IEnumerator ToAppleSelect()
    {
        yield return new WaitForSecondsRealtime(1.2f);

        dialoguePanel.SetActive(false);
        cardUIPanel.SetActive(true);

        state = State.Select;
    }

    public void ShowResult()
    {
        if (state != State.Select) return;

        cardUIPanel.SetActive(false);
        resultCardPanel.SetActive(true);

        state = State.Result;
        StartCoroutine(FadeInResult());
    }

    IEnumerator FadeInResult()
    {
        resultCanvasGroup.alpha = 0f;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            resultCanvasGroup.alpha = t / fadeDuration;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(textDelay);

        tmiText.text = GetRandomTmi();
        tmiText.canvasRenderer.SetAlpha(0f);
        tmiText.CrossFadeAlpha(1f, 0.3f, false);
    }

    void OnAPressed()
    {
        if (state != State.Result) return;

        // 다시 뽑기
        resultCardPanel.SetActive(false);
        cardUIPanel.SetActive(true);

        state = State.Select;
    }

    void OnXPressed()
    {
        if (state == State.None) return;

        CloseAll();
    }

    void CloseAll()
    {
        StopAllCoroutines();

        dialoguePanel.SetActive(false);
        cardUIPanel.SetActive(false);
        resultCardPanel.SetActive(false);

        state = State.None;

        UIManager.IsUIBlocking = false;
        padInput.currentMode = PadInputEventRouter.InputMode.Player;
    }

    string[] tmiList =
    {
        "청송 사과는 일교차가 커서 당도가 높아요.",
        "청송은 전국 최대 규모의 사과 산지 중 하나예요.",
        "청송 사과는 저장성이 좋아 오래 보관할 수 있어요.",
        "청송사과축제는 매년 가을에 열려요."
    };

    string GetRandomTmi()
    {
        return tmiList[Random.Range(0, tmiList.Length)];
    }
}
