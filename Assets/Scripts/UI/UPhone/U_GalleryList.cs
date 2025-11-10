using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;

[System.Serializable]
public class GalleryData
{
    public string gallery_id;
    public string url;
    public string filename;
    public string uploaded_at;
}

public class U_GalleryList : MonoBehaviour
{
    [Header("UI")]
    public Transform contentParent;     // ScrollView → Content
    public GameObject galleryItemPrefab; // Prefab (GalleryItemUI 포함)

    private string baseUrl = ServerConfig.baseUrl;
    public void OpenGallery()
    {
        gameObject.SetActive(true);
        StartCoroutine(LoadGallery());
    }

    private IEnumerator LoadGallery()
    {
        string token = PlayerPrefs.GetString("access_token", "");
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("❌ Access token 없음!");
            yield break;
        }

        string url = $"{baseUrl}/gallery/gallery_list/";
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + token);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ 갤러리 요청 실패: " + www.error + "\n" + www.downloadHandler.text);
            yield break;
        }

        List<GalleryData> results = null;
        try
        {
            results = JsonConvert.DeserializeObject<List<GalleryData>>(www.downloadHandler.text);
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ JSON 파싱 실패: " + e.Message);
            yield break;
        }

        // 기존 목록 삭제
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 프리팹 생성
        foreach (var g in results)
        {
            GameObject item = Instantiate(galleryItemPrefab, contentParent);
                item.SetActive(true);
            GalleryListUI ui = item.GetComponent<GalleryListUI>();
            if (ui != null)
                ui.SetData(g.url, g.uploaded_at);
        }

        Debug.Log($"✅ 갤러리 불러오기 완료 ({results.Count}개)");
    }
}
