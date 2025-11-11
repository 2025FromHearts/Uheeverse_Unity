using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;

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

    private string baseUrl = ServerConfig.baseUrl;

    // 버튼 OnClick에 연결할 함수
    public void OpenFriendPanel()
    {
        if (friendPanel != null)
        {
            friendPanel.SetActive(true);
        }

        // 패널 활성화 후 코루틴 실행
        StartCoroutine(OpenWithDelay());
    }

    private IEnumerator OpenWithDelay()
    {
        yield return null;
        yield return RefreshFriends();
    }

    public IEnumerator RefreshFriends()
    {

        string token = PlayerPrefs.GetString("access_token", "");
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("❌ Access token 없음, 로그인 먼저 필요");
            yield break;
        }

        string url = $"{baseUrl}/social/friends/list/";
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + token);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ 친구 목록 API 실패: " + www.error + "\n응답: " + www.downloadHandler.text);
            yield break;
        }

        // 기존 항목 삭제
        foreach (Transform child in friendsParent)
        {
            Destroy(child.gameObject);
        }

        // 응답 파싱
        List<FriendData> results = null;
        try
        {
            results = JsonConvert.DeserializeObject<List<FriendData>>(www.downloadHandler.text);
        }
        catch (System.Exception e)
        {
            Debug.LogError("JSON 파싱 실패: " + e.Message);
        }

        if (results == null || results.Count == 0)
        {
            Debug.LogWarning("친구 데이터 없음");
            yield break;
        }

        foreach (var f in results)
        {
            GameObject obj = Instantiate(friendPrefab, friendsParent);
            Debug.Log("생성 완료: " + obj.name);

            FriendListUI ui = obj.GetComponent<FriendListUI>();
            if (ui != null)
            {
                ui.SetData(f);
            }
        }
    }
}
