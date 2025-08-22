using UnityEngine;
using UnityEngine.EventSystems;

public class MyBoothTOMystation : MonoBehaviour
{
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private LayerMask clickableLayers; // Everything로 둬도 되고, 현재 레이어만 체크해도 됨
    [SerializeField] private float maxDistance = 2000f;

    Camera cam;

    void Awake()
    {
        cam = Camera.main;
        if (uiPanel) uiPanel.SetActive(false);
    }

    void Update()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // 트리거 콜라이더도 맞추고 싶으면 마지막 인자에 QueryTriggerInteraction.Collide
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, clickableLayers, QueryTriggerInteraction.Collide))
        {
            // "나 자신"을 맞췄는지 확인
            if (hit.collider.transform == transform)   // <= 이 한 줄
            {
                if (uiPanel) uiPanel.SetActive(true);
            }
        }
    }
}
