using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSlot : MonoBehaviour
{
    public TMP_Text itemNameText;
    public Image itemImage;
    public Button itemButton;

    private ShopUI.ItemDataDTO itemData;
    private ShopUI shopUI;

    // 슬롯에 데이터 설정
    public void Set(ShopUI.ItemDataDTO item, ShopUI ui)
    {
        itemData = item;
        shopUI = ui;

        // 이름 텍스트 설정
        if (itemNameText != null)
            itemNameText.text = item.item_name;

        // 아이콘 이미지 설정
        if (itemImage != null && !string.IsNullOrEmpty(item.item_icon))
        {
            Sprite iconSprite = Resources.Load<Sprite>("Icons/" + item.item_icon);
            if (iconSprite != null)
                itemImage.sprite = iconSprite;
            else
                Debug.LogWarning("⚠️ 아이콘 로드 실패: " + item.item_icon);
        }

        // 버튼 클릭 시 상세 보기
        if (itemButton != null)
        {
            itemButton.onClick.RemoveAllListeners();
            itemButton.onClick.AddListener(() => shopUI.OnSlotClicked(itemData));
        }
    }
}