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
}

public class U_FriendList : MonoBehaviour
{
    [Header("UI")]
    public GameObject friendPanel;        // 친구 패널 (Inspector에 연결)
    public Transform friendsParent;       // 친구 리스트 Content (Vertical Layout Group)
    public GameObject friendPrefab;       // 프리팹 (FriendListUI 붙어 있어야 함)

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
        yield return null; // 한 프레임 대기 (UI 레이아웃 갱신 시간 확보)
        yield return RefreshFriends();
    }

    public IEnumerator RefreshFriends()
    {
        Debug.Log("📡 친구 목록 불러오기 시작");

        string token = PlayerPrefs.GetString("access_token", "");
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("❌ Access token 없음! 로그인 먼저 필요");
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

        Debug.Log("📡 Raw Response: " + www.downloadHandler.text);

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
            Debug.LogError("❌ JSON 파싱 실패: " + e.Message);
        }

        if (results == null || results.Count == 0)
        {
            Debug.LogWarning("⚠️ 친구 데이터 없음");
            yield break;
        }

        Debug.Log($"📡 파싱된 친구 수: {results.Count}");

        foreach (var f in results)
        {
            Debug.Log($"👉 프리팹 생성 시도: {f.character_name} ({f.character_id})");

            GameObject obj = Instantiate(friendPrefab, friendsParent);
            Debug.Log("✅ 생성됨: " + obj.name + " / 부모: " + obj.transform.parent.name);

            FriendListUI ui = obj.GetComponent<FriendListUI>();
            if (ui != null)
            {
                ui.SetData(f);
            }
            else
            {
                Debug.LogError("❌ FriendListUI 컴포넌트 없음!");
            }
        }
    }
}
