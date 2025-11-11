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
    public TMP_InputField searchInput;      // 닉네임 입력칸
    public Transform resultsParent;         // 결과 패널 (Vertical Layout Group)
    public GameObject resultPrefab;         // 결과 프리팹 (FriendResultUI 붙어있음)

    private string baseUrl = ServerConfig.baseUrl;
    private Coroutine searchCoroutine;

    [System.Serializable]
    public class CharacterResult
    {
        public string character_id;     // 서버 DB의 캐릭터 ID
        public string character_name;   // 캐릭터 닉네임
        public string character_style;  // 캐릭터 스타일명
    }

    void Start()
    {
        searchInput.onValueChanged.AddListener(OnSearchInputChanged);
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

    IEnumerator SearchFriends(string query)
    {
        string token = PlayerPrefs.GetString("access_token", "");

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("❌ Access token 없음! 로그인 먼저 필요");
            yield break;
        }

        string url = $"{baseUrl}/social/friends/search/?q={UnityWebRequest.EscapeURL(query)}";
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + token);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Friend search failed: " + www.error + "\n응답: " + www.downloadHandler.text);
            yield break;
        }

        // 기존 결과 삭제
        foreach (Transform child in resultsParent)
            Destroy(child.gameObject);

        // JSON 파싱
        List<CharacterResult> results = JsonConvert.DeserializeObject<List<CharacterResult>>(www.downloadHandler.text);

        foreach (var c in results)
        {
            GameObject obj = Instantiate(resultPrefab, resultsParent);
            FriendResultUI ui = obj.GetComponent<FriendResultUI>();
            if (ui)
            {
                ui.SetData(c, this);
            }
        }
    }

    public IEnumerator AddFriendRequest(string targetId)
    {
        string token = PlayerPrefs.GetString("access_token", "");
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("❌ Access token 없음. 로그인 필요");
            yield break;
        }

        string url = $"{baseUrl}/social/friends/add/";

        WWWForm form = new WWWForm();
        form.AddField("target_id", targetId);

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        www.SetRequestHeader("Authorization", "Bearer " + token);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ 친구 추가 실패: " + www.error + "\n응답: " + www.downloadHandler.text);
        }
        else
        {
            Debug.Log("✅ 친구 추가 성공: " + www.downloadHandler.text);
        }
    }
}
