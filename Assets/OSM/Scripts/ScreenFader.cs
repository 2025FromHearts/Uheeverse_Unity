using System.Collections;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    public CanvasGroup group;
    public float defaultTime = 0.4f;

    void Reset() { group = GetComponent<CanvasGroup>(); }

    public IEnumerator FadeTo(float target, float time = -1f) {
        if (time < 0) time = defaultTime;
        float start = group.alpha;
        float t = 0f;
        while (t < 1f) {
            t += Time.deltaTime / time;
            group.alpha = Mathf.Lerp(start, target, t);
            yield return null;
        }
        group.alpha = target;
    }

    public IEnumerator FadeOut(float t = -1f) => FadeTo(1f, t);
    public IEnumerator FadeIn (float t = -1f) => FadeTo(0f, t);
}
