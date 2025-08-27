using System.Collections;
using UnityEngine;

public class TicketReveal : MonoBehaviour
{
    [Header("Targets")]
    public CanvasGroup cg;          // 티켓 루트(CanvasGroup)
    public RectTransform rt;        // 티켓 루트(RectTransform)
    public CanvasGroup textCg;      // 텍스트(CanvasGroup)

    [Header("Timing")]
    public float delay = 0.0f;        // 티켓 나오기 전 대기
    public float fadeTime = 0.35f;    // 티켓 페이드 시간
    public float popTime = 0.25f;     // 티켓 팝(스케일) 시간
    public float textDelay = 0.3f;    // 티켓 다 나온 뒤 텍스트 딜레이
    public float autoHideTime = 2f;   // 전체 닫히는 시간

    [Header("Pop Scale")]
    public float startScale = 0.9f;
    public float endScale = 1.0f;

    [Header("Slide")]
    public bool slideFromBottom = true;
    public float slideOffset = 80f;

    [Header("Blocking")]
    public Canvas mainCanvas;         // 허용할 메인 캔버스
    public Canvas[] blockingCanvases; // 막아야 할 UI (상점, 인벤토리 등)

    Vector2 _startPos;
    Coroutine _running;
    bool _revealing;

    void Awake()
    {
        if (!rt) rt = GetComponent<RectTransform>();
        if (!cg) cg = GetComponent<CanvasGroup>();
        _startPos = rt.anchoredPosition;

        // 초기 숨김
        cg.alpha = 0f;
        rt.localScale = Vector3.one * startScale;
        if (textCg) textCg.alpha = 0f;
    }

    public void Reveal()
    {
        if (_revealing) return;

        if (IsBlockedByOtherUI())
        {
            Debug.Log("[TicketReveal] 다른 UI 때문에 티켓 발급 대기");
            StartCoroutine(WaitUntilUnblockedThenReveal());
            return;
        }

        DoReveal();
    }

    bool IsBlockedByOtherUI()
    {
        foreach (var c in blockingCanvases)
        {
            if (!c) continue;
            if (mainCanvas && c == mainCanvas) continue;
            if (c.gameObject.activeInHierarchy) return true;
        }
        return false;
    }

    IEnumerator WaitUntilUnblockedThenReveal()
    {
        while (IsBlockedByOtherUI()) yield return null;
        DoReveal();
    }

    void DoReveal()
    {
        if (_running != null) StopCoroutine(_running);
        _running = StartCoroutine(CoReveal());
    }

    IEnumerator CoReveal()
    {
        _revealing = true;

        yield return new WaitForSeconds(delay);

        // 티켓 등장 애니메이션
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeTime;
            float a = Mathf.SmoothStep(0f, 1f, t);
            cg.alpha = a;

            float p = Mathf.SmoothStep(startScale, endScale, Mathf.Clamp01(t * (fadeTime / popTime)));
            rt.localScale = new Vector3(p, p, 1f);

            if (slideFromBottom)
            {
                rt.anchoredPosition = Vector2.Lerp(
                    _startPos - new Vector2(0, slideOffset),
                    _startPos, a);
            }
            yield return null;
        }

        cg.alpha = 1f;
        rt.localScale = Vector3.one * endScale;
        rt.anchoredPosition = _startPos;

        // 텍스트 딜레이 후 등장
        if (textCg)
        {
            yield return new WaitForSeconds(textDelay);

            float tt = 0f;
            while (tt < 1f)
            {
                tt += Time.unscaledDeltaTime / fadeTime;
                textCg.alpha = Mathf.SmoothStep(0f, 1f, tt);
                yield return null;
            }
            textCg.alpha = 1f;
        }

        // 자동 닫힘
        yield return new WaitForSeconds(autoHideTime);
        Hide();

        _revealing = false;
    }

    public void Hide(float time = 0.2f)
    {
        if (_running != null) StopCoroutine(_running);
        _running = StartCoroutine(CoHide(time));
    }

    IEnumerator CoHide(float time)
    {
        float startA = cg.alpha;
        float startTextA = textCg ? textCg.alpha : 0f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / time;
            cg.alpha = Mathf.Lerp(startA, 0f, t);
            if (textCg) textCg.alpha = Mathf.Lerp(startTextA, 0f, t);
            yield return null;
        }
        cg.alpha = 0f;
        if (textCg) textCg.alpha = 0f;
    }
}
