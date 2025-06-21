using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSlot : MonoBehaviour
{
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemPriceText;
    public Image itemImage;
    public Button buyButton;

    private ItemData currentItem;

    public void SetItem(ItemData item)
    {
        currentItem = item;
        itemNameText.text = item.item_name;
        itemPriceText.text = item.item_price.ToString() + "코인";
        // itemImage.sprite = ... 필요 시 Sprite 로딩

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(BuyItem);
    }

    private void BuyItem()
    {
        Debug.Log("구매 시도: " + currentItem.item_name);
        // 서버에 구매 요청 보내기 or 코인 차감 + 인벤토리 추가
    }
}