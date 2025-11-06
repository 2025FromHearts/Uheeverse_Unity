using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ProfileUI : MonoBehaviour
{
    [Header("UI 요소")]
    public TMP_Text nicknameText;
    public TMP_Text coinText;
    public TMP_Text introText;
    public Image profileImage;

    [Header("프로필 이미지 매핑")]
    public List<ProfileSprite> styleSpriteList;
    private Dictionary<string, Sprite> styleSpriteMap;

    [System.Serializable]
    public class ProfileSprite
    {
        public string styleName; // 예: "Girl", "Sports"
        public Sprite sprite;
    }

    private string baseUrl;
    private string accessToken;

    void Awake()
    {
        baseUrl = ServerConfig.baseUrl;
        accessToken = PlayerPrefs.GetString("access_token", "");

        // 스타일 매핑 초기화
        styleSpriteMap = new Dictionary<string, Sprite>();
        foreach (var entry in styleSpriteList)
        {
            if (!styleSpriteMap.ContainsKey(entry.styleName))
                styleSpriteMap[entry.styleName] = entry.sprite;
        }
    }

    // ✅ 패널이 켜질 때마다 자동으로 최신화
    void OnEnable()
    {
        StartCoroutine(LoadCharacterInfo());
    }

    IEnumerator LoadCharacterInfo()
    {
        string url = $"{baseUrl}/users/get_my_character_name/";
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ 캐릭터 정보 로드 실패: " + www.error);
            yield break;
        }

        Debug.Log($"서버 응답: {www.downloadHandler.text}");

        CharacterInfo info = JsonUtility.FromJson<CharacterInfo>(www.downloadHandler.text);

        // ✅ 데이터 반영
        nicknameText.text = info.character_name;
        introText.text = info.character_intro;
        coinText.text = info.character_coin.ToString();

        // ✅ 중첩 객체에서 스타일명 꺼내기
        if (info.character_style != null && !string.IsNullOrEmpty(info.character_style.characterStyle))
        {
            string styleName = info.character_style.characterStyle;
            Debug.Log($"🎨 서버 스타일: {styleName}");
            ApplyProfileSprite(styleName);

            PlayerPrefs.SetString("character_style", styleName);
            PlayerPrefs.Save();
        }
        else
        {
            string savedStyle = PlayerPrefs.GetString("character_style", "");
            if (!string.IsNullOrEmpty(savedStyle))
            {
                Debug.Log($"🎨 PlayerPrefs 스타일: {savedStyle}");
                ApplyProfileSprite(savedStyle);
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

    // ✅ 서버 응답 구조에 맞게 수정된 클래스
    [System.Serializable]
    public class CharacterInfo
    {
        public string character_name;
        public string character_intro;
        public int character_coin;
        public CharacterStyleData character_style;
    }

    [System.Serializable]
    public class CharacterStyleData
    {
        public string characterName;
        public string characterStyle;
    }

    // ✅ 수동 새로고침 버튼
    public void OnRefreshButtonClicked()
    {
        StopAllCoroutines();
        StartCoroutine(LoadCharacterInfo());
    }
}
