using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PhotoModeController : MonoBehaviour
{
    [Header("카메라")]
    public Camera playerCamera;
    public Camera photoCamera;              // 비활성화해 두기 권장
    public Transform photoPivot;            // 비워두면 photoCamera.transform 사용
    public Transform photoSpawnPoint;

    [Header("HUD / UI")]
    public GameObject photoHud;             // 촬영/나가기 UI
    public GameObject mainUICanvas;
    public Button shutterButton;            // 촬영
    public Button exitButton;               // 종료
    public CanvasGroup flash;               // 선택: 촬영 플래시

    [Header("플레이어 제어 비활성화 목록")]
    public MonoBehaviour[] componentsToDisableWhilePhoto; // PlayerMovement 등

    [Header("키 설정")]
    public KeyCode shutterKey = KeyCode.Space;
    public KeyCode exitKey = KeyCode.Escape;
    public KeyCode zoomInKey = KeyCode.Equals;   // 키보드 +
    public KeyCode zoomOutKey = KeyCode.Minus;   // 키보드 -

    [Header("이동/줌 설정")]
    public float moveSpeed = 3f;            // WASD 평면 이동 속도
    public float zoomSpeed = 50f;           // FOV 변화 속도(휠/키)
    public float fovDefault = 60f;
    public float fovMin = 20f;
    public float fovMax = 90f;

    private bool isOn;
    private float lockY;                    // 수평 이동용 고정 높이

    void Awake()
    {
        if (photoCamera != null) photoCamera.enabled = false;
        if (photoHud != null) photoHud.SetActive(false);

        if (shutterButton != null)
        {
            shutterButton.onClick.RemoveAllListeners();
            shutterButton.onClick.AddListener(TakeShot);
        }
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(ExitPhotoMode);
        }
    }

    public void EnterPhotoMode()
    {
        isOn = true;

        // 피벗 기본값: 포토 카메라 트랜스폼
        if (photoPivot == null && photoCamera != null)
            photoPivot = photoCamera.transform;

        if (photoCamera != null)
        {
            photoCamera.enabled = true;
            photoCamera.fieldOfView = fovDefault;
        }
        if (playerCamera != null) playerCamera.enabled = false;

        if (photoHud != null) photoHud.SetActive(true);

        foreach (var c in componentsToDisableWhilePhoto)
            if (c != null) c.enabled = false;

        // 수평 이동을 위해 현재 Y 고정
        if (photoPivot != null) lockY = photoPivot.position.y;

        if (mainUICanvas != null)
            mainUICanvas.SetActive(false);
    }

    public void ExitPhotoMode()
    {
        isOn = false;

        if (photoHud != null) photoHud.SetActive(false);

        if (photoCamera != null)
        {
            photoCamera.fieldOfView = fovDefault;
            photoCamera.enabled = false;
        }
        if (playerCamera != null) playerCamera.enabled = true;

        foreach (var c in componentsToDisableWhilePhoto)
            if (c != null) c.enabled = true;

        if (mainUICanvas != null)
            mainUICanvas.SetActive(true);
    }

    void Update()
    {
        if (!isOn) return;

        // --- 평면 이동 (앞/뒤/좌/우) ---
        if (photoPivot != null && photoCamera != null)
        {
            // 카메라 기준의 평면 방향 벡터
            Vector3 fwd = photoCamera.transform.forward; fwd.y = 0f; fwd.Normalize();
            Vector3 right = photoCamera.transform.right; right.y = 0f; right.Normalize();

            float h = Input.GetAxisRaw("Horizontal"); // A/D
            float v = Input.GetAxisRaw("Vertical");   // W/S

            Vector3 delta = (right * h + fwd * v).normalized * moveSpeed * Time.unscaledDeltaTime;
            Vector3 pos = photoPivot.position + delta;
            pos.y = lockY; // 수직 고정
            photoPivot.position = pos;
        }

        // --- 줌 (FOV) : 마우스 휠 + 단축키(+/-) ---
        if (photoCamera != null)
        {
            float wheel = Input.mouseScrollDelta.y; // 위로 굴리면 +, 아래 - (플랫폼별 반대로 보이면 부호 반전)
            float key = 0f;
            if (Input.GetKey(zoomInKey)) key += 1f;
            if (Input.GetKey(zoomOutKey)) key -= 1f;

            float delta = (wheel + key) * zoomSpeed * Time.unscaledDeltaTime;
            if (Mathf.Abs(delta) > 0.0001f)
            {
                float fov = Mathf.Clamp(photoCamera.fieldOfView - delta, fovMin, fovMax);
                photoCamera.fieldOfView = fov;
            }
        }

        // --- 단축키 ---
        if (Input.GetKeyDown(shutterKey)) TakeShot();
        if (Input.GetKeyDown(exitKey)) ExitPhotoMode();
    }

    private void TakeShot()
    {
        StartCoroutine(CaptureWithoutUI());
    }

    private System.Collections.IEnumerator CaptureWithoutUI()
    {
        // 1️⃣ 모든 Canvas 비활성화
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas c in canvases)
            c.enabled = false;

        yield return new WaitForEndOfFrame(); // UI 꺼진 프레임 반영

        // 2️⃣ 캡처
        string file = $"photo_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        string path = Path.Combine(Application.persistentDataPath, file);

        ScreenCapture.CaptureScreenshot(path);
        Debug.Log($"📸 Saved (UI 제외): {path}");

        if (flash != null)
            StartCoroutine(FlashRoutine());

        // 3️⃣ 잠시 대기 후 UI 다시 켜기
        yield return new WaitForSecondsRealtime(0.3f);
        foreach (Canvas c in canvases)
            c.enabled = true;
    }


    private System.Collections.IEnumerator FlashRoutine()
    {
        flash.gameObject.SetActive(true);
        flash.alpha = 1f;
        float t = 0f;
        while (t < 0.15f)
        {
            t += Time.unscaledDeltaTime;
            flash.alpha = Mathf.Lerp(1f, 0f, t / 0.15f);
            yield return null;
        }
        flash.gameObject.SetActive(false);
    }
}
