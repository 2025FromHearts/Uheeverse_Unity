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

    private string characterId;
    private string accessToken;
    private string BASE_URL;


    private const string BASE_URL = "https://209f-203-252-223-254.ngrok-free.app";

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
        public string item_rotation;
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

        Debug.Log("🔵 LoadInventory 시작");
        BASE_URL = ServerConfig.baseUrl;
        characterId = PlayerPrefs.GetString("character_id", "");
        accessToken = PlayerPrefs.GetString("access_token", "");

        string url = BASE_URL + "/item/inventory/" + characterId + "/";
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
                else
                    Debug.LogWarning("⚠️ ItemName 텍스트 누락");

                Transform iconTransform = slot.transform.Find("Button/ItemImage");
                if (iconTransform != null)
                {
                    Image iconImage = iconTransform.GetComponent<Image>();
                    Sprite iconSprite = Resources.Load<Sprite>("Icons/" + item.item.item_icon);
                    if (iconSprite != null)
                        iconImage.sprite = iconSprite;
                    else
                        Debug.LogWarning("⚠️ 아이콘 스프라이트를 못 찾음: " + item.item.item_icon);
                }

                Button btn = slot.transform.Find("Button")?.GetComponent<Button>();
                if (btn != null)
                {
                    ItemData capturedItem = item.item;
                    btn.onClick.AddListener(() => EquipItem(capturedItem));
                }
                else
                {
                    Debug.LogWarning("⚠️ Button 컴포넌트 누락");
                }
            }
        }
    }

    void EquipItem(ItemData item)
    {
        Debug.Log($"🧩 EquipItem() 호출됨 | item: {(item != null ? item.item_name : "null")}");

        if (item == null)
        {
            Debug.LogError("❌ item is null");
            return;
        }

        if (itemAttacher == null)
        {
            Debug.LogError("❌ itemAttacher is null. 에디터에서 할당했는지 확인!");
            return;
        }

        if (string.IsNullOrEmpty(item.item_type))
        {
            Debug.LogError("❌ item_type is null or empty");
            return;
        }

        Transform attachPoint = itemAttacher.GetAttachPoint(item.item_type);
        if (attachPoint == null)
        {
            Debug.LogError($"❌ GetAttachPoint()에서 null 반환됨: {item.item_type}");
            return;
        }

        foreach (Transform child in attachPoint)
            Destroy(child.gameObject);

        GameObject prefab = Resources.Load<GameObject>("ItemModels/" + item.item_icon);
        if (prefab == null)
        {
            Debug.LogError("❌ 프리팹 로드 실패: " + item.item_icon);
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
            else
            {
                Debug.LogWarning("⚠️ 회전값 파싱 실패: " + item.item_rotation);
            }
        }

        Debug.Log($"✅ 장착 완료: {item.item_name}");
    }
}
