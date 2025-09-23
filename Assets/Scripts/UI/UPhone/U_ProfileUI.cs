using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Globalization;

public class U_ProfileUI : MonoBehaviour
{
    [Header("UI Reference")]
    public TMP_Text lastLoginText;
    public TMP_Text itemCountText;

    private string userInfoUrl;

    void Awake()
    {
        userInfoUrl = $"{ServerConfig.baseUrl}/users/get_my_Uprofile/";
    }

    // 메인 버튼 OnClick에 연결할 함수
    public void OpenProfile()
    {
        StartCoroutine(GetUserInfo());
    }

    IEnumerator GetUserInfo()
    {
        string token = PlayerPrefs.GetString("access_token", "");
        Debug.Log("👉 불러온 access_token: " + token);

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("❌ access_token이 비어 있음. 로그인 단계에서 저장됐는지 확인 필요");
            yield break;
        }

        UnityWebRequest www = UnityWebRequest.Get(userInfoUrl);
        www.SetRequestHeader("Authorization", "Bearer " + token);

        yield return www.SendWebRequest();

        Debug.Log("📡 UserInfo Raw Response: " + www.downloadHandler.text);
        Debug.Log("📡 lastLoginText 연결됨?: " + (lastLoginText != null));
        Debug.Log("📡 itemCountText 연결됨?: " + (itemCountText != null));

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ User Info API 실패: " + www.error);
        }
        else
        {
            try
            {
                UserInfoResponse data = JsonUtility.FromJson<UserInfoResponse>(www.downloadHandler.text);

                // 최근 접속 날짜 갱신
                string formattedDate = FormatDate(data.last_login);
                lastLoginText.text = $"{formattedDate}";

                // 아이템 개수 요청 이어서 실행
                StartCoroutine(GetInventoryCount(data.character_id));
            }
            catch (Exception e)
            {
                Debug.LogError("❌ JSON 파싱 실패: " + e.Message);
                lastLoginText.text = "최근 접속: 파싱 실패";
            }
        }
    }

    IEnumerator GetInventoryCount(string characterId)
    {
        string token = PlayerPrefs.GetString("access_token", "");
        string url = $"{ServerConfig.baseUrl}/item/inventory/count/{characterId}/";

        Debug.Log("👉 호출 URL: " + url);
        Debug.Log("👉 access_token: " + token);

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("❌ access_token이 비어 있음. 로그인 단계에서 저장됐는지 확인 필요");
            yield break;
        }

        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + token);

        yield return www.SendWebRequest();

        Debug.Log("📡 Inventory Raw Response: " + www.downloadHandler.text);

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Inventory Count API 실패: " + www.error);
        }
        else
        {
            try
            {
                InventoryCountResponse data = JsonUtility.FromJson<InventoryCountResponse>(www.downloadHandler.text);
                itemCountText.text = $"{data.inventory_count}";
            }
            catch (Exception e)
            {
                Debug.LogError("❌ JSON 파싱 실패: " + e.Message);
                itemCountText.text = "아이템 수: 파싱 실패";
            }
        }
    }

    private string FormatDate(string isoDate)
    {
        if (string.IsNullOrEmpty(isoDate)) return "-";
        try
        {
            DateTime parsedDate = DateTime.Parse(isoDate, null, DateTimeStyles.RoundtripKind);
            return parsedDate.ToString("yyyy-MM-dd");
        }
        catch (Exception e)
        {
            Debug.LogError("❌ 날짜 변환 실패: " + e.Message);
            return isoDate;
        }
    }

    [System.Serializable]
    public class UserInfoResponse
    {
        public string character_id;
        public string character_name;
        public string last_login;
    }

    [System.Serializable]
    public class InventoryCountResponse
    {
        public string character_id;
        public int inventory_count;
    }
}
