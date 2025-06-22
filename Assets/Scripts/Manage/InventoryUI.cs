using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel;
    public Transform slotParent;
    public GameObject slotPrefab;
    public ItemAttacher itemAttacher;

    public GameObject detailPanel;
    public Image detailImage;
    public Image detailItemImage;
    public TMP_Text detailName;
    public TMP_Text detailDescription;
    public Button putOnButton;
    public Button putOffButton;

    public GameObject infoGroup;
    public GameObject placeholderText;

    private ItemDataDTO currentSelectedItem;
    private string baseUrl;
    private string characterId;
    private string accessToken;

    [System.Serializable]
    public class ItemDataDTO
    {
        public string item_id;
        public string item_type;
        public string item_name;
        public string item_description;
        public int item_price;
        public string item_icon;
        public string map;
        public string item_rotation;
    }

    [System.Serializable]
    public class InventoryItem
    {
        public string inventory_id;
        public ItemDataDTO item;
        public int slot_location;
    }

    [System.Serializable]
    public class InventoryWrapper
    {
        public List<InventoryItem> Items;
    }

    public void OpenInventory()
    {
        inventoryPanel.SetActive(true);
        detailPanel.SetActive(true);

        if (infoGroup != null) infoGroup.SetActive(false);
        if (placeholderText != null) placeholderText.SetActive(true);

        StartCoroutine(LoadInventory());
    }

    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
        detailPanel.SetActive(false);

        if (infoGroup != null) infoGroup.SetActive(false);
        if (placeholderText != null) placeholderText.SetActive(false);
    }

    IEnumerator LoadInventory()
    {
        baseUrl = ServerConfig.baseUrl;
        characterId = PlayerPrefs.GetString("character_id", "");
        accessToken = PlayerPrefs.GetString("access_token", "");

        string url = baseUrl + "/item/inventory/" + characterId + "/";
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Inventory load failed: " + www.error);
        }
        else
        {
            Debug.Log("✅ Raw inventory JSON: " + www.downloadHandler.text);

            string json = "{\"Items\":" + www.downloadHandler.text + "}";
            InventoryWrapper wrapper = JsonUtility.FromJson<InventoryWrapper>(json);

            foreach (Transform child in slotParent)
                Destroy(child.gameObject);

            foreach (var item in wrapper.Items)
            {
                GameObject slot = Instantiate(slotPrefab, slotParent);

                TMP_Text text = slot.transform.Find("Button/ItemName")?.GetComponent<TMP_Text>();
                if (text != null)
                    text.text = item.item.item_name;

                Transform iconTransform = slot.transform.Find("Button/ItemImage");
                if (iconTransform != null)
                {
                    Image iconImage = iconTransform.GetComponent<Image>();
                    Sprite iconSprite = Resources.Load<Sprite>("Icons/" + item.item.item_icon);
                    if (iconSprite != null)
                        iconImage.sprite = iconSprite;
                }

                Button btn = slot.transform.Find("Button")?.GetComponent<Button>();
                if (btn != null)
                {
                    ItemDataDTO capturedItem = item.item;
                    btn.onClick.AddListener(() =>
                    {
                        currentSelectedItem = capturedItem;
                        ShowDetail(capturedItem);
                    });
                }
            }
        }
    }

    void ShowDetail(ItemDataDTO item)
    {
        if (item == null) return;

        detailPanel.SetActive(true);

        if (infoGroup != null) infoGroup.SetActive(true);
        if (placeholderText != null) placeholderText.SetActive(false);

        detailName.text = item.item_name;
        detailDescription.text = item.item_description;

        Sprite iconSprite = Resources.Load<Sprite>("Icons/" + item.item_icon);
        if (iconSprite != null && detailItemImage != null)
            detailItemImage.sprite = iconSprite;
        else
            Debug.LogWarning("⚠️ 아이템 이미지 로드 실패: " + item.item_icon);

        putOnButton.onClick.RemoveAllListeners();
        putOnButton.onClick.AddListener(() => EquipItem(item));

        putOffButton.onClick.RemoveAllListeners();
        putOffButton.onClick.AddListener(() => UnEquipItem(item));
    }

    void EquipItem(ItemDataDTO item)
    {
        Debug.Log($"🧙 EquipItem() | item: {(item != null ? item.item_name : "null")}");

        if (item == null || itemAttacher == null || string.IsNullOrEmpty(item.item_type)) return;

        Transform attachPoint = itemAttacher.GetAttachPoint(item.item_type);
        if (attachPoint == null) return;

        foreach (Transform child in attachPoint)
            Destroy(child.gameObject);

        GameObject prefab = Resources.Load<GameObject>("ItemModels/" + item.item_icon);
        if (prefab == null)
        {
            Debug.LogError("❌ Prefab not found: " + item.item_icon);
            return;
        }

        GameObject equipped = Instantiate(prefab, attachPoint);
        equipped.transform.localPosition = Vector3.zero;

        if (!string.IsNullOrEmpty(item.item_rotation))
        {
            string[] rotParts = item.item_rotation.Split(',');
            if (rotParts.Length == 3 &&
                float.TryParse(rotParts[0], out float x) &&
                float.TryParse(rotParts[1], out float y) &&
                float.TryParse(rotParts[2], out float z))
            {
                equipped.transform.localRotation = Quaternion.Euler(x, y, z);
            }
        }

        Debug.Log($"✅ Equipped: {item.item_name}");
    }

    void UnEquipItem(ItemDataDTO item)
    {
        if (item == null || itemAttacher == null || string.IsNullOrEmpty(item.item_type)) return;

        Transform attachPoint = itemAttacher.GetAttachPoint(item.item_type);
        if (attachPoint == null) return;

        foreach (Transform child in attachPoint)
            Destroy(child.gameObject);

        Debug.Log($"🔓 Unequipped: {item.item_name}");
    }
}
