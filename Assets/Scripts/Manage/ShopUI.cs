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
    public GameObject closeButtonObject;

    [Header("설정값")]
    public string mapId;  
    
    [Header("구매 확인창")]
    public GameObject confirmPanel;
    public TMP_Text confirmText;
    public Button confirmYesButton;
    public Button confirmNoButton;

    private ItemData pendingItem; 
    public void OpenShop()  
    {
        shopPanel.SetActive(true);

        if (closeButtonObject != null)
            closeButtonObject.SetActive(true);

        StartCoroutine(LoadShop());
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
    }

    private IEnumerator LoadShop()
    {
        string url = ServerConfig.baseUrl + "/item/items/map/" + mapId + "/";
        string accessToken = PlayerPrefs.GetString("access_token");

        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ 아이템 로드 실패: " + www.error);
            yield break;
        }

        string wrappedJson = "{\"Items\":" + www.downloadHandler.text + "}";
        ShopWrapper wrapper = JsonUtility.FromJson<ShopWrapper>(wrappedJson);

        foreach (Transform child in slotParent)
            Destroy(child.gameObject);

        foreach (var item in wrapper.Items)
        {
            GameObject slot = Instantiate(slotPrefab, slotParent);

            TMP_Text text = slot.transform.Find("Button/ItemName")?.GetComponent<TMP_Text>();
            if (text != null) text.text = item.item_name;

            Image icon = slot.transform.Find("Button/ItemImage")?.GetComponent<Image>();
            if (icon != null)
            {
                Sprite iconSprite = Resources.Load<Sprite>("Icons/" + item.item_icon);
                icon.sprite = iconSprite;
            }

            Button btn = slot.transform.Find("Button")?.GetComponent<Button>();
            if (btn != null)
            {
                ItemData captured = item;
                btn.onClick.AddListener(() => ShowPurchaseConfirmation(captured));
            }
        }
    }
    void ShowPurchaseConfirmation(ItemData item)
    {
        pendingItem = item;

        confirmText.text = $"‘{item.item_name}’\n구매하시겠습니까?";
        confirmPanel.SetActive(true);

        confirmYesButton.onClick.RemoveAllListeners();
        confirmNoButton.onClick.RemoveAllListeners();

        confirmYesButton.onClick.AddListener(() => ConfirmPurchase());
        confirmNoButton.onClick.AddListener(() => confirmPanel.SetActive(false));
    }
    void ConfirmPurchase()
    {
        confirmPanel.SetActive(false);
        StartCoroutine(SendPurchaseRequest(pendingItem));
    }

    private IEnumerator SendPurchaseRequest(ItemData item)
    {
        string url = ServerConfig.baseUrl + "/item/inventory/add/";
        string accessToken = PlayerPrefs.GetString("access_token");
        string characterId = PlayerPrefs.GetString("character_id");

        if (string.IsNullOrEmpty(characterId))
        {
            Debug.LogError("❌ 캐릭터 ID 없음. 로그인 또는 캐릭터 초기화 확인");
            yield break;
        }

        Dictionary<string, object> data = new Dictionary<string, object>()
    {
        { "character_id", characterId },
        { "item_id", item.item_id },
        { "slot_location", 0 }
    };

        string jsonData = JsonUtility.ToJson(new Wrapper(data));

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);
        Debug.Log("요청 주소 확인: " + url);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ 구매 요청 실패: " + request.error);
        }
        else
        {
            Debug.Log("✅ 아이템 구매 완료 및 인벤토리 추가됨: " + item.item_name);
            // 여기에 인벤토리 UI 갱신 넣기
        }
    }

    // Dictionary용 JsonUtility 변환 우회
    [System.Serializable]
    public class Wrapper
    {
        public string character_id;
        public string item_id;
        public int slot_location;

        public Wrapper(Dictionary<string, object> dict)
        {
            character_id = dict["character_id"].ToString();
            item_id = dict["item_id"].ToString();
            slot_location = (int)dict["slot_location"];
        }
    }


    [System.Serializable]
    public class ShopWrapper
    {
        public List<ItemData> Items;
    }

    void EquipItem(ItemData item)
    {
        Debug.Log("장착: " + item.item_name);
    }
}
