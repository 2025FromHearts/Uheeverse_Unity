using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class FriendResultUI : MonoBehaviour
{
    [Header("UI 요소")]
    public TMP_Text nameText;
    public Button addFriendButton;
    public Image profileImage;

    private string targetCharacterId;
    private U_SearchFriend manager;

    [Header("프로필 이미지 매핑")]
    public List<ProfileUI.ProfileSprite> styleSpriteList;  // Inspector에서 등록
    private Dictionary<string, Sprite> styleSpriteMap = new();


    /// 검색 결과 UI 데이터 설정

    public void SetData(U_SearchFriend.CharacterResult data, U_SearchFriend managerRef)
    {
        manager = managerRef;
        targetCharacterId = data.character_id;

        // 매핑 초기화
        if (styleSpriteMap == null || styleSpriteMap.Count == 0)
        {
            styleSpriteMap = new Dictionary<string, Sprite>();
            foreach (var entry in styleSpriteList)
            {
                if (!styleSpriteMap.ContainsKey(entry.styleName))
                    styleSpriteMap[entry.styleName] = entry.sprite;
            }
        }

        // 이름 표시
        if (nameText != null)
            nameText.text = data.character_name + " 님";

        // 프로필 이미지 매핑
        if (profileImage != null && !string.IsNullOrEmpty(data.character_style))
        {
            if (styleSpriteMap.TryGetValue(data.character_style, out Sprite sprite))
            {
                profileImage.sprite = sprite;
                Debug.Log($"✅ {data.character_name}: 스타일={data.character_style} → 스프라이트={sprite.name}");
            }
            else
            {
                Debug.LogWarning($"⚠️ '{data.character_style}'에 해당하는 스프라이트를 찾을 수 없습니다. (매핑된 키 수: {styleSpriteMap.Count})");
            }
        }

        // 버튼 이벤트 설정
        if (addFriendButton != null)
        {
            addFriendButton.onClick.RemoveAllListeners();
            addFriendButton.onClick.AddListener(() =>
            {
                manager.StartCoroutine(manager.AddFriendRequest(targetCharacterId));
            });
        }
    }
}
