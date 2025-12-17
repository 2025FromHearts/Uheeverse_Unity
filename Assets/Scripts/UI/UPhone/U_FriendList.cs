using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class FriendData
{
    public string character_id;
    public string character_name;
    public string last_login;
    public string last_festival;
    public string character_style;
}

public class U_FriendList : MonoBehaviour
{
    [Header("UI")]
    public GameObject friendPanel;
    public Transform friendsParent;
    public GameObject friendPrefab;

    [Header("Input")]
    public PadInputEventRouter padInput;

    [Header("Overlay")]
    public GameObject plusOverlayPanel;
    public TMP_InputField nicknameInput;

    [Header("Pad")]
    public PadCommandSender padSender;
    public UdpPadReceiver pad;

    [Header("Search")]
    public U_SearchFriend searchFriend;

    private string baseUrl = ServerConfig.baseUrl;
    bool isInitialized = false;
    public ListNavigation listNavigation;

    /* ===================== LifeCycle ===================== */

    void Start()
    {
        OpenFriendPanel();
    }

    void Update()
    {
        if (pad == null || pad.latest == null) return;
        if (!friendPanel.activeInHierarchy) return;
        if (listNavigation == null) return;

        float y = pad.latest.ly;
        if (Mathf.Abs(y) < 0.6f) return;

        // 1하이라이트 이동 (index 변경)
        if (y > 0)
            listNavigation.MoveUp();
        else
            listNavigation.MoveDown();

        // 2스크롤 이동
        ScrollRect sr = listNavigation.scrollRect;
        if (sr != null)
        {
            float speed = 0.8f * Time.unscaledDeltaTime;
            sr.verticalNormalizedPosition = Mathf.Clamp01(
                sr.verticalNormalizedPosition + (y > 0 ? speed : -speed)
            );
        }
    }

    void ResetListState()
    {
        listNavigation.scrollRect.verticalNormalizedPosition = 1f;
        listNavigation.SetItems(new List<MonoBehaviour>(), false);
    }

    void OnEnable()
    {
        if (padInput != null)
        {
            padInput.OnPlusPressed += OnPlus;
            padInput.OnBPressed += OnB;
        }

        PadCommandReceiver.OnSubmitText += ApplyNickname;
    }

    void OnDisable()
    {
        if (padInput != null)
        {
            padInput.OnPlusPressed -= OnPlus;
            padInput.OnBPressed -= OnB;
        }

        PadCommandReceiver.OnSubmitText -= ApplyNickname;
        ResetState();

    }

    void OnPlus()
    {
        if (!friendPanel.activeInHierarchy) return;

        plusOverlayPanel.SetActive(true);

        nicknameInput.text = "";
        EventSystem.current.SetSelectedGameObject(nicknameInput.gameObject);
        nicknameInput.ActivateInputField();


        if (padSender != null)
            padSender.SendOpenTextInput("nickname");

        Debug.Log("[FriendList] Plus → Overlay Open + Pad Keyboard Request");
    }

    void OnB()
    {
        if (!friendPanel.activeInHierarchy) return;
        if (padInput.currentMode != PadInputEventRouter.InputMode.UPhone) return;
        if (listNavigation == null || listNavigation.scrollRect == null) return;

        ResetListState();
        plusOverlayPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        padInput.currentMode = PadInputEventRouter.InputMode.UPhone;
        return;
    }


    void ApplyNickname(string text)
    {
        nicknameInput.text = text;
        nicknameInput.caretPosition = text.Length;
        nicknameInput.ForceLabelUpdate();

        EventSystem.current.SetSelectedGameObject(null);

        StartCoroutine(searchFriend.SearchFriends(text));
    }

    public void ResetState()
    {
        if (listNavigation != null)
            listNavigation.ResetToTop();
    }
    public void OpenFriendPanel()
    {
        if (friendPanel.activeSelf) return;

        friendPanel.SetActive(true);
        StartCoroutine(OpenWithDelay());
    }

    IEnumerator OpenWithDelay()
    {
        yield return null;
        yield return RefreshFriends();
    }

    public IEnumerator RefreshFriends()
    {
        string token = PlayerPrefs.GetString("access_token", "");
        if (string.IsNullOrEmpty(token)) yield break;

        string url = $"{baseUrl}/social/friends/list/";
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + token);

        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success) yield break;

        foreach (Transform child in friendsParent)
            Destroy(child.gameObject);

        var results = JsonConvert.DeserializeObject<List<FriendData>>(www.downloadHandler.text);
        if (results == null) yield break;

        List<MonoBehaviour> uiList = new List<MonoBehaviour>();

        foreach (var f in results)
        {
            var obj = Instantiate(friendPrefab, friendsParent);
            var ui = obj.GetComponent<FriendListUI>();
            if (ui != null)
            {
                yield return null;
                ui.SetData(f);
                uiList.Add(ui);
            }
        }
        listNavigation.SetItems(uiList);
    }
}
