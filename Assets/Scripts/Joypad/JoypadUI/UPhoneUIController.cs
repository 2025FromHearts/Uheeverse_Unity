using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UPhoneUIController : MonoBehaviour
{
    public PadInputEventRouter padInput;

    [Header("Tab ¹öÆ° (¿Þ¡æ¿À)")]
    public List<Button> tabs;

    int currentIndex = 0;

    void OnEnable()
    {
        Highlight();

        padInput.OnAPressed += MoveNext;  
        padInput.OnYPressed += MovePrev; 
        padInput.OnBPressed += Close;
    }

    void OnDisable()
    {
        padInput.OnAPressed -= MoveNext;
        padInput.OnBPressed -= Close;
    }

    void MoveNext()
    {
        currentIndex = (currentIndex + 1) % tabs.Count;
        Highlight();
    }

    void MovePrev()
    {
        currentIndex = (currentIndex - 1 + tabs.Count) % tabs.Count;
        Highlight();
    }

    void Highlight()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            ColorBlock cb = tabs[i].colors;
            cb.normalColor = (i == currentIndex) ? Color.cyan : Color.white;
            tabs[i].colors = cb;
        }
    }

    void Close()
    {
        gameObject.SetActive(false);
    }
}
