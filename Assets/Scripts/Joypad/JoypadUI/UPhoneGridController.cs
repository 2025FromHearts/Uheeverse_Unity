using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UPhoneGridController : MonoBehaviour
{
    [Header("입력")]
    public UdpPadReceiver pad;
    public PadInputEventRouter padInput;

    [Header("아이콘 (왼→오, 위→아래 순서)")]
    public List<Image> icons;

    [Header("아이콘에 대응하는 패널")]
    public List<GameObject> panels;

    private GameObject currentPanel = null;

    [Header("그리드 설정")]
    public int columns = 2;
    public float inputThreshold = 0.5f;
    public float inputDelay = 0.25f;

    [Header("Scale Highlight")]
    public float selectedScale = 1.15f;
    public float scaleSpeed = 10f;

    int currentIndex = 0;
    float lastInputTime;

    List<Vector3> originalScales = new List<Vector3>();

    void Awake()
    {
        originalScales.Clear();
        foreach (var icon in icons)
        {
            originalScales.Add(icon.transform.localScale);
        }
    }

    void OnEnable()
    {
        lastInputTime = 0f;

        if (padInput != null)
        {
            padInput.OnAPressed += OpenSelectedPanel;
            padInput.OnBPressed += CloseCurrentPanel;
            padInput.OnXPressed += ExitUPhone;
        }
    }

    void OnDisable()
    {
        if (padInput != null)
        {
            padInput.OnAPressed -= OpenSelectedPanel;
            padInput.OnBPressed -= CloseCurrentPanel;
            padInput.OnXPressed -= ExitUPhone;
        }

        for (int i = 0; i < icons.Count; i++)
        {
            if (icons[i] != null)
                icons[i].transform.localScale = originalScales[i];
        }
    }

    void Update()
    {
        HandleInput();
        UpdateIconScale();
    }

    void HandleInput()
    {
        if (pad == null || pad.latest == null) return;
        if (Time.time - lastInputTime < inputDelay) return;

        float x = pad.latest.lx;
        float y = pad.latest.ly;

        if (x > inputThreshold) MoveRight();
        else if (x < -inputThreshold) MoveLeft();
        else if (y > inputThreshold) MoveUp();
        else if (y < -inputThreshold) MoveDown();
    }

    void MoveRight()
    {
        int next = currentIndex + 1;
        if (next < icons.Count)
            SetIndex(next);
    }

    void MoveLeft()
    {
        int next = currentIndex - 1;
        if (next >= 0)
            SetIndex(next);
    }

    void MoveUp()
    {
        int next = currentIndex - columns;
        if (next >= 0)
            SetIndex(next);
    }

    void MoveDown()
    {
        int next = currentIndex + columns;
        if (next < icons.Count)
            SetIndex(next);
    }

    void SetIndex(int index)
    {
        currentIndex = index;
        lastInputTime = Time.time;
    }

    void UpdateIconScale()
    {
        for (int i = 0; i < icons.Count; i++)
        {
            if (icons[i] == null) continue;

            Transform t = icons[i].transform;
            Vector3 baseScale = originalScales[i];

            Vector3 targetScale =
                (i == currentIndex)
                ? baseScale * selectedScale
                : baseScale;

            t.localScale = Vector3.Lerp(
                t.localScale,
                targetScale,
                Time.unscaledDeltaTime * scaleSpeed
            );
        }
    }

    void OpenSelectedPanel()
    {
        if (currentIndex < 0 || currentIndex >= panels.Count) return;

        if (currentPanel != null)
            currentPanel.SetActive(false);

        currentPanel = panels[currentIndex];
        if (currentPanel != null)
            currentPanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
    }

    void CloseCurrentPanel()
    {
        if (currentPanel == null) return;

        currentPanel.SetActive(false);
        currentPanel = null;
    }

    void ExitUPhone()
    {
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
            currentPanel = null;
        }

        foreach (var p in panels)
        {
            if (p != null)
                p.SetActive(false);
        }

        gameObject.SetActive(false);
    }
}
