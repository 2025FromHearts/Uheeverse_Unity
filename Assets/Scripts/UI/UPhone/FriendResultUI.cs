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
    public List<ProfileUI.ProfileSprite> styleSpriteList;
    private Dictionary<string, Sprite> styleSpriteMap = new();

    [Header("Highlight")]
    public Image background;   //이미 있는 Image 연결

    public Color normalColor = Color.white;
    public Color highlightColor = new Color(0.85f, 0.9f, 1f, 1f); // 연한 파랑

    public void SetHighlight(bool on)
    {
        if (background == null) return;
        background.color = on ? highlightColor : normalColor;
    }

    public void SetData(U_SearchFriend.CharacterResult data, U_SearchFriend managerRef)
    {
        manager = managerRef;
        targetCharacterId = data.character_id;

        if (styleSpriteMap == null || styleSpriteMap.Count == 0)
        {
            styleSpriteMap = new Dictionary<string, Sprite>();
            foreach (var entry in styleSpriteList)
            {
                if (!styleSpriteMap.ContainsKey(entry.styleName))
                    styleSpriteMap[entry.styleName] = entry.sprite;
            }
        }

        if (nameText != null)
            nameText.text = data.character_name + " 님";

        if (profileImage != null && !string.IsNullOrEmpty(data.character_style))
        {
            if (styleSpriteMap.TryGetValue(data.character_style, out Sprite sprite))
                profileImage.sprite = sprite;
        }

        SetHighlight(false);

        if (addFriendButton != null)
        {
            addFriendButton.onClick.RemoveAllListeners();
            addFriendButton.onClick.AddListener(() =>
            {
                manager.StartCoroutine(manager.AddFriendRequest(targetCharacterId));
            });
        }
    }


    public void ConfirmAddFriend()
    {
        if (manager == null) return;

        manager.StartCoroutine(manager.AddFriendRequest(targetCharacterId));
    }

    public string GetTargetId()
    {
        return targetCharacterId;
    }
}
