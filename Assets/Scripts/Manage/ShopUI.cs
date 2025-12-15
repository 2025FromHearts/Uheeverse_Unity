using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class ShopUI : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject shopPanel;
    public Transform slotParent;
    public GameObject slotPrefab;
    public string mapId;
    public CanvasGroup shopCanvasGroup;

    [Header("상세 정보 패널")]
    public GameObject detailPanel;
    public Image detailItemImage;
    public TMP_Text detailName;
    public TMP_Text detailDescription;
    public Button purchaseButton;

    [Header("Pad Input")]
    public PadInputEventRouter padInput;
    public float moveDelay = 0.25f;

    float lastMoveTime;

    public GameObject infoGroup;
    public GameObject placeholderText;

    private ItemDataDTO currentSelectedItem;
    private string baseUrl;
    private string accessToken;

    List<ShopSlot> slots = new List<ShopSlot>();
    int currentIndex = 0;

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
    }

    public void OpenShop()
    {
        shopPanel.SetActive(true);

        if (shopCanvasGroup != null)
        {
            shopCanvasGroup.alpha = 1f;
            shopCanvasGroup.interactable = true;
            shopCanvasGroup.blocksRaycasts = true;
        }

        detailPanel.SetActive(true);
        infoGroup.SetActive(false);
        placeholderText.SetActive(true);

        StartCoroutine(LoadShopItems());
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);

        if (shopCanvasGroup != null)
        {
            shopCanvasGroup.alpha = 0f;
            shopCanvasGroup.interactable = false;
            shopCanvasGroup.blocksRaycasts = false;
        }

        // 상태 초기화
        currentSelectedItem = null;
        slots.Clear();
        currentIndex = 0;

        // UI 초기화
        if (detailPanel != null) detailPanel.SetActive(false);
        if (infoGroup != null) infoGroup.SetActive(false);
        if (placeholderText != null) placeholderText.SetActive(true);
    }

    void Update()
    {
        if (!shopPanel.activeInHierarchy) return;
        if (slots.Count == 0) return;

        var pad = FindAnyObjectByType<UdpPadReceiver>();
        if (pad == null || pad.latest == null) return;

        if (Time.unscaledTime - lastMoveTime < moveDelay) return;

        float x = pad.latest.lx;

        if (x < -0.6f)
        {
            MoveSelection(-1);
            lastMoveTime = Time.unscaledTime;
        }
        else if (x > 0.6f)
        {
            MoveSelection(1);
            lastMoveTime = Time.unscaledTime;
        }
    }
    void OnEnable()
    {
        if (padInput != null)
            padInput.OnAPressed += OnAPressed;

        if (padInput == null) return;
        padInput.OnXPressed += OnXPressed;
    }


    void OnDisable()
    {
        if (padInput != null)
            padInput.OnAPressed -= OnAPressed;
        if (padInput == null) return;
        padInput.OnXPressed -= OnXPressed;
    }
    void OnXPressed()
    {
        if (!shopPanel.activeInHierarchy) return;
        CloseShop();
    }
    void OnAPressed()
    {
        if (!shopPanel.activeInHierarchy) return;

        // 상세가 아직 안 열렸으면 선택
        if (currentSelectedItem == null)
        {
            ConfirmSelection(); // 기존 동작
        }
        else
        {
            ConfirmPurchase(); // 구매
        }
    }
    void MoveSelection(int dir)
    {
        if (slots.Count == 0) return;

        int next = Mathf.Clamp(currentIndex + dir, 0, slots.Count - 1);
        if (next == currentIndex) return;

        UpdateSelection(next);
    }
    public void ConfirmPurchase()
    {
        if (currentSelectedItem == null) return;
        PurchaseItem(currentSelectedItem);
    }

    IEnumerator LoadShopItems()
    {
        baseUrl = ServerConfig.baseUrl;
        accessToken = PlayerPrefs.GetString("access_token");

        if (string.IsNullOrEmpty(accessToken))
            yield break;

        string url = baseUrl + "/item/items/map/" + mapId + "/";
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
            yield break;

        List<ItemDataDTO> items =
            JsonUtilityWrapper.FromJsonList<ItemDataDTO>(www.downloadHandler.text);

        foreach (Transform child in slotParent)
            Destroy(child.gameObject);

        slots.Clear();
        currentIndex = 0;

        foreach (var item in items)
        {
            GameObject obj = Instantiate(slotPrefab, slotParent);
            ShopSlot slot = obj.GetComponent<ShopSlot>();

            if (slot != null)
            {
                slot.Set(item, this);
                slots.Add(slot);
            }
        }

        if (slots.Count > 0)
            UpdateSelection(0);
    }
    void UpdateSelection(int index)
    {
        currentIndex = Mathf.Clamp(index, 0, slots.Count - 1);

        for (int i = 0; i < slots.Count; i++)
            slots[i].SetSelected(i == currentIndex);

        slots[currentIndex].Select();
    }

    public void OnSlotClicked(ItemDataDTO item)
    {
        currentSelectedItem = item;
        ShowDetail(item);
    }

    public void ConfirmSelection()
    {
        if (slots.Count == 0) return;
        slots[currentIndex].Select();
    }


    void ShowDetail(ItemDataDTO item)
    {
        detailPanel.SetActive(true);
        infoGroup.SetActive(true);
        placeholderText.SetActive(false);

        detailName.text = item.item_name;
        detailDescription.text = item.item_description;

        Sprite icon = Resources.Load<Sprite>("Icons/" + item.item_icon);
        if (icon != null)
            detailItemImage.sprite = icon;

        purchaseButton.onClick.RemoveAllListeners();
        purchaseButton.onClick.AddListener(() => PurchaseItem(item));
    }

    void PurchaseItem(ItemDataDTO item)
    {
        StartCoroutine(SendPurchaseRequest(item));
    }

    IEnumerator SendPurchaseRequest(ItemDataDTO item)
    {
        string url = $"{ServerConfig.baseUrl}/item/inventory/add/";
        WWWForm form = new WWWForm();
        form.AddField("item_id", item.item_id);
        form.AddField("character_id", PlayerPrefs.GetString("character_id"));

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return www.SendWebRequest();
    }

    public static class JsonUtilityWrapper
    {
        public static List<T> FromJsonList<T>(string json)
        {
            string wrappedJson = "{\"Items\":" + json + "}";
            return JsonUtility.FromJson<Wrapper<T>>(wrappedJson).Items;
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public List<T> Items;
        }
    }
}
