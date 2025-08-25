using UnityEngine;
using UnityEngine.EventSystems;

public class Store : MonoBehaviour
{
    [SerializeField] private GameObject Mystation_Store;   // Canvas 밑 패널
    [SerializeField] private LayerMask storeLayer;       // storeLayer 체크
    [SerializeField] private float maxDistance = 200f;

    Camera cam;
    private ShopUI ShopUI;

    void Awake()
    {
        cam = Camera.main; // MainCamera 태그 꼭!
        if (Mystation_Store != null)
        {
            Mystation_Store.SetActive(false);
            ShopUI = Mystation_Store.GetComponent<ShopUI>(); // 캐시

            // 닫을 때 루트도 꺼줌 (선택)
            if (ShopUI != null)
                ShopUI.OnShopClosed += () => Mystation_Store.SetActive(false);
        }
    }

    void Update()
    {
            // UI 위 클릭이면 월드 클릭 무시
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (!Input.GetMouseButtonDown(0)) return;

            if (cam == null) { Debug.LogWarning("MainCamera 태그 확인해줘"); return; }

            var ray = cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, maxDistance, storeLayer)) return;

            // 내 가게만 반응
            if (!hit.collider.transform.IsChildOf(transform)) return;

            if (Mystation_Store == null) { Debug.LogWarning($"{name}: ui 미할당"); return; }

            Mystation_Store.SetActive(true);   // 루트 on
            if (ShopUI == null) ShopUI = Mystation_Store.GetComponent<ShopUI>();
            if (ShopUI != null) ShopUI.OpenShop();
            Debug.Log($"{name}: UI ON");
    }
    
}
