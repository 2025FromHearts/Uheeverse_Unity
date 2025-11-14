using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static PhotoPoseData;

public class PhotoModeController : MonoBehaviour
{
    public CharacterPoseSet[] poseData;

    [Header("카메라")]
    public Camera playerCamera;
    public Camera photoCamera;

    [Header("HUD / UI")]
    public GameObject photoHud;
    public GameObject mainUICanvas;
    public Button shutterButton;
    public Button exitButton;
    public Button[] poseButtons;
    public Transform photoSpawnPoint;

    [Header("플레이어 제어 비활성화 목록")]
    public MonoBehaviour[] componentsToDisableWhilePhoto;

    [Header("포토 모드에서 숨길 NPC들")]
    public GameObject[] npcsToHide;

    [Header("키 설정")]
    public KeyCode shutterKey = KeyCode.Plus;
    public KeyCode exitKey = KeyCode.Minus;
    public KeyCode zoomInKey = KeyCode.A;
    public KeyCode zoomOutKey = KeyCode.B;

    [Header("줌 설정")]
    public float zoomSpeed = 50f;
    public float fovDefault = 60f;
    public float fovMin = 20f;
    public float fovMax = 90f;

    private GameObject currentPoseObj;
    private CharacterPoseSet currentPoseSet;
    private GameObject playerCharacter;

    private bool isOn;

    private string uploadUrl = $"{ServerConfig.baseUrl}/gallery/upload_image/";

    void Awake()
    {
        if (photoCamera != null) photoCamera.enabled = false;
        photoHud?.SetActive(false);

        shutterButton.onClick.AddListener(TakeShot);
        exitButton.onClick.AddListener(ExitPhotoMode);

        for (int i = 0; i < poseButtons.Length; i++)
        {
            int index = i;
            poseButtons[i].onClick.AddListener(() => SetPose(index + 1));
        }
    }

    private void SetPose(int poseIndex)
    {
        if (currentPoseSet == null) return;

        if (currentPoseObj != null)
            Destroy(currentPoseObj);

        GameObject prefab = poseIndex switch
        {
            1 => currentPoseSet.pose1Prefab,
            2 => currentPoseSet.pose2Prefab,
            3 => currentPoseSet.pose3Prefab,
            _ => null
        };

        Vector3 posOffset = poseIndex switch
        {
            1 => currentPoseSet.pose1PositionOffset,
            2 => currentPoseSet.pose2PositionOffset,
            3 => currentPoseSet.pose3PositionOffset,
            _ => Vector3.zero
        };

        Vector3 rotOffset = poseIndex switch
        {
            1 => currentPoseSet.pose1RotationOffset,
            2 => currentPoseSet.pose2RotationOffset,
            3 => currentPoseSet.pose3RotationOffset,
            _ => Vector3.zero
        };

        Vector3 finalPos = photoSpawnPoint.position + posOffset;
        Quaternion finalRot = photoSpawnPoint.rotation * Quaternion.Euler(rotOffset);

        currentPoseObj = Instantiate(prefab, finalPos, finalRot);
    }

    public void EnterPhotoMode(string characterStyle, GameObject playerObj)
    {
        isOn = true;
        playerCharacter = playerObj;
        playerCharacter.SetActive(false);

        // pose set 찾기
        currentPoseSet = Array.Find(poseData, set => set.characterName == characterStyle);

        if (currentPoseSet == null)
        {
            Debug.LogWarning($"PoseSet 미발견: {characterStyle}");
            return;
        }

        SetPose(1);

        photoCamera.enabled = true;
        playerCamera.enabled = false;

        photoHud?.SetActive(true);
        mainUICanvas?.SetActive(false);

        foreach (var c in componentsToDisableWhilePhoto)
            c.enabled = false;

        foreach (var npc in npcsToHide)
        {
            if (npc != null)
                npc.SetActive(false);
        }
    }

    public void ExitPhotoMode()
    {
        isOn = false;

        if (currentPoseObj != null)
            Destroy(currentPoseObj);

        playerCharacter?.SetActive(true);

        photoHud?.SetActive(false);
        mainUICanvas?.SetActive(true);

        photoCamera.enabled = false;
        playerCamera.enabled = true;

        foreach (var c in componentsToDisableWhilePhoto)
            c.enabled = true;

        foreach (var npc in npcsToHide)
        {
            if (npc != null)
                npc.SetActive(true);
        }
    }

    void Update()
    {
        if (!isOn) return;

        float wheel = Input.mouseScrollDelta.y;
        float key = (Input.GetKey(zoomInKey) ? 1 : 0) - (Input.GetKey(zoomOutKey) ? 1 : 0);

        float delta = (wheel + key) * zoomSpeed * Time.deltaTime;
        if (Mathf.Abs(delta) > 0.01f)
            photoCamera.fieldOfView = Mathf.Clamp(photoCamera.fieldOfView - delta, fovMin, fovMax);

        if (Input.GetKeyDown(shutterKey)) TakeShot();
        if (Input.GetKeyDown(exitKey)) ExitPhotoMode();
    }

    public void TakeShot()
    {
        StartCoroutine(CaptureAndUpload());
    }

    private IEnumerator CaptureAndUpload()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas c in canvases) c.enabled = false;

        yield return new WaitForEndOfFrame();

        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();

        byte[] pngBytes = screenshot.EncodeToPNG();
        Destroy(screenshot);

        foreach (Canvas c in canvases) c.enabled = true;

        yield return UploadImageBytes(pngBytes);
    }

    // 업로드 함수
    private IEnumerator UploadImageBytes(byte[] imageBytes)
    {
        string token = PlayerPrefs.GetString("access_token", "");

        WWWForm form = new WWWForm();
        form.AddBinaryData("image", imageBytes, "photo.png", "image/png");

        UnityWebRequest www = UnityWebRequest.Post(uploadUrl, form);
        www.SetRequestHeader("Authorization", $"Bearer {token}");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            Debug.LogError($"업로드 실패: {www.error}\n응답: {www.downloadHandler.text}");

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"✅ 업로드 성공! 응답: {www.downloadHandler.text}");

            // 업로드 성공 → 갤러리 업데이트 콜백 실행
            var galleryList = FindAnyObjectByType<U_GalleryList>(FindObjectsInactive.Include);
            if (galleryList != null)
                galleryList.RefreshGallery();
        }
    }
}
