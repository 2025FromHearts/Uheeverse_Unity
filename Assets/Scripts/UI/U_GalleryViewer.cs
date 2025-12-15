using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class U_GalleryViewer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RawImage fullImage;
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private Button deleteButton;

    private string currentPhotoUrl;
    private string currentPhotoId; // 서버 DB의 gallery_id
    private string baseUrl;
    private string accessToken; 

    private void Awake()
    {
        if (fullImage == null)
            fullImage = GetComponentInChildren<RawImage>(true);

        if (fullImage != null)
        {
            fullImage.texture = null;
            fullImage.color = new Color(1f, 1f, 1f, 0f);
        }

        // 시작 시 비활성화
        gameObject.SetActive(false);

        // 삭제 버튼 이벤트 등록
        if (deleteButton != null)
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(OnDeletePhoto);
    }

    /// 갤러리 사진을 열어 보여줄 때 호출

    public void ShowPhoto(Texture2D tex, string date, string url, string galleryId)
    {
        gameObject.SetActive(true);
        fullImage.texture = tex;
        fullImage.color = Color.white;

        if (dateText != null)
            dateText.text = FormatDate(date);

        currentPhotoUrl = url;
        currentPhotoId = galleryId;
    }
    private string FormatDate(string isoDate)
    {
        if (System.DateTime.TryParse(isoDate, out System.DateTime parsed))
            return parsed.ToString("yyyy.MM.dd");
        return "-";
    }

    /// 닫기 버튼

    public void CloseViewer()
    {
        if (fullImage != null)
        {
            fullImage.texture = null;
            fullImage.color = new Color(1f, 1f, 1f, 0f);
        }
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 삭제 버튼 클릭 시
    /// </summary>
    public void OnDeletePhoto()
    {
        if (string.IsNullOrEmpty(currentPhotoId))
        {
            Debug.LogWarning("❗ currentPhotoId가 비어 있어 삭제할 수 없습니다.");
            return;
        }

        StartCoroutine(DeletePhotoRequest());
    }

    /// <summary>
    /// Django 서버로 DELETE 요청
    /// </summary>
    private IEnumerator DeletePhotoRequest()
    {
        baseUrl = ServerConfig.baseUrl;
        accessToken = PlayerPrefs.GetString("access_token", "");

        string url = $"{baseUrl}/gallery/gallery_list/{currentPhotoId}/"; // ✅ 서버와 일치
        UnityWebRequest request = UnityWebRequest.Delete(url);
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);

        Debug.Log($"🗑️ DELETE 요청 전송: {url}");
        yield return request.SendWebRequest();

        long code = request.responseCode;
        string body = request.downloadHandler != null ? request.downloadHandler.text : "";

        if (request.result == UnityWebRequest.Result.Success && (code == 200 || code == 204))
        {
            Debug.Log("✅ 사진 삭제 성공");
            CloseViewer();

            var galleryList = FindAnyObjectByType<U_GalleryList>(FindObjectsInactive.Include);
            if (galleryList != null)
                galleryList.RefreshGallery();
            else
                Debug.LogWarning("⚠️ U_GalleryList를 찾지 못했습니다.");
        }
        else
        {
            Debug.LogError($"❌ 삭제 실패: code={code}, error={request.error}\n응답: {body}");
        }
    }

}
