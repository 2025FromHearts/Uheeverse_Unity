using System.Collections;
using UnityEngine;
using Cinemachine;

public class ShotMotion : MonoBehaviour
{
    [Header("Move (Local Space)")]
    [Tooltip("카메라 기준 로컬(X=오른쪽, Y=위, Z=앞)으로 이동할 오프셋")]
    public Vector3 localOffset = new Vector3(1.5f, 0f, 0f); // 예: 옆으로 1.5m
    public bool enableMove = true;

    [Header("Zoom (FOV)")]
    public bool enableZoom = false;
    [Tooltip("목표 FOV (예: 35~55)")]
    public float targetFOV = 40f;

    [Header("Ease / Options")]
    public bool useSmoothstep = true;

    CinemachineVirtualCamera _vcam;

    void Awake()
    {
        _vcam = GetComponent<CinemachineVirtualCamera>();
    }

    public void Play(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(Run(duration));
    }

    IEnumerator Run(float duration)
    {
        duration = Mathf.Max(0.01f, duration);

        // 시작 상태
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation; // 회전은 유지 (원하면 추가 가능)

        // 로컬 오프셋을 월드로 환산
        Vector3 worldOffset = transform.right * localOffset.x
                            + transform.up    * localOffset.y
                            + transform.forward * localOffset.z;
        Vector3 endPos = enableMove ? (startPos + worldOffset) : startPos;

        // FOV
        float startFov = _vcam ? _vcam.m_Lens.FieldOfView : 60f;
        float endFov   = enableZoom ? targetFOV : startFov;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float k = useSmoothstep ? (t * t * (3f - 2f * t)) : t;

            if (enableMove) transform.position = Vector3.Lerp(startPos, endPos, k);
            if (_vcam && enableZoom) _vcam.m_Lens.FieldOfView = Mathf.Lerp(startFov, endFov, k);

            yield return null;
        }

        // 최종 값 스냅
        if (enableMove) transform.position = endPos;
        if (_vcam && enableZoom) _vcam.m_Lens.FieldOfView = endFov;
    }
}
