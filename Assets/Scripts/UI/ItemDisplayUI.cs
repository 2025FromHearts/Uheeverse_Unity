using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ItemDisplayUI : MonoBehaviour
{
    public Image itemIcon;

    public Sprite speedSprite;
    public Sprite slowSprite;
    public Sprite spinSprite;
    public Sprite reverseSprite;

    void Awake()
    {
        ClearIcon(); // 시작 시 아이콘 비활성화
    }

    public void SetItemIcon(ItemEffectType effectType)
    {
        switch (effectType)
        {
            case ItemEffectType.SpeedUp:
                itemIcon.sprite = speedSprite;
                break;
            case ItemEffectType.SlowDown:
                itemIcon.sprite = slowSprite;
                break;
            case ItemEffectType.Spin:
                itemIcon.sprite = spinSprite;
                break;
            case ItemEffectType.ReverseControl:
                itemIcon.sprite = reverseSprite;
                break;
        }

        itemIcon.enabled = true;
    }

    public void ShowItem(ItemEffectType effectType)
    {
        SetItemIcon(effectType);  // 중복 제거

        StartCoroutine(HideAfterSeconds(2f));
    }

    public void ClearIcon()
    {
        itemIcon.sprite = null;
        itemIcon.enabled = false;
    }

    private IEnumerator HideAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ClearIcon();
    }
}
