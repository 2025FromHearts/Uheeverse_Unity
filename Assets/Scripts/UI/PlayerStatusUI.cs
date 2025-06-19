using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatusUI : MonoBehaviour
{
    public Image playerImage;
    public TextMeshProUGUI lapText;

    public void SetPlayerSprite(Sprite sprite)
    {
        if (playerImage != null)
            playerImage.sprite = sprite;
    }

    public void UpdateLapText(int currentLap, int maxLaps)
    {
        if (lapText != null)
            lapText.text = $"Lap: {currentLap} / {maxLaps}";
    }
}
