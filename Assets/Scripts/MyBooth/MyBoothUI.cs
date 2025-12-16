using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class MyBoothUI : MonoBehaviour
{
    public GameObject inventoryPanel;
    public Transform slotParent;
    public GameObject slotPrefab;

    public GameObject detailPanel;
    public Image detailItemImage;
    public TMP_Text detailName;
    public TMP_Text detailDescription;
    public Button putOnButton;
    public GameObject infoGroup;
    public GameObject placeholderText;

    public PadInputEventRouter padInput;
    public MyBoothPadController padController;
    public BoothManager boothManager;

    [SerializeField] int gridColumns = 5;
    [SerializeField] int gridRows = 3;
    int PageSize => gridColumns * gridRows;

    [SerializeField] Button prevPageBtn, nextPageBtn;
    [SerializeField] TMP_Text pageLabel;

    [SerializeField] PlacementController placement;
    [SerializeField] KeyCode rotateKey = KeyCode.R;

    string baseUrl, characterId, accessToken;

    List<InventoryItem> _items = new();
    readonly List<GameObject> _slotPool = new();
    int _currentPage;

    ItemDataDTO _current;
    bool isPlacing = false;

    public int SlotCount => _slotPool.Count;

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

        public Vector3 positionOffset;
    }

    [System.Serializable]
    public class InventoryItem
    {
        public string inventory_id;
        public ItemDataDTO item;
        public int slot_location;
        public int count = 1;
    }

    [System.Serializable]
    public class InventoryWrapper
    {
        public List<InventoryItem> Items;
    }

    void Start()
    {
        if (placement != null)
            placement.onPlacementComplete = () => isPlacing = false;
    }

    void Awake()
    {
        if (prevPageBtn) prevPageBtn.onClick.AddListener(() => SetPage(_currentPage - 1));
        if (nextPageBtn) nextPageBtn.onClick.AddListener(() => SetPage(_currentPage + 1));
    }

    public void OpenInventory()
    {
        inventoryPanel.SetActive(true);
        detailPanel.SetActive(true);

        if (infoGroup) infoGroup.SetActive(false);
        if (placeholderText) placeholderText.SetActive(true);

        StartCoroutine(LoadInventory());

        if (padController != null)
            padController.Activate();

        UIManager.IsUIBlocking = true;
        if (padInput != null) padInput.currentMode = PadInputEventRouter.InputMode.Popup;
    }

    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
        detailPanel.SetActive(false);

        if (infoGroup) infoGroup.SetActive(false);
        if (placeholderText) placeholderText.SetActive(false);

        if (padController != null)
            padController.Deactivate();

        UIManager.IsUIBlocking = false;
        if (padInput != null) padInput.currentMode = PadInputEventRouter.InputMode.Player;
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
        Debug.Log("[MyBoothUI] Inventory response: " + www.downloadHandler.text);
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
            yield break;
        }

        var wrapper = JsonUtility.FromJson<InventoryWrapper>("{\"Items\":" + www.downloadHandler.text + "}");
        var rawItems = wrapper?.Items;

        Debug.Log(rawItems == null ? "[MyBoothUI] rawItems == null" : $"[MyBoothUI] rawItems count = {rawItems.Count}");

        var mergedDict = new Dictionary<string, InventoryItem>();
        foreach (var inv in rawItems)
        {
            if (inv == null || inv.item == null) continue;

            string key = !string.IsNullOrEmpty(inv.item.item_id) ? inv.item.item_id : inv.item.item_icon;
            if (string.IsNullOrEmpty(key)) continue;

            if (mergedDict.ContainsKey(key)) mergedDict[key].count += 1;
            else
            {
                mergedDict[key] = inv;
                mergedDict[key].count = 1;
            }
        }

        _items = new List<InventoryItem>(mergedDict.Values);

        foreach (Transform c in slotParent)
            Destroy(c.gameObject);

        _slotPool.Clear();

        foreach (var invItem in _items)
        {
            if (invItem.item == null) continue;
            invItem.item.positionOffset = Vector3.zero; // 일단 단순화
        }

        BuildPoolIfNeeded();
        SetPage(0);
    }

    void BuildPoolIfNeeded()
    {
        while (_slotPool.Count < PageSize)
        {
            var s = Instantiate(slotPrefab, slotParent);
            s.SetActive(true);
            _slotPool.Add(s);
        }
    }

    void BindSlot(GameObject slot, InventoryItem inv)
    {
        var itemNameTxt = slot.transform.Find("Button/ItemName")?.GetComponent<TMP_Text>();
        if (itemNameTxt)
            itemNameTxt.text = inv.count > 1 ? $"{inv.item.item_name} x{inv.count}" : inv.item.item_name;

        var iconImg = slot.transform.Find("Button/ItemImage")?.GetComponent<Image>();
        var icon = Resources.Load<Sprite>("Icons/" + inv.item.item_icon);
        if (iconImg && icon) iconImg.sprite = icon;

        var btn = slot.transform.Find("Button")?.GetComponent<Button>();
        if (btn)
        {
            btn.onClick.RemoveAllListeners();
            var cap = inv.item;
            btn.onClick.AddListener(() =>
            {
                _current = cap;
                ShowDetail(cap);
            });
        }
    }

    void SetPage(int page)
    {
        int total = Mathf.Max(1, Mathf.CeilToInt((float)_items.Count / PageSize));
        _currentPage = Mathf.Clamp(page, 0, total - 1);

        for (int i = 0; i < _slotPool.Count; i++)
        {
            int idx = _currentPage * PageSize + i;
            if (idx < _items.Count)
            {
                _slotPool[i].SetActive(true);
                BindSlot(_slotPool[i], _items[idx]);
            }
            else _slotPool[i].SetActive(false);
        }

        if (pageLabel) pageLabel.text = $"{_currentPage + 1} / {total}";
        if (prevPageBtn) prevPageBtn.interactable = _currentPage > 0;
        if (nextPageBtn) nextPageBtn.interactable = _currentPage < total - 1;
    }

    void ShowDetail(ItemDataDTO item)
    {
        detailPanel.SetActive(true);
        if (infoGroup) infoGroup.SetActive(true);
        if (placeholderText) placeholderText.SetActive(false);

        detailName.text = item.item_name;
        detailDescription.text = item.item_description;

        var icon = Resources.Load<Sprite>("Icons/" + item.item_icon);
        if (icon && detailItemImage)
        {
            detailItemImage.sprite = icon;
            detailItemImage.preserveAspect = true;
            detailItemImage.enabled = true;
            var c = detailItemImage.color; c.a = 1f; detailItemImage.color = c;
        }

        putOnButton.onClick.RemoveAllListeners();
        putOnButton.GetComponentInChildren<TMP_Text>().text = "배치하기";
        putOnButton.onClick.AddListener(() => BeginPlacement(item, false)); // 마우스
    }

    Vector3 fixedPosition = new Vector3(-465f, 80f, 0f);

    void BeginPlacement(ItemDataDTO item, bool fromPad)
    {
        if (placement == null) return;

        isPlacing = true;
        CloseInventory();

        placement.rotateKey = rotateKey;

        placement.BeginPreview(item.item_icon, fixedPosition, item.positionOffset, fromPad);
    }

    public void BeginPlacementFromPad()
    {
        Debug.Log($"[BeginPlacementFromPad] current={_current}, isPlacing={isPlacing}");
        if (_current == null) return;

        BeginPlacement(_current, true); // 패드 배치
    }

    public void ShowDetailByIndex(int index)
    {
        if (_items == null || index < 0 || index >= _items.Count) return;

        _current = _items[index].item;
        ShowDetail(_current);
    }

    public GameObject GetSlotButton(int index)
    {
        if (index < 0 || index >= _slotPool.Count) return null;
        return _slotPool[index].transform.Find("Button").gameObject;
    }

    void OnEnable()
    {
        if (padInput != null)
            padInput.OnRPressed += ToggleFromBooth;

        if (padInput != null)
            padInput.OnYPressed += OnExitBooth;
    }

    void OnDisable()
    {
        if (padInput != null)
            padInput.OnRPressed -= ToggleFromBooth;

        if (padInput != null)
            padInput.OnYPressed -= OnExitBooth;
    }

    void ToggleFromBooth()
    {
        if (inventoryPanel.activeSelf) CloseInventory();
        else OpenInventory();
    }
    void OnExitBooth()
    {
        boothManager.SaveAndExit();
    }


}
