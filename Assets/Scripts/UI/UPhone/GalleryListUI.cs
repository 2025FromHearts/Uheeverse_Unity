using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;
using System;

public class GalleryListUI : MonoBehaviour
{
    public RawImage imageDisplay;
    public TextMeshProUGUI dateText;

    public void SetData(string imageUrl, string uploadedAt)
    {
        // ✅ 날짜 포맷 변환
        dateText.text = FormatDate(uploadedAt);

        // ✅ 이미지 로드
        StartCoroutine(LoadImage(imageUrl));
    }

    private string FormatDate(string isoDate)
    {
        if (DateTime.TryParse(isoDate, out DateTime parsed))
        {
            return parsed.ToString("yyyy.MM.dd");
        }
        else
        {
            Debug.LogWarning($"⚠️ 날짜 파싱 실패: {isoDate}");
            return "-";
        }
    }

    private IEnumerator LoadImage(string url)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Texture2D tex = DownloadHandlerTexture.GetContent(www);
            imageDisplay.texture = tex;
        }
        else
        {
            Debug.LogError("❌ 이미지 로드 실패: " + www.error);
        }
    }
}
