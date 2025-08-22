using UnityEngine;
using UnityEngine.EventSystems;

public class Store : MonoBehaviour
{
    [SerializeField] private GameObject uiPanel_Store;   // Canvas 밑 패널
    [SerializeField] private LayerMask storeLayer;       // storeLayer 체크
    [SerializeField] private float maxDistance = 200f;

    Camera cam;

    void Awake()
    {
        cam = Camera.main; // MainCamera 태그 꼭!
        if (uiPanel_Store != null) uiPanel_Store.SetActive(false);
    }

    void Update()
    {
        // UI 위 클릭이면 월드 클릭 무시
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (!Input.GetMouseButtonDown(0)) return;

        if (cam == null) { Debug.LogWarning("MainCamera 태그 확인해줘"); return; }

        var ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, storeLayer))
        {
            // 맞은 콜라이더가 이 건물의 자식인가?
            if (hit.collider.transform.IsChildOf(transform))
            {
                if (uiPanel_Store != null)
                {
                    uiPanel_Store.SetActive(true);
                    Debug.Log($"{name}: UI ON");
                }
                else Debug.LogWarning($"{name}: uiPanel_Store 미할당");
            }
            // 다른 가게 맞았으면 내 패널은 켜지지 않음 (원하면 여기서 끄기 처리 가능)
        }
    }
}
