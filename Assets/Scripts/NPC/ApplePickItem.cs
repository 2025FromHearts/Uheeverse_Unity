using UnityEngine;

public class ApplePickItem : MonoBehaviour
{
    RectTransform rect;
    AppleSelectManager manager;
    int index;

    public float normalScale = 1f;
    public float selectedScale = 1.2f;
    public float scaleSpeed = 12f;

    bool isSelected = false;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void Init(int idx, AppleSelectManager m)
    {
        index = idx;
        manager = m;
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
    }

    void Update()
    {
        if (rect == null) return;

        float target = isSelected ? selectedScale : normalScale;
        rect.localScale = Vector3.Lerp(
            rect.localScale,
            Vector3.one * target,
            Time.unscaledDeltaTime * scaleSpeed
        );
    }

    public void Select()
    {
        manager.OnAppleSelected(index);
    }
}
