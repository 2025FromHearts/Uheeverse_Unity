using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;
using System;

public class GalleryListUI : MonoBehaviour
{
    [Header("UI")]
    public RawImage imageDisplay;
    public TextMeshProUGUI dateText;

    [Header("Viewer 연결")]
    public U_GalleryViewer galleryViewer;

    private Texture2D loadedTexture;
    private string photoUrl;
    private string galleryId; 

    private void Awake()
    {
        // 자동으로 Viewer 찾아 연결
        if (galleryViewer == null)
            galleryViewer = FindObjectOfType<U_GalleryViewer>(true);
    }

    /// 갤러리 리스트 항목에 데이터 설정
    public void SetData(string imageUrl, string uploadedAt, string id)
    {
        photoUrl = imageUrl;
        galleryId = id;
        dateText.text = FormatDate(uploadedAt);

        // 클릭 이벤트 등록
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnPhotoClick);
        }

        StartCoroutine(LoadImage(photoUrl));
    }

    /// ISO 날짜 문자열을 yyyy.MM.dd 형식으로 변환
    private string FormatDate(string isoDate)
    {
        if (DateTime.TryParse(isoDate, out DateTime parsed))
            return parsed.ToString("yyyy.MM.dd");
        return "-";
    }

    /// 이미지 로드
    private IEnumerator LoadImage(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("⚠️ 이미지 URL이 비어 있습니다.");
            yield break;
        }

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                loadedTexture = DownloadHandlerTexture.GetContent(www);
                imageDisplay.texture = loadedTexture;
                GetComponent<Button>().interactable = true;
            }
            else
            {
                Debug.LogError("❌ 이미지 로드 실패: " + www.error);
            }
        }
    }

    /// 썸네일 클릭 시 전체 보기 호출
    private void OnPhotoClick()
    {
        if (galleryViewer == null)
        {
            Debug.LogWarning("⚠ galleryViewer 연결 안 됨");
            return;
        }

        if (loadedTexture == null)
        {
            Debug.LogWarning("⚠ 이미지 아직 로드 중");
            return;
        }

        // galleryId 포함해서 전달
        galleryViewer.ShowPhoto(
            loadedTexture,
            dateText != null ? dateText.text : "",
            photoUrl,
            galleryId
        );
    }
}
