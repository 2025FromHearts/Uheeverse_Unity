using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel;
    public GameObject closeButtonObject;
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

    // === 페이지네이션 설정 추가===
    [SerializeField] int gridColumns = 5;
    [SerializeField] int gridRows = 3;
    int PageSize => gridColumns * gridRows;

    [SerializeField] Button prevPageBtn;
    [SerializeField] Button nextPageBtn;
    [SerializeField] TMP_Text pageLabel;

    // 전체 아이템 / 슬롯 풀
    List<InventoryItem> _items = new List<InventoryItem>();
    readonly List<GameObject> _slotPool = new List<GameObject>();
    int _currentPage = 0;
    void Awake()
    {
        if (prevPageBtn) prevPageBtn.onClick.AddListener(() => SetPage(_currentPage - 1));
        if (nextPageBtn) nextPageBtn.onClick.AddListener(() => SetPage(_currentPage + 1));
    }
    //여기까지 추가됨

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
            //추가됨
            foreach (Transform child in slotParent)
                Destroy(child.gameObject);               // 최초 1회 깔끔히 비움(원하면 유지해도 OK)

            _items = (wrapper != null && wrapper.Items != null) ? wrapper.Items : new List<InventoryItem>();

            BuildPoolIfNeeded();   // 페이지에 필요한 슬롯 개수만 생성
            SetPage(0);            // 첫 페이지 표시
        }
    }
    void BuildPoolIfNeeded() //추가됨
    {
        if (slotPrefab == null || slotParent == null) return;

        while (_slotPool.Count < PageSize)
        {
            var slot = Instantiate(slotPrefab, slotParent);
            slot.SetActive(true); // 비활성 프리팹 방지
            _slotPool.Add(slot);
        }
    }

    void BindSlot(GameObject slot, InventoryItem inv) //추가됨
    {
        // 이름
        var text = slot.transform.Find("Button/ItemName")?.GetComponent<TMP_Text>();
        if (text) text.text = inv.item.item_name;

        // 아이콘
        var iconTr = slot.transform.Find("Button/ItemImage");
        var iconImg = iconTr ? iconTr.GetComponent<Image>() : null;
        var icon = Resources.Load<Sprite>("Icons/" + inv.item.item_icon);
        if (iconImg && icon) iconImg.sprite = icon;

        // 버튼
        var btn = slot.transform.Find("Button")?.GetComponent<Button>();
        if (btn)
        {
            btn.onClick.RemoveAllListeners();
            var captured = inv.item;
            btn.onClick.AddListener(() =>
            {
                currentSelectedItem = captured;
                ShowDetail(captured);
            });
        }
    }

    void SetPage(int page) //추가됨
    {
        if (_items == null) return;

        int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)_items.Count / PageSize));
        _currentPage = Mathf.Clamp(page, 0, totalPages - 1);

        for (int i = 0; i < _slotPool.Count; i++)
        {
            int dataIndex = _currentPage * PageSize + i;
            if (dataIndex < _items.Count)
            {
                _slotPool[i].SetActive(true);
                BindSlot(_slotPool[i], _items[dataIndex]);
            }
            else
            {
                _slotPool[i].SetActive(false);
            }
        }

        if (pageLabel) pageLabel.text = $"{_currentPage + 1} / {totalPages}";
        if (prevPageBtn) prevPageBtn.interactable = _currentPage > 0;
        if (nextPageBtn) nextPageBtn.interactable = _currentPage < totalPages - 1;

        var rt = slotParent as RectTransform;
        if (rt) UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
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
