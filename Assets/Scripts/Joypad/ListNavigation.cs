using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ListNavigation : MonoBehaviour
{
    [Header("Assign")]
    public ScrollRect scrollRect;

    [Header("Input")]
    public float moveDelay = 0.25f;
    public float moveThreshold = 0.6f;

    float lastMoveTime = -999f;

    List<MonoBehaviour> items = new List<MonoBehaviour>();
    int currentIndex = 0;
    public void SetItems(List<MonoBehaviour> newItems, bool resetToTop = true)
    {
        items = newItems ?? new List<MonoBehaviour>();
        currentIndex = 0;

        StopAllCoroutines();
        StartCoroutine(ApplyAfterLayout(resetToTop));
    }

    public void ResetToTop()
    {
        currentIndex = 0;
        UpdateHighlight();

        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
    }

    IEnumerator ApplyAfterLayout(bool resetToTop)
    {
        yield return null;
        ForceLayout();

        UpdateHighlight();

        if (resetToTop && scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;

        yield return null;
        ForceLayout();
        ScrollToCurrent();
    }

    public void MoveUp()
    {
        if (items.Count == 0) return;
        if (Time.unscaledTime - lastMoveTime < moveDelay) return;

        currentIndex = Mathf.Max(0, currentIndex - 1);
        lastMoveTime = Time.unscaledTime;

        UpdateHighlight();
        ScrollToCurrent();
    }

    public void MoveDown()
    {
        if (items.Count == 0) return;
        if (Time.unscaledTime - lastMoveTime < moveDelay) return;

        currentIndex = Mathf.Min(items.Count - 1, currentIndex + 1);
        lastMoveTime = Time.unscaledTime;

        UpdateHighlight();
        ScrollToCurrent();

        Debug.Log("[ListNavigation] MoveDown called");
    }

    void UpdateHighlight()
    {
        if (items == null || items.Count == 0) return;
        if (currentIndex < 0 || currentIndex >= items.Count) return;

        for (int i = 0; i < items.Count; i++)
        {
            if (!items[i]) continue;

            items[i].SendMessage(
                "SetHighlight",
                i == currentIndex,
                SendMessageOptions.DontRequireReceiver
            );
        }
    }

    void ScrollToCurrent()
    {
        if (scrollRect == null) return;
        if (items.Count == 0) return;

        var content = scrollRect.content;
        var viewport = scrollRect.viewport ? scrollRect.viewport : (RectTransform)scrollRect.transform;
        if (content == null || viewport == null) return;

        var target = items[currentIndex].GetComponent<RectTransform>();
        if (target == null) return;

        ForceLayout();

        Bounds viewBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, viewport);
        Bounds targetBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, target);

        float deltaY = 0f;
        if (targetBounds.max.y > viewBounds.max.y)
            deltaY = targetBounds.max.y - viewBounds.max.y;
        else if (targetBounds.min.y < viewBounds.min.y)
            deltaY = targetBounds.min.y - viewBounds.min.y;

        if (Mathf.Abs(deltaY) > 0.001f)
        {
            content.anchoredPosition -= new Vector2(0f, deltaY);
            ClampContentY();
        }
    }

    void ClampContentY()
    {
        if (scrollRect == null) return;

        var content = scrollRect.content;
        var viewport = scrollRect.viewport ? scrollRect.viewport : (RectTransform)scrollRect.transform;
        if (content == null || viewport == null) return;

        ForceLayout();

        float contentH = content.rect.height;
        float viewH = viewport.rect.height;

        if (contentH <= viewH)
        {
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, 0f);
            return;
        }

        float maxY = contentH - viewH;
        float y = Mathf.Clamp(content.anchoredPosition.y, 0f, maxY);
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, y);
    }

    void ForceLayout()
    {
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null && scrollRect.content != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
    }
}
