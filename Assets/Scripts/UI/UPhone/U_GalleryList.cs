using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class GalleryItemData
{
    public string gallery_id;
    public string url;
    public string filename;
    public string uploaded_at;
}

public class U_GalleryList : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform galleryContainer;   // 갤러리 아이템들이 배치될 부모
    [SerializeField] private GameObject galleryItemPrefab; // 하나의 썸네일 프리팹

    private string baseUrl;
    private string accessToken;

    private void Start()
    {
        RefreshGallery();
    }

    /// ✅ 갤러리 목록 새로고침 (Viewer에서 호출)
    public void RefreshGallery()
    {
        StopAllCoroutines();
        StartCoroutine(LoadGallery());
    }

    private IEnumerator LoadGallery()
    {
        baseUrl = ServerConfig.baseUrl;
        accessToken = PlayerPrefs.GetString("access_token", "");

        string url = $"{baseUrl}/gallery/gallery_list/";
        UnityWebRequest req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"❌ 갤러리 로드 실패: {req.error}\n{req.downloadHandler.text}");
            yield break;
        }

        // 기존 아이템 모두 제거
        foreach (Transform child in galleryContainer)
            Destroy(child.gameObject);

        // JSON 파싱
        string json = req.downloadHandler.text;
        GalleryItemData[] items = JsonHelper.FromJson<GalleryItemData>(json);

        foreach (var item in items)
        {
            GameObject thumb = Instantiate(galleryItemPrefab, galleryContainer);

            // 버튼 참조
            Button btn = thumb.GetComponent<Button>();
            if (btn == null)
            {
                Debug.LogWarning("⚠️ galleryItemPrefab에 Button 컴포넌트가 없습니다.");
                continue;
            }

            // 클릭 잠금 (이미지 로드 중에는 비활성화)
            btn.interactable = false;
            btn.onClick.RemoveAllListeners();

            // 각각의 텍스트 찾기
            var texts = thumb.GetComponentsInChildren<TextMeshProUGUI>();
            TextMeshProUGUI festivalNameText = null;
            TextMeshProUGUI dateText = null;

            foreach (var t in texts)
            {
                if (t.name == "T_festival_name") festivalNameText = t;
                else if (t.name == "T_date") dateText = t;
            }

            if (festivalNameText != null)
                festivalNameText.text = "청송사과축제"; // 축제명 표시

            if (dateText != null)
                dateText.text = FormatDate(item.uploaded_at); // 날짜 표시

            // 이미지 로드
            var image = thumb.GetComponentInChildren<RawImage>();
            UnityWebRequest texReq = UnityWebRequestTexture.GetTexture(item.url);
            yield return texReq.SendWebRequest();

            if (texReq.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = ((DownloadHandlerTexture)texReq.downloadHandler).texture;
                image.texture = tex;

                // ✅ 로드 완료 시 버튼 활성화
                btn.interactable = true;

                // ✅ 중복 리스너 방지 후 새로 등록
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    var viewer = FindAnyObjectByType<U_GalleryViewer>(FindObjectsInactive.Include);
                    if (viewer == null)
                    {
                        Debug.LogError("🚫 U_GalleryViewer를 씬에서 찾지 못했습니다. (Scene에 Viewer 오브젝트가 비활성화되어 있는지 확인하세요)");
                        return;
                    }

                    Debug.Log($"✅ 썸네일 클릭됨 → {item.filename}");
                    viewer.ShowPhoto(tex, FormatDate(item.uploaded_at), item.url, item.gallery_id);
                });
            }
            else
            {
                Debug.LogWarning($"⚠️ 이미지 로드 실패: {item.url}");
            }
        }
    }

    private string FormatDate(string isoDate)
    {
        if (System.DateTime.TryParse(isoDate, out System.DateTime parsed))
            return parsed.ToString("yyyy.MM.dd");
        return "-";
    }
}
