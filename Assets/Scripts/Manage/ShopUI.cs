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
    public GameObject closeButtonObject;
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

    public GameObject infoGroup;
    public GameObject placeholderText;

    [Header("구매 알림 UI")]
    public GameObject alarmRoot;
    public TMP_Text alarmText;
    public float alarmDuration = 2f;

    Coroutine alarmCoroutine;

    private ItemDataDTO currentSelectedItem;
    private string baseUrl;
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
    }

    /* ===================== UI OPEN / CLOSE ===================== */

    public void OpenShop()
    {
        if (shopCanvasGroup != null)
        {
            shopCanvasGroup.alpha = 1f;
            shopCanvasGroup.interactable = true;
            shopCanvasGroup.blocksRaycasts = true;
        }

        shopPanel.SetActive(true);
        detailPanel.SetActive(true);

        if (infoGroup != null) infoGroup.SetActive(false);
        if (placeholderText != null) placeholderText.SetActive(true);

        StartCoroutine(LoadShopItems());
    }

    public void CloseShop()
    {
        if (shopCanvasGroup != null)
        {
            shopCanvasGroup.alpha = 0f;
            shopCanvasGroup.interactable = false;
            shopCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            shopPanel.SetActive(false);
            detailPanel.SetActive(false);
        }

        if (infoGroup != null) infoGroup.SetActive(false);
        if (placeholderText != null) placeholderText.SetActive(false);
    }

    /* ===================== ITEM LOAD ===================== */

    IEnumerator LoadShopItems()
    {
        baseUrl = ServerConfig.baseUrl;
        accessToken = PlayerPrefs.GetString("access_token");

        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.LogError("❌ access_token이 없습니다.");
            yield break;
        }

        string url = baseUrl + "/item/items/map/" + mapId + "/";
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Shop item load failed: " + www.error);
            yield break;
        }

        List<ItemDataDTO> items =
            JsonUtilityWrapper.FromJsonList<ItemDataDTO>(www.downloadHandler.text);

        foreach (Transform child in slotParent)
            Destroy(child.gameObject);

        foreach (var item in items)
        {
            GameObject slot = Instantiate(slotPrefab, slotParent);

            TMP_Text text = slot.transform.Find("Button/ItemName")
                ?.GetComponent<TMP_Text>();
            if (text != null)
                text.text = item.item_name;

            Image iconImage = slot.transform.Find("Button/ItemImage")
                ?.GetComponent<Image>();
            if (iconImage != null)
            {
                Sprite iconSprite = Resources.Load<Sprite>("Icons/" + item.item_icon);
                if (iconSprite != null)
                    iconImage.sprite = iconSprite;
            }

            Button btn = slot.transform.Find("Button")?.GetComponent<Button>();
            if (btn != null)
            {
                ItemDataDTO capturedItem = item;
                btn.onClick.AddListener(() =>
                {
                    currentSelectedItem = capturedItem;
                    ShowDetail(capturedItem);
                });
            }
        }
    }

    /* ===================== DETAIL ===================== */

    void ShowDetail(ItemDataDTO item)
    {
        if (item == null) return;

        detailPanel.SetActive(true);
        if (infoGroup != null) infoGroup.SetActive(true);
        if (placeholderText != null) placeholderText.SetActive(false);

        detailName.text = item.item_name;
        detailDescription.text = item.item_description;

        Sprite icon = Resources.Load<Sprite>("Icons/" + item.item_icon);
        if (icon != null)
            detailItemImage.sprite = icon;

        purchaseButton.onClick.RemoveAllListeners();
        purchaseButton.onClick.AddListener(() => PurchaseItem(item));
    }

    public void OnSlotClicked(ItemDataDTO item)
    {
        currentSelectedItem = item;
        ShowDetail(item);
    }

    /* ===================== PURCHASE ===================== */

    void PurchaseItem(ItemDataDTO item)
    {
        Debug.Log($"구매 시도: {item.item_name}");
        StartCoroutine(SendPurchaseRequest(item));
    }

    IEnumerator SendPurchaseRequest(ItemDataDTO item)
    {
        string url = $"{ServerConfig.baseUrl}/item/inventory/add/";

        WWWForm form = new WWWForm();
        form.AddField("item_id", item.item_id);
        form.AddField("character_id", PlayerPrefs.GetString("character_id"));

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        www.SetRequestHeader("Authorization", "Bearer " + accessToken.Trim());

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            ShowAlarm("구매 완료! 인벤토리를 확인해보세요.");
        }
        else if (www.responseCode == 400)
        {
            ShowAlarm("코인이 부족합니다.");
        }
        else
        {
            ShowAlarm("구매에 실패했습니다.");
            Debug.LogError(www.error);
        }
    }

    /* ===================== ALARM ===================== */

    void ShowAlarm(string message)
    {
        if (alarmCoroutine != null)
            StopCoroutine(alarmCoroutine);

        alarmCoroutine = StartCoroutine(AlarmRoutine(message));
    }

    IEnumerator AlarmRoutine(string message)
    {
        alarmRoot.SetActive(true);
        alarmText.text = message;

        yield return new WaitForSecondsRealtime(alarmDuration);

        alarmRoot.SetActive(false);
    }
}

/* ===================== JSON LIST WRAPPER ===================== */

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
