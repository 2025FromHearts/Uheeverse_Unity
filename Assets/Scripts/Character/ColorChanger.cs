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
        // 1. 입력값 NULL 체크
        if (string.IsNullOrEmpty(hex))
        {
            Debug.LogWarning("⚠️ HEX 값이 비어있습니다!");
            return;
        }

        // 2. # 제거 및 대문자 변환
        hex = hex.Trim().Replace("#", "").ToUpper();

        // 3. HEX 길이 검증 (3, 4, 6, 8자리 허용)
        if (hex.Length != 3 && hex.Length != 4 && hex.Length != 6 && hex.Length != 8)
        {
            Debug.LogWarning($"⚠️ 유효하지 않은 HEX 길이: {hex}");
            return;
        }

        // 4. 짧은 HEX(3자리) → 6자리 확장 (예: F00 → FF0000)
        if (hex.Length == 3 || hex.Length == 4)
        {
            hex = System.String.Concat(hex, hex); // FAB → FFAABB
        }

        // 5. 알파값 추가 (6자리 → 8자리)
        if (hex.Length == 6)
        {
            hex += "FF"; // 알파값 100%
        }

        // 6. 최종 HEX 파싱 시도
        Color color;
        if (ColorUtility.TryParseHtmlString("#" + hex, out color))
        {
            ApplyColorToAll(color);
            selectedHexColor = "#" + hex;
        }
        else
        {
            Debug.LogWarning($"⚠️ HEX 파싱 실패: #{hex}");
        }
    }
}
