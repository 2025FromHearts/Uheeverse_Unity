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

    public Transform attachPoint; // 아이템 장착 위치 (Empty GameObject)

    private string characterId;
    private string accessToken;

    [System.Serializable]
    public class ItemData
    {
        public string item_id;
        public string item_type;
        public string item_name;
        public string item_description;
        public int item_price;
        public string item_icon;
        public string map;
        public string item_rotation;  // ← 추가됨
    }

    [System.Serializable]
    public class InventoryItem
    {
        public string inventory_id;
        public ItemData item;
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
        StartCoroutine(LoadInventory());
    }

    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
    }

    IEnumerator LoadInventory()
    {
        characterId = PlayerPrefs.GetString("character_id", "");
        accessToken = PlayerPrefs.GetString("access_token", "");

        string url = "http://127.0.0.1:8000/item/inventory/" + characterId + "/";
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Inventory load failed: " + www.error);
        }
        else
        {
            string json = "{\"Items\":" + www.downloadHandler.text + "}";
            InventoryWrapper wrapper = JsonUtility.FromJson<InventoryWrapper>(json);

            foreach (Transform child in slotParent)
            {
                Destroy(child.gameObject);
            }

            foreach (var item in wrapper.Items)
            {
                GameObject slot = Instantiate(slotPrefab, slotParent);

                // 이름 텍스트
                TMP_Text text = slot.transform.Find("ItemName").GetComponent<TMP_Text>();
                text.text = item.item.item_name;

                // 아이콘 이미지
                Image iconImage = slot.transform.Find("ItemImage").GetComponent<Image>();
                Sprite iconSprite = Resources.Load<Sprite>("Icons/" + item.item.item_icon);
                if (iconSprite != null) iconImage.sprite = iconSprite;

                // 버튼 클릭 → 아이템 착용
                Button btn = slot.GetComponent<Button>();
                if (btn != null)
                {
                    ItemData capturedItem = item.item;
                    btn.onClick.AddListener(() => EquipItem(capturedItem));
                }
            }
        }
    }

    void EquipItem(ItemData item)
    {
        // 기존 아이템 제거
        foreach (Transform child in attachPoint)
        {
            Destroy(child.gameObject);
        }

        // 프리팹 로드
        GameObject prefab = Resources.Load<GameObject>("Wearables/" + item.item_icon);
        if (prefab == null)
        {
            Debug.LogWarning($"❌ 프리팹 없음: {item.item_icon}");
            return;
        }

        // 장착
        GameObject equipped = Instantiate(prefab, attachPoint);
        equipped.transform.localPosition = Vector3.zero;

        // 회전값 적용
        string[] rotParts = item.item_rotation.Split(',');
        if (rotParts.Length == 3)
        {
            if (float.TryParse(rotParts[0], out float x) &&
                float.TryParse(rotParts[1], out float y) &&
                float.TryParse(rotParts[2], out float z))
            {
                equipped.transform.localRotation = Quaternion.Euler(x, y, z);
            }
        }

        Debug.Log($"🎮 아이템 장착됨: {item.item_name}");
    }
}
