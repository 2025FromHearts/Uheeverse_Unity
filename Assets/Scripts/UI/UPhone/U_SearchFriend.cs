using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class U_SearchFriend : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField searchInput;
    public Transform resultsParent;
    public GameObject resultPrefab;

    [Header("Pad Input")]
    public PadInputEventRouter padInput;
    public UdpPadReceiver pad;

    [Header("Pad Navigation")]
    public float moveThreshold = 0.6f;
    public float moveDelay = 0.25f;
    float lastMoveTime = 0f;

    [Header("Scroll")]
    public ScrollRect scrollRect;

    private string baseUrl = ServerConfig.baseUrl;
    private Coroutine searchCoroutine;

    private List<FriendResultUI> resultUIs = new List<FriendResultUI>();
    private int currentIndex = 0;

    [System.Serializable]
    public class CharacterResult
    {
        public string character_id;
        public string character_name;
        public string character_style;
    }

    void Start()
    {
        searchInput.onValueChanged.AddListener(OnSearchInputChanged);
    }

    void OnEnable()
    {
        if (padInput != null)
            padInput.OnAPressed += OnAPressed;
    }

    void OnDisable()
    {
        if (padInput != null)
            padInput.OnAPressed -= OnAPressed;
        ResetState();
    }

    void Update()
    {
        if (pad == null || pad.latest == null) return;
        if (resultUIs.Count == 0) return;
        if (Time.time - lastMoveTime < moveDelay) return;

        float y = pad.latest.ly;
        if (Mathf.Abs(y) < moveThreshold) return;

        // 하이라이트 이동
        if (y > 0)
            MoveUp();
        else
            MoveDown();

        // 2스크롤 이동
        if (scrollRect != null)
        {
            float speed = 0.8f * Time.unscaledDeltaTime;
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(
                scrollRect.verticalNormalizedPosition + (y > 0 ? speed : -speed)
            );
        }

        lastMoveTime = Time.time;
    }
    void MoveUp()
    {
        int next = currentIndex - 1;
        if (next < 0) return;

        SetHighlight(next);
        lastMoveTime = Time.time;
    }

    public void ResetState()
    {
        currentIndex = 0;

        for (int i = 0; i < resultUIs.Count; i++)
            resultUIs[i].SetHighlight(i == 0);

        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
    }

    void MoveDown()
    {
        int next = currentIndex + 1;
        if (next >= resultUIs.Count) return;

        SetHighlight(next);
        lastMoveTime = Time.time;
    }

    void OnSearchInputChanged(string query)
    {
        if (searchCoroutine != null)
            StopCoroutine(searchCoroutine);

        searchCoroutine = StartCoroutine(DebouncedSearch(query));
    }

    IEnumerator DebouncedSearch(string query)
    {
        yield return new WaitForSeconds(0.3f);

        if (!string.IsNullOrEmpty(query))
            yield return StartCoroutine(SearchFriends(query));
    }

    public IEnumerator SearchFriends(string query)
    {
        padInput.currentMode = PadInputEventRouter.InputMode.Popup;

        string token = PlayerPrefs.GetString("access_token", "");
        if (string.IsNullOrEmpty(token)) yield break;

        string url = $"{baseUrl}/social/friends/search/?q={UnityWebRequest.EscapeURL(query)}";
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + token);

        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success) yield break;

        foreach (Transform child in resultsParent)
            Destroy(child.gameObject);

        resultUIs.Clear();
        currentIndex = 0;

        List<CharacterResult> results =
            JsonConvert.DeserializeObject<List<CharacterResult>>(www.downloadHandler.text);

        foreach (var c in results)
        {
            GameObject obj = Instantiate(resultPrefab, resultsParent);
            FriendResultUI ui = obj.GetComponent<FriendResultUI>();
            if (ui)
            {
                ui.SetData(c, this);
                ui.SetHighlight(false);
                resultUIs.Add(ui);
            }
        }

        if (resultUIs.Count > 0)
            SetHighlight(0);
    }

    void SetHighlight(int index)
    {
        if (index < 0 || index >= resultUIs.Count) return;

        for (int i = 0; i < resultUIs.Count; i++)
            resultUIs[i].SetHighlight(i == index);

        currentIndex = index;
    }

    void OnAPressed()
    {
        if (padInput.currentMode != PadInputEventRouter.InputMode.Popup)
            return;

        if (resultUIs.Count == 0) return;

        resultUIs[currentIndex].ConfirmAddFriend();
    }

    public IEnumerator AddFriendRequest(string targetId)
    {
        string token = PlayerPrefs.GetString("access_token", "");
        if (string.IsNullOrEmpty(token)) yield break;

        string url = $"{baseUrl}/social/friends/add/";
        WWWForm form = new WWWForm();
        form.AddField("target_id", targetId);

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        www.SetRequestHeader("Authorization", "Bearer " + token);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
            Debug.Log("✅ 친구 추가 성공");
        else
            Debug.LogError("❌ 친구 추가 실패: " + www.error);
    }
}
