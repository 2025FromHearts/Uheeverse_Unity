using UnityEngine;
using UnityEngine.EventSystems; // UI 위 클릭 방지용

public class MyBoothDoor : MonoBehaviour
{
    [SerializeField] private GameObject uiPanel_MyBooth;   // 클릭 시 켤 패널 (처음엔 비활성화 권장)
    [SerializeField] private LayerMask clickableLayers;     // 클릭 허용 건물(오브젝트) 레이어
    [SerializeField] private float maxDistance = 100f;      // 레이캐스트 거리

    Camera cam;

    void Awake()
    {
        cam = Camera.main;
        if (uiPanel_MyBooth != null) uiPanel_MyBooth.SetActive(false); // 선택: 시작 시 꺼두기
    }

    void Update()
    {
        // UI 위를 클릭한 경우(버튼/스크롤 등) 게임 월드 클릭 처리 막기
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, clickableLayers))
        {
            // 원하는 경우 태그로도 추가 필터링 가능:
            // if (!hit.collider.CompareTag("MyBooth")) return;

            if (uiPanel_MyBooth != null)
                uiPanel_MyBooth.SetActive(true);
            else
                Debug.LogWarning("[MyBoothDoor] uiPanel_MyBooth가 할당되지 않았습니다.");
        }
    }
}
