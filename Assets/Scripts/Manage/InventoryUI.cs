using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel;
    public Transform slotParent; // 그리드 패널
    public GameObject slotPrefab; // 슬롯 프리팹

    private string characterId;
    private string accessToken;
<<<<<<< Updated upstream
=======
    private string BASE_URL;
>>>>>>> Stashed changes

    [System.Serializable]
    public class ItemData
    {
        public string item_id;
        public string item_type;
        public string item_name;
        public string item_description;
        public int item_price;
        public string map;
    }

    [System.Serializable]
    public class InventoryItem
    {
        public string inventory_id;
        public ItemData item;  // 중첩 구조로 변경
        public int slot_location;
    }

    [System.Serializable]
    public class InventoryWrapper
    {
        public List<InventoryItem> Items;
    }

    public void OpenInventory()
    {
        Debug.Log("✅ OpenInventory 호출됨");
        inventoryPanel.SetActive(true);
        StartCoroutine(LoadInventory());
    }

    IEnumerator LoadInventory()
    {
<<<<<<< Updated upstream
        Debug.Log("🔵 LoadInventory 시작");

=======
        BASE_URL = ServerConfig.baseUrl;
>>>>>>> Stashed changes
        characterId = PlayerPrefs.GetString("character_id", "");
        accessToken = PlayerPrefs.GetString("access_token", "");

        Debug.Log($"🟡 character_id: {characterId}");
        Debug.Log($"🟡 access_token: {accessToken}");

        string url = ServerConfig.baseUrl + "/item/inventory/" + characterId + "/";
        Debug.Log($"🔵 요청 URL: {url}");

        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return www.SendWebRequest();

        Debug.Log($"🟡 응답 상태: {www.result}, 코드: {www.responseCode}");

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Inventory load failed: " + www.error);
        }
        else
        {
            Debug.Log("🟢 Inventory 데이터 로드 성공");
            Debug.Log("📦 응답 데이터: " + www.downloadHandler.text);

            string json = "{\"Items\":" + www.downloadHandler.text + "}";
            InventoryWrapper wrapper = JsonUtility.FromJson<InventoryWrapper>(json);

            Debug.Log($"📦 파싱된 아이템 개수: {wrapper.Items.Count}");

            foreach (Transform child in slotParent)
            {
                Destroy(child.gameObject);
            }

            foreach (var item in wrapper.Items)
            {
                GameObject slot = Instantiate(slotPrefab, slotParent);
                TMP_Text text = slot.transform.Find("ItemName").GetComponent<TMP_Text>();
                text.text = item.item.item_name;
                Debug.Log($"✅ 슬롯 생성됨: {text.text}");
            }

            if (wrapper.Items.Count == 0)
            {
                Debug.Log("❌ 인벤토리에 아이템 없음");
            }
        }
    }
}
