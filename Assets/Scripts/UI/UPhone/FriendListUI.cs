using UnityEngine;
using TMPro;

public class FriendListUI : MonoBehaviour
{
    [Header("UI Reference")]
    public TMP_Text nameText;
    public TMP_Text lastLoginText;
    public TMP_Text lastFestivalText;

    public void SetData(FriendData data)
    {
        if (nameText != null)
            nameText.text = data.character_name + " 님";

        if (lastLoginText != null)
        {
            // 날짜 포맷 변환
            if (!string.IsNullOrEmpty(data.last_login))
            {
                System.DateTime parsed;
                if (System.DateTime.TryParse(data.last_login, out parsed))
                {
                    lastLoginText.text = "최근 로그인: " + parsed.ToString("yyyy년 MM월 dd일");
                }
                else
                {
                    lastLoginText.text = "최근 로그인: -";
                }
            }
            else
            {
                lastLoginText.text = "최근 로그인: -";
            }
        }

        if (lastFestivalText != null)
            lastFestivalText.text = "최근 축제: " + (string.IsNullOrEmpty(data.last_festival) ? "-" : data.last_festival);
    }
}
