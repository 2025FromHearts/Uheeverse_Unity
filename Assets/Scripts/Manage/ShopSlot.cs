using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ShopSlot : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text itemNameText;
    public Image itemImage;
    public Button itemButton;

    [Header("Scale Emphasis")]
    public float selectedScale = 1.08f;
    public float scaleSpeed = 8f;

    Vector3 normalScale;
    Coroutine scaleRoutine;

    private ShopUI.ItemDataDTO itemData;
    private ShopUI shopUI;

    void Awake()
    {
        normalScale = transform.localScale;
    }
    public void Set(ShopUI.ItemDataDTO item, ShopUI ui)
    {
        itemData = item;
        shopUI = ui;

        if (itemNameText != null)
            itemNameText.text = item.item_name;

        if (itemImage != null && !string.IsNullOrEmpty(item.item_icon))
        {
            Sprite iconSprite = Resources.Load<Sprite>("Icons/" + item.item_icon);
            if (iconSprite != null)
                itemImage.sprite = iconSprite;
        }

        if (itemButton != null)
        {
            itemButton.onClick.RemoveAllListeners();
            itemButton.onClick.AddListener(Select);
        }

        ApplyScale(false, true);
    }

    public void Select()
    {
        shopUI.OnSlotClicked(itemData);
    }

    public void SetSelected(bool selected)
    {
        ApplyScale(selected, false);
    }

    void ApplyScale(bool selected, bool instant)
    {
        Vector3 targetScale = selected
            ? normalScale * selectedScale
            : normalScale;

        if (scaleRoutine != null)
            StopCoroutine(scaleRoutine);

        if (instant)
        {
            transform.localScale = targetScale;
        }
        else
        {
            scaleRoutine = StartCoroutine(ScaleAnim(targetScale));
        }
    }

    IEnumerator ScaleAnim(Vector3 target)
    {
        Vector3 start = transform.localScale;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * scaleSpeed;
            transform.localScale = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.localScale = target;
    }
}
