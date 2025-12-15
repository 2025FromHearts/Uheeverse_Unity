using UnityEngine;
using TMPro;
using static ProfileUI;
using System.Collections.Generic;
using UnityEngine.UI;

public class FriendListUI : MonoBehaviour
{
    [Header("UI Reference")]
    public TMP_Text nameText;
    public TMP_Text lastLoginText;
    public TMP_Text lastFestivalText;
    public Image profileImage;

    [Header("프로필 이미지 매핑")]
    public List<ProfileSprite> styleSpriteList;
    private Dictionary<string, Sprite> styleSpriteMap;

    [System.Serializable]
    public class ProfileSprite
    {
        public string styleName;
        public Sprite sprite;
    }

    private void Awake()
    {
        // 스타일 매핑 초기화
        styleSpriteMap = new Dictionary<string, Sprite>();
        foreach (var entry in styleSpriteList)
        {
            if (!styleSpriteMap.ContainsKey(entry.styleName))
                styleSpriteMap[entry.styleName] = entry.sprite;
        }
    }

    /// 서버에서 받은 친구 정보 설정
    public void SetData(FriendData data)
    {
        // 이름
        if (nameText != null)
            nameText.text = data.character_name + " 님";

        // 최근 로그인
        if (lastLoginText != null)
        {
            if (!string.IsNullOrEmpty(data.last_login))
            {
                System.DateTime parsed;
                if (System.DateTime.TryParse(data.last_login, out parsed))
                    lastLoginText.text = "최근 로그인: " + parsed.ToString("yyyy년 MM월 dd일");
                else
                    lastLoginText.text = "최근 로그인: -";
            }
            else
                lastLoginText.text = "최근 로그인: -";
        }

        // 최근 축제
        if (lastFestivalText != null)
            lastFestivalText.text = "최근 축제: " + (string.IsNullOrEmpty(data.last_festival) ? "-" : data.last_festival);

        // 프로필 이미지 매핑
        if (!string.IsNullOrEmpty(data.character_style))
        {
            ApplyProfileSprite(data.character_style);
        }
        else
        {
            Debug.LogWarning($"⚠️ {data.character_name}의 캐릭터 스타일이 비어 있습니다.");
        }
    }

    /// 스타일 이름을 기반으로 스프라이트 적용
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
}
