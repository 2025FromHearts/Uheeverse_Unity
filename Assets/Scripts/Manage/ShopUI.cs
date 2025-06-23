using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using NUnit.Framework.Interfaces;

public class ShopUI : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject shopPanel;
    public GameObject closeButtonObject;
    public Transform slotParent;
    public GameObject slotPrefab;
    public string mapId;

    [Header("상세 정보 패널")]
    public GameObject detailPanel;
    public Image detailItemImage;
    public TMP_Text detailName;
    public TMP_Text detailDescription;
    public Button purchaseButton;

    public GameObject infoGroup;
    public GameObject placeholderText;

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

    public void OnSlotClicked(ItemDataDTO item)
    {
        currentSelectedItem = item;
        ShowDetail(item);
    }

    public void OpenShop()
    {
        shopPanel.SetActive(true);
        detailPanel.SetActive(true);

        if (infoGroup != null) infoGroup.SetActive(false);
        if (placeholderText != null) placeholderText.SetActive(true);

        StartCoroutine(LoadShopItems());
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
        detailPanel.SetActive(false);

        if (infoGroup != null) infoGroup.SetActive(false);
        if (placeholderText != null) placeholderText.SetActive(false);
    }

    IEnumerator LoadShopItems()
    {
        baseUrl = ServerConfig.baseUrl;
        accessToken = PlayerPrefs.GetString("access_token");
        Debug.Log("🔥 토큰 확인: " + accessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.LogError("❌ access_token이 없습니다. 로그인 먼저 하세요.");
            yield break;
        }

        string url = baseUrl + "/item/items/map/" + mapId + "/";
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Shop item load failed: " + www.error);
        }
        else
        {
            List<ItemDataDTO> items = JsonUtilityWrapper.FromJsonList<ItemDataDTO>(www.downloadHandler.text);

            foreach (Transform child in slotParent)
                Destroy(child.gameObject);

            foreach (var item in items)
            {
                GameObject slot = Instantiate(slotPrefab, slotParent);

                TMP_Text text = slot.transform.Find("Button/ItemName")?.GetComponent<TMP_Text>();
                if (text != null)
                    text.text = item.item_name;

                Transform iconTransform = slot.transform.Find("Button/ItemImage");
                if (iconTransform != null)
                {
                    Image iconImage = iconTransform.GetComponent<Image>();
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

        purchaseButton.onClick.RemoveAllListeners();
        purchaseButton.onClick.AddListener(() => PurchaseItem(item));
    }

    void PurchaseItem(ItemDataDTO item)
    {
        Debug.Log($"💰 구매 시도: {item.item_name} (가격: {item.item_price})");
        StartCoroutine(SendPurchaseRequest(item));
    }

    IEnumerator SendPurchaseRequest(ItemDataDTO item)
    {
        string accessToken = PlayerPrefs.GetString("access_token");
        string url = $"{ServerConfig.baseUrl}/item/inventory/add/";

        WWWForm form = new WWWForm();
        form.AddField("item_id", item.item_id);

        string characterId = PlayerPrefs.GetString("character_id");
        form.AddField("character_id", characterId);

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        www.SetRequestHeader("Authorization", "Bearer " + accessToken.Trim());

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"❌ 구매 실패: {www.error}, URL: {url}, Code: {www.responseCode}");
            Debug.LogError($"응답 내용: {www.downloadHandler.text}");
        }
        else
        {
            Debug.Log($"✅ 구매 성공: {item.item_name}, 응답: {www.downloadHandler.text}");
        }
    }
}

// JsonUtility가 List를 파싱하지 못하므로 wrapper 사용
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


