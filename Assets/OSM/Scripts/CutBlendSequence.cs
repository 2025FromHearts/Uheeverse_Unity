using System.Collections;
using UnityEngine;
using Cinemachine;

public class CutBlendSequence : MonoBehaviour
{
    [Header("UI (게임 UI 루트)")]
    public CanvasGroup uiGroup;

    [Header("인트로 동안 비활성할 컨트롤(선택)")]
    public Behaviour[] playerControls;          // PlayerInput, 이동 스크립트 등

    [Header("프리뷰 샷 순서 (VCams)")]
    public CinemachineVirtualCamera[] shots;    // VCam_01~N
    public float[] holdSeconds;                 // 각 샷 유지 시간(초)

    [Header("샷 모션(선택) — shots와 인덱스 맞추기")]
    public ShotMotion[] motions;                // 각 VCam에 붙인 ShotMotion

    [Header("플레이 시작 시 복귀 카메라")]
    public CinemachineVirtualCamera playerVCam; // PlayerVCam

    [Header("플레이어 루트(선택)")]
    public Transform playerRoot;

    [Header("페이드 설정")]
    public CanvasGroup fadeGroup;               // FadeCanvas의 CanvasGroup(검정 이미지)
    public float fadeTime = 0.5f;               // 한쪽(닫기/열기) 시간
    [Range(0f,1f)] public float midAlpha = 0.6f;// 완전 블랙 대신 이 값까지만
    public bool useSmoothstep = true;           // 이징 적용
    public bool startFromBlack = false;         // true면 시작을 완전 블랙으로

    [Header("모션 프리롤(전환 중에도 다음 컷이 미리 움직임)")]
    [Tooltip("음수면 fadeTime을 자동 사용")]
    public float motionPreRoll = -1f;

    [Header("스킵")]
    public bool canSkip = true;
    public KeyCode skipKey = KeyCode.Space;

    IEnumerator Start()
    {
        // 시작 상태 정리
        if (uiGroup){ uiGroup.alpha = 0f; uiGroup.interactable = false; uiGroup.blocksRaycasts = false; }
        SetPlayerControls(false);

        if (shots == null || shots.Length == 0) yield break;
        foreach (var v in shots) if (v) v.Priority = 0;
        if (playerVCam) playerVCam.Priority = 0;

        // holdSeconds 길이 보정
        if (holdSeconds == null || holdSeconds.Length < shots.Length)
        {
            var n = new float[shots.Length];
            for (int i = 0; i < shots.Length; i++)
                n[i] = (holdSeconds != null && i < holdSeconds.Length) ? holdSeconds[i] : 1f;
            holdSeconds = n;
        }

        // 페이드 초기 알파
        if (fadeGroup)
            fadeGroup.alpha = startFromBlack ? 1f : midAlpha;

        yield return null; // Brain 준비

        float pre = (motionPreRoll < 0f) ? fadeTime : motionPreRoll;

        // ── 첫 샷: 모션을 '조금 더 길게' 시작해두고, 열면서 보여줌
        shots[0].Priority = 30;                             // 컷
        if (motions != null && motions.Length > 0 && motions[0] != null)
            motions[0].Play(holdSeconds[0] + pre);          // 다음 전환 중에도 계속 움직이도록 프리롤 포함
        yield return FadeTo(0f, fadeTime);                  // 열기
        yield return WaitOrSkip(holdSeconds[0]);            // 보이는 구간 유지

        // ── 나머지 샷 순회
        for (int i = 1; i < shots.Length; i++)
        {
            // 1) 다음 컷 모션을 '미리' 시작 (전환 동안에도 계속 움직이게)
            if (motions != null && i < motions.Length && motions[i] != null)
                motions[i].Play(holdSeconds[i] + pre);

            // 2) 중간까지만 어둡게 닫는 동안, 양쪽 컷은 각각 계속 움직임
            yield return FadeTo(midAlpha, fadeTime);

            // 3) 컷 전환
            shots[i - 1].Priority = 0;
            shots[i].Priority     = 30;

            // 4) 밝게 → 전환 뒤에도 이미 진행 중인 모션이 이어져 보임
            yield return FadeTo(0f, fadeTime);

            // 5) 보이는 구간 유지 시간만큼 대기
            yield return WaitOrSkip(holdSeconds[i]);
        }

        // ── 플레이 시작으로 넘어가기
        yield return FadeTo(midAlpha, fadeTime);            // 닫고
        foreach (var v in shots) if (v) v.Priority = 0;

        if (playerVCam) playerVCam.Priority = 30;           // PlayerVCam으로 컷

        // UI/조작 ON
        if (uiGroup){ uiGroup.alpha = 1f; uiGroup.interactable = true; uiGroup.blocksRaycasts = true; }
        SetPlayerControls(true);
        if (playerRoot) playerRoot.gameObject.SetActive(true);

        yield return FadeTo(0f, fadeTime);                  // 열고 끝
    }

    // ── Helpers ────────────────────────────────────────────────────────────
    IEnumerator FadeTo(float target, float time)
    {
        if (!fadeGroup) yield break;
        float start = fadeGroup.alpha;
        float t = 0f;
        time = Mathf.Max(0.01f, time);

        while (t < 1f)
        {
            t += Time.deltaTime / time;
            float k = useSmoothstep ? (t * t * (3f - 2f * t)) : t; // SmoothStep
            fadeGroup.alpha = Mathf.Lerp(start, target, k);
            yield return null;
        }
        fadeGroup.alpha = target;
    }

    IEnumerator WaitOrSkip(float sec)
    {
        if (!canSkip) { yield return new WaitForSeconds(sec); yield break; }
        float t = 0f;
        while (t < sec)
        {
            if (Input.GetKeyDown(skipKey)) break;
            // 모바일 탭 스킵도 허용하려면 아래 주석 해제
            // if (Input.touchCount > 0) break;
            t += Time.deltaTime;
            yield return null;
        }
    }

    void SetPlayerControls(bool on)
    {
        if (playerControls == null) return;
        foreach (var b in playerControls) if (b) b.enabled = on;
    }
}
