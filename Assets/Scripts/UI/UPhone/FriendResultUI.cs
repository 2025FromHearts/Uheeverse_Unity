using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Mathematics;

public class FriendResultUI : MonoBehaviour
{
    public TMP_Text nameText;      
    public Button addFriendButton;   

    private string targetCharacterId;

    public void SetData(U_SearchFriend.CharacterResult data, U_SearchFriend manager)
    {
        targetCharacterId = data.character_id;

        if (nameText) nameText.text = data.character_name + " ดิ";

        if (addFriendButton)
        {
            addFriendButton.onClick.RemoveAllListeners();
            addFriendButton.onClick.AddListener(() => {
                manager.StartCoroutine(manager.AddFriendRequest(targetCharacterId));
            });
        }
    }
}
