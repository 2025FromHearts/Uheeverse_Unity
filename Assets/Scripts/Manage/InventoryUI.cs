using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class InventoryUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject inventoryPanel;
    public Transform slotParent;
    public GameObject slotPrefab;

    [Header("Detail")]
    public GameObject detailPanel;
    public Image detailItemImage;
    public TMP_Text detailName;
    public TMP_Text detailDescription;

    public GameObject infoGroup;
    public GameObject placeholderText;

    UdpPadReceiver pad;
    public float axisThreshold = 0.6f;
    public float moveDelay = 0.25f;
    float lastMoveTime;

    private string baseUrl;
    private string characterId;
    private string accessToken;

    private List<InventoryItem> _items = new List<InventoryItem>();
    private List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();
    private int selectedIndex = -1;

    [System.Serializable]
    public class ItemDataDTO
    {
        public string item_id;
        public string item_name;
        public string item_description;
        public string item_icon;
    }

    [System.Serializable]
    public class InventoryItem
    {
        public ItemDataDTO item;
        public int count;
    }

    [System.Serializable]
    public class InventoryWrapper
    {
        public List<RawInventoryItem> Items;
    }

    [System.Serializable]
    public class RawInventoryItem
    {
        public string inventory_id;
        public ItemDataDTO item;
    }

    void Start()
    {
        pad = FindAnyObjectByType<UdpPadReceiver>();
    }

    void Update()
    {
        if (!inventoryPanel.activeInHierarchy) return;
        if (pad == null || pad.latest == null) return;
        if (_items.Count == 0) return;

        if (Time.unscaledTime - lastMoveTime < moveDelay) return;

        float x = pad.latest.lx;
        if (Mathf.Abs(x) < axisThreshold) return;

        MoveSelection(x > 0 ? 1 : -1);
        lastMoveTime = Time.unscaledTime;
    }

    public void OpenInventory()
    {
        inventoryPanel.SetActive(true);
        detailPanel.SetActive(true);

        infoGroup.SetActive(false);
        placeholderText.SetActive(true);

        selectedIndex = -1;
        StartCoroutine(LoadInventory());
    }

    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
        detailPanel.SetActive(false);

        infoGroup.SetActive(false);
        placeholderText.SetActive(false);

        selectedIndex = -1;
        _items.Clear();
        slotUIs.Clear();

        foreach (Transform child in slotParent)
            Destroy(child.gameObject);
    }
    IEnumerator LoadInventory()
    {
        baseUrl = ServerConfig.baseUrl;
        characterId = PlayerPrefs.GetString("character_id", "");
        accessToken = PlayerPrefs.GetString("access_token", "");

        string url = $"{baseUrl}/item/inventory/{characterId}/";
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Inventory load failed: " + www.error);
            yield break;
        }

        string json = "{\"Items\":" + www.downloadHandler.text + "}";
        InventoryWrapper wrapper = JsonUtility.FromJson<InventoryWrapper>(json);

        // === 1. 중복 아이템 합치기 ===
        Dictionary<string, InventoryItem> merged = new Dictionary<string, InventoryItem>();

        foreach (var raw in wrapper.Items)
        {
            if (raw == null || raw.item == null) continue;
            if (string.IsNullOrEmpty(raw.item.item_id)) continue;

            string key = raw.item.item_id;

            if (merged.ContainsKey(key))
            {
                merged[key].count++;
            }
            else
            {
                merged[key] = new InventoryItem
                {
                    item = raw.item,
                    count = 1
                };
            }
        }

        _items = new List<InventoryItem>(merged.Values);

        // === 2. 슬롯 생성 ===
        foreach (Transform child in slotParent)
            Destroy(child.gameObject);

        slotUIs.Clear();

        foreach (var inv in _items)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotParent);

            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
            if (slotUI != null)
                slotUIs.Add(slotUI);

            // 텍스트 (개수 포함)
            TMP_Text text = slotObj.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = inv.count > 1
                    ? $"{inv.item.item_name} x{inv.count}"
                    : inv.item.item_name;
            }

            //Icon
            Image img = slotObj.transform
                .Find("Button/ItemImage")
                ?.GetComponent<Image>();

            Sprite icon = Resources.Load<Sprite>("Icons/" + inv.item.item_icon);

            if (img != null && icon != null)
                img.sprite = icon;
            else
            {
                if (img == null)
                    Debug.LogError("❌ ItemImage 못 찾음 (Button/ItemImage)");
                if (icon == null)
                    Debug.LogError("❌ 아이콘 로드 실패: " + inv.item.item_icon);
            }

            // 마우스 클릭 유지
            Button btn = slotObj.GetComponentInChildren<Button>();
            if (btn != null)
            {
                InventoryItem captured = inv;
                btn.onClick.AddListener(() =>
                {
                    selectedIndex = _items.IndexOf(captured);
                    UpdateSelection();
                });
            }
        }

        if (_items.Count > 0)
        {
            selectedIndex = 0;
            UpdateSelection();
        }
    }
    void MoveSelection(int dir)
    {
        if (_items.Count == 0) return;

        selectedIndex += dir;

        if (selectedIndex < 0)
            selectedIndex = _items.Count - 1;
        else if (selectedIndex >= _items.Count)
            selectedIndex = 0;

        UpdateSelection();
    }

    void UpdateSelection()
    {
        for (int i = 0; i < slotUIs.Count; i++)
        {
            if (slotUIs[i] != null)
                slotUIs[i].SetSelected(i == selectedIndex);
        }

        ShowDetail(_items[selectedIndex].item);
    }

    void ShowDetail(ItemDataDTO item)
    {
        if (item == null) return;

        detailPanel.SetActive(true);
        infoGroup.SetActive(true);
        placeholderText.SetActive(false);

        detailName.text = item.item_name;
        detailDescription.text = item.item_description;

        Sprite icon = Resources.Load<Sprite>("Icons/" + item.item_icon);
        if (icon != null)
            detailItemImage.sprite = icon;
    }
}
