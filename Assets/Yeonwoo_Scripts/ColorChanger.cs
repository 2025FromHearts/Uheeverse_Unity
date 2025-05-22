using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    public Renderer targetRenderer;
    public Renderer targetRenderer2;
    public Renderer targetRenderer3;
    public string selectedHexColor;

    // Inspector에서 지정할 색상
    public Color colorToSet = Color.white;

    public void ApplyPresetColor()
    {
        ApplyColorToAll(colorToSet);
        selectedHexColor = "#" + ColorUtility.ToHtmlStringRGB(colorToSet);
    }

    public void SetColorCustom(Color newColor)
    {
        ApplyColorToAll(newColor);
        selectedHexColor = "#" + ColorUtility.ToHtmlStringRGB(newColor);
    }

    // ✅ 공통 함수: 3개의 렌더러에 색 적용
    private void ApplyColorToAll(Color color)
    {
        if (targetRenderer != null) targetRenderer.material.color = color;
        if (targetRenderer2 != null) targetRenderer2.material.color = color;
        if (targetRenderer3 != null) targetRenderer3.material.color = color;
    }

    public void SetColorMaroon()
    {
        Color color = new Color(0.729f, 0.6f, 0.4f);
        ApplyColorToAll(color);
        selectedHexColor = "#" + ColorUtility.ToHtmlStringRGB(color);
    }

    public void SetColorMaroon2()
    {
        Color color = new Color(0.36f, 0.24f, 0.16f);
        ApplyColorToAll(color);
        selectedHexColor = "#" + ColorUtility.ToHtmlStringRGB(color);
    }

    public void SetColorHairPink()
    {
        Color color = new Color(0.85f, 0.49f, 0.51f);
        ApplyColorToAll(color);
        selectedHexColor = "#" + ColorUtility.ToHtmlStringRGB(color);
    }

    public void SetColorBlack()
    {
        Color color = Color.black;
        ApplyColorToAll(color);
        selectedHexColor = "#" + ColorUtility.ToHtmlStringRGB(color);
    }

    public void SetColorYellow()
    {
        Color color = new Color(1.0f, 1.0f, 0.7f);
        ApplyColorToAll(color);
        selectedHexColor = "#" + ColorUtility.ToHtmlStringRGB(color);
    }

    public void SetColorBlue()
    {
        Color color = new Color(0.15f, 0.23f, 0.38f);
        ApplyColorToAll(color);
        selectedHexColor = "#" + ColorUtility.ToHtmlStringRGB(color);
    }

    public void SetColorBabyPink()
    {
        Color color = new Color(1.0f, 0.8f, 0.86f);
        ApplyColorToAll(color);
        selectedHexColor = "#" + ColorUtility.ToHtmlStringRGB(color);
    }

    public void SetColorLightOrange()
    {
        Color color = new Color(1.0f, 0.75f, 0.5f);
        ApplyColorToAll(color);
        selectedHexColor = "#" + ColorUtility.ToHtmlStringRGB(color);
    }

    public void SetColorRed()
    {
        Color color = new Color(0.96f, 0.65f, 0.65f);
        ApplyColorToAll(color);
        selectedHexColor = "#" + ColorUtility.ToHtmlStringRGB(color);
    }

    public void SetColorPink()
    {
        Color color = new Color(1.0f, 0.85f, 0.88f);
        ApplyColorToAll(color);
        selectedHexColor = "#" + ColorUtility.ToHtmlStringRGB(color);
    }

    public void SetColorFromHex(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
        {
            if (targetRenderer != null) targetRenderer.material.color = color;
            if (targetRenderer2 != null) targetRenderer2.material.color = color;
            if (targetRenderer3 != null) targetRenderer3.material.color = color;
        }
        else
        {
            Debug.LogWarning("HEX 색상 파싱 실패: " + hex);
        }
    }
}
