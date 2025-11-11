using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine.UI;

public class U_ProfileUI : MonoBehaviour
{
    [Header("UI Reference")]
    public TMP_Text nameText;
    public TMP_Text introText;
    public TMP_Text lastLoginText;
    public TMP_Text itemCountText;
    public TMP_Text friendCountText;
    public TMP_Text ticketCountText;
    public TMP_Text visitFestivalText;
    public Image profileImage;

    [Header("프로필 이미지 매핑")]
    public List<ProfileSprite> styleSpriteList;
    private Dictionary<string, Sprite> styleSpriteMap;

    [Serializable]
    public class ProfileSprite
    {
        public string styleName;
        public Sprite sprite;
    }

    private string userInfoUrl;
    private string userFriendsCountUrl;

    void Awake()
    {
        userInfoUrl = $"{ServerConfig.baseUrl}/users/get_my_Uprofile/";
        userFriendsCountUrl = $"{ServerConfig.baseUrl}/social/friends/count/";

        styleSpriteMap = new Dictionary<string, Sprite>();
        foreach (var entry in styleSpriteList)
        {
            if (!styleSpriteMap.ContainsKey(entry.styleName))
                styleSpriteMap[entry.styleName] = entry.sprite;
        }
    }

    // 메인 버튼 OnClick에 연결할 함수
    public void OpenProfile()
    {
        StartCoroutine(GetUserInfo());
    }

    IEnumerator GetUserInfo()
    {
        string token = PlayerPrefs.GetString("access_token", "");
        Debug.Log("불러온 access_token: " + token);

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("❌ access_token이 비어 있음");
            yield break;
        }

        UnityWebRequest www = UnityWebRequest.Get(userInfoUrl);
        www.SetRequestHeader("Authorization", "Bearer " + token);

        yield return www.SendWebRequest();

        Debug.Log("📡 UserInfo Raw Response: " + www.downloadHandler.text);

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ User Info API 실패: " + www.error);
        }
        else
        {
            try
            {
                UserInfoResponse data = JsonUtility.FromJson<UserInfoResponse>(www.downloadHandler.text);

                nameText.text = data.character_name + " 님";
                introText.text = data.character_intro;

                // 최근 접속 날짜 포맷
                string formattedDate = FormatDate(data.last_login);
                lastLoginText.text = formattedDate;

                // 스타일 이름 매핑
                if (data.character_style != null && !string.IsNullOrEmpty(data.character_style.characterStyle))
                {
                    string styleName = data.character_style.characterStyle;
                    ApplyProfileSprite(styleName);
                }
                else
                {
                    Debug.LogWarning("⚠️ character_style이 비어 있음. 기본 이미지 사용.");
                }

                // 티켓/인벤토리/친구 수 불러오기
                StartCoroutine(GetTicketCount(data.character_id));
                StartCoroutine(GetInventoryCount(data.character_id));
                StartCoroutine(GetFriendCount());
            }
            catch (Exception e)
            {
                Debug.LogError("❌ JSON 파싱 실패: " + e.Message);
                lastLoginText.text = "최근 접속: 파싱 실패";
            }
        }
    }

    private void ApplyProfileSprite(string styleName)
    {
        if (styleSpriteMap.TryGetValue(styleName, out Sprite sprite))
        {
            profileImage.sprite = sprite;
        }
        else
        {
            Debug.LogWarning($"⚠️ '{styleName}'에 해당하는 스프라이트를 찾을 수 없습니다.");
        }
    }

    IEnumerator GetTicketCount(string characterId)
    {
        string token = PlayerPrefs.GetString("access_token", "");
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("❌ access_token이 비어 있음");
            yield break;
        }

        string url = $"{ServerConfig.baseUrl}/tickets/count/{characterId}/";
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + token);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Ticket Count API 실패: " + www.error);
        }
        else
        {
            try
            {
                TicketCountResponse data = JsonUtility.FromJson<TicketCountResponse>(www.downloadHandler.text);
                ticketCountText.text = $"{data.ticket_count}";
                visitFestivalText.text = $"{data.ticket_count}";
            }
            catch (Exception e)
            {
                Debug.LogError("❌ 티켓 수 JSON 파싱 실패: " + e.Message);
                ticketCountText.text = "-";
            }
        }
    }

    [Serializable]
    public class TicketCountResponse
    {
        public int ticket_count;
    }

    IEnumerator GetInventoryCount(string characterId)
    {
        string token = PlayerPrefs.GetString("access_token", "");
        string url = $"{ServerConfig.baseUrl}/item/inventory/count/{characterId}/";

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("❌ access_token이 비어 있음");
            yield break;
        }

        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + token);

        yield return www.SendWebRequest();

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
                itemCountText.text = "파싱 실패";
            }
        }
    }

    IEnumerator GetFriendCount()
    {
        string token = PlayerPrefs.GetString("access_token", "");
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("❌ access_token이 비어 있음");
            yield break;
        }

        UnityWebRequest www = UnityWebRequest.Get(userFriendsCountUrl);
        www.SetRequestHeader("Authorization", "Bearer " + token);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Friend Count API 실패: " + www.error);
        }
        else
        {
            try
            {
                FriendCountResponse data = JsonUtility.FromJson<FriendCountResponse>(www.downloadHandler.text);
                friendCountText.text = $"{data.friend_count}";
            }
            catch (Exception e)
            {
                Debug.LogError("❌ 친구 수 JSON 파싱 실패: " + e.Message);
                friendCountText.text = "-";
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

    [Serializable]
    public class UserInfoResponse
    {
        public string character_id;
        public string character_intro;
        public string character_name;
        public string last_login;
        public CharacterStyleData character_style;
    }

    [Serializable]
    public class CharacterStyleData
    {
        public string characterName;
        public string characterStyle;
    }

    [Serializable]
    public class InventoryCountResponse
    {
        public string character_id;
        public int inventory_count;
    }

    [Serializable]
    public class FriendCountResponse
    {
        public int friend_count;
    }
}
