using System;
using System.Collections;
using System.Collections.Generic;
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

    [Header("NPC 포토존 스폰 포인트")]
    public Transform npcSpawnPoints;
    public float detectRadius = 2f;

    [Header("줌 설정")]
    public Button zoomInButton;
    public Button zoomOutButton;
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
        if (photoCamera != null)
            photoCamera.enabled = false;

        photoHud?.SetActive(false);

        shutterButton.onClick.AddListener(TakeShot);
        exitButton.onClick.AddListener(ExitPhotoMode);

        // 포즈 버튼 연결
        for (int i = 0; i < poseButtons.Length; i++)
        {
            int index = i;
            poseButtons[i].onClick.AddListener(() => SetPose(index + 1));
        }

        // 줌인 / 줌아웃 버튼 연결
        zoomInButton.onClick.AddListener(ZoomIn);
        zoomOutButton.onClick.AddListener(ZoomOut);
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

        HideNPCsNearSpawnPoints();
    }


    private List<GameObject> autoHiddenNPCs = new List<GameObject>();

    private void HideNPCsNearSpawnPoints()
    {
        autoHiddenNPCs.Clear();

        if (npcSpawnPoints == null)
        {
            Debug.LogWarning("NPC 스폰 포인트를 설정해주세요.");
            return;
        }

        Collider[] hits = Physics.OverlapSphere(npcSpawnPoints.position, detectRadius);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("NPC"))
            {
                hit.gameObject.SetActive(false);
                autoHiddenNPCs.Add(hit.gameObject);
            }
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

        foreach (var npc in autoHiddenNPCs)
        {
            if (npc != null)
                npc.SetActive(true);
        }
    }


    void Update()
    {
        if (!isOn) return;
    }


    public void ZoomIn()
    {
        photoCamera.fieldOfView = Mathf.Clamp(
            photoCamera.fieldOfView - zoomSpeed * Time.deltaTime,
            fovMin, fovMax
        );
    }

    public void ZoomOut()
    {
        photoCamera.fieldOfView = Mathf.Clamp(
            photoCamera.fieldOfView + zoomSpeed * Time.deltaTime,
            fovMin, fovMax
        );
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


    private IEnumerator UploadImageBytes(byte[] imageBytes)
    {
        string token = PlayerPrefs.GetString("access_token", "");

        WWWForm form = new WWWForm();
        form.AddBinaryData("image", imageBytes, "photo.png", "image/png");

        UnityWebRequest www = UnityWebRequest.Post(uploadUrl, form);
        www.SetRequestHeader("Authorization", $"Bearer {token}");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"업로드 실패: {www.error}\n응답: {www.downloadHandler.text}");
        }
        else
        {
            Debug.Log($"✅ 업로드 성공! 응답: {www.downloadHandler.text}");

            var galleryList = FindAnyObjectByType<U_GalleryList>(FindObjectsInactive.Include);
            if (galleryList != null)
                galleryList.RefreshGallery();
        }
    }
}
