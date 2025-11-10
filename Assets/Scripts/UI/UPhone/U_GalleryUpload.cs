using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

public class U_GalleryUpload : MonoBehaviour
{
    public Button captureButton;

    private string baseUrl = ServerConfig.baseUrl; 
    private string apiEndpoint = "/gallery/upload_image/";

    void Start()
    {
        if (captureButton != null)
            captureButton.onClick.AddListener(() => StartCoroutine(CaptureAndUpload()));
    }

    private IEnumerator CaptureAndUpload()
    {
        yield return new WaitForEndOfFrame();

        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();

        byte[] imageBytes = screenshot.EncodeToPNG();
        Destroy(screenshot);

        yield return StartCoroutine(UploadImageBytes(imageBytes));
    }

    private IEnumerator UploadImageBytes(byte[] imageBytes)
    {
        string token = PlayerPrefs.GetString("access_token", "");
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("❌ Access token 없음!");
            yield break;
        }

        string url = $"{baseUrl}{apiEndpoint}";
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", imageBytes, "screenshot.png", "image/png");

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        www.SetRequestHeader("Authorization", "Bearer " + token);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"❌ 업로드 실패: {www.error}\n응답: {www.downloadHandler.text}");
        }
        else
        {
            Debug.Log($"✅ 업로드 성공! 응답: {www.downloadHandler.text}");
        }
    }
}
