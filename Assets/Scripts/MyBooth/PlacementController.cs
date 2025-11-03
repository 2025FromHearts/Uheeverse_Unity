using UnityEngine;
using System;

public class PlacementController : MonoBehaviour
{
    [Header("Refs")]
    public Camera cam; // 메인 카메라 지정

    [Header("Layers & Masks")]
    public LayerMask groundMask;
    public LayerMask overlapMask;
    public string ghostLayerName = "Ghost";
    public string placedLayerName = "PlacedItem";

    [Header("Placement Settings")]
    public float cellSize = 1f;
    public float rotationStep = 90f;
    public KeyCode rotateKey = KeyCode.R;
    public KeyCode cancelKey = KeyCode.Escape;
    public bool enableOverlapCheck = true;

    [Header("Ghost Visuals (optional)")]
    public Material ghostValidMat;
    public Material ghostInvalidMat;

    private GameObject ghost;
    private GameObject currentPrefab;
    private float rotY;
    private bool canPlace;

    public Action onPlacementComplete;

    // 배치 결과 데이터 구조
    public struct PlacementResult
    {
        public string key;
        public Vector3 position;
        public float rotY;
        public Vector3 scale;
        public GameObject placedObject;
    }

    public event Action<PlacementResult> OnPlaced;
    public event Action OnPreviewCanceled;

    void Reset()
    {
        cam = Camera.main;
    }

    // 미리보기 시작 (카탈로그 없이 Resources 폴더에서 직접 로드)
    public void BeginPreview(string prefabName, Vector3 fixedPos, Vector3 offset)
    {
        CancelPreview();

        // ItemModels 폴더 기준으로 프리팹 찾기
        var prefab = Resources.Load<GameObject>("ItemModels/" + prefabName);
        if (prefab == null)
        {
            Debug.LogError($"[Placement] Prefab not found in Resources/ItemModels: {prefabName}");
            return;
        }

        currentPrefab = prefab;
        ghost = Instantiate(prefab);

        // 콜라이더 비활성화 (고스트는 충돌 안 함)
        foreach (var c in ghost.GetComponentsInChildren<Collider>())
            c.enabled = false;

        ghost.transform.position = fixedPos + offset;
        ghost.transform.rotation = Quaternion.identity;

        int ghostLayer = LayerMask.NameToLayer(ghostLayerName);
        if (ghostLayer >= 0)
            SetLayerRecursively(ghost, ghostLayer);

        ApplyGhostMaterial(ghostInvalidMat);
        rotY = 0f;
    }

    // 미리보기 취소
    public void CancelPreview()
    {
        if (ghost) Destroy(ghost);
        ghost = null;
        currentPrefab = null;
        OnPreviewCanceled?.Invoke();
    }

    // 배치 갱신 루프
    void Update()
    {
        if (!ghost) return;
        var camUse = cam ? cam : Camera.main;
        if (!camUse) return;

        if (Physics.Raycast(camUse.ScreenPointToRay(Input.mousePosition), out var hit, 2000f, groundMask))
        {
            Vector3 pos = SnapXZ(hit.point, cellSize);
            pos.y = hit.point.y;
            ghost.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, rotY, 0f));

            canPlace = !enableOverlapCheck || !Physics.CheckBox(
                GetBoundsCenter(ghost),
                GetBoundsExtents(ghost),
                ghost.transform.rotation,
                overlapMask,
                QueryTriggerInteraction.Ignore
            );

            ApplyGhostMaterial(canPlace ? ghostValidMat : ghostInvalidMat);

            if (Input.GetMouseButtonDown(0) && canPlace)
                Place();
        }

        if (Input.GetKeyDown(rotateKey))
        {
            rotY = Mathf.Repeat(rotY + rotationStep, 360f);
            ghost.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
        }

        if (Input.GetKeyDown(cancelKey))
            CancelPreview();
    }

    // 실제 배치
    void Place()
    {
        if (currentPrefab == null) return;

        var go = Instantiate(currentPrefab, ghost.transform.position, ghost.transform.rotation);
        foreach (var c in go.GetComponentsInChildren<Collider>())
            c.enabled = true;

        int placedLayer = LayerMask.NameToLayer(placedLayerName);
        if (placedLayer >= 0)
            SetLayerRecursively(go, placedLayer);

        OnPlaced?.Invoke(new PlacementResult
        {
            key = currentPrefab.name,
            position = go.transform.position,
            rotY = go.transform.eulerAngles.y,
            scale = go.transform.localScale,
            placedObject = go
        });

        onPlacementComplete?.Invoke();
        CancelPreview();
    }

    // 유틸리티
    static Vector3 SnapXZ(Vector3 p, float cell)
    {
        return new Vector3(
            Mathf.Round(p.x / cell) * cell,
            p.y,
            Mathf.Round(p.z / cell) * cell
        );
    }

    Bounds GetWorldBounds(GameObject g)
    {
        var renderers = g.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            var b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
            return b;
        }

        var colliders = g.GetComponentsInChildren<Collider>();
        if (colliders.Length > 0)
        {
            var b = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++) b.Encapsulate(colliders[i].bounds);
            return b;
        }

        return new Bounds(g.transform.position, Vector3.one * 0.5f);
    }

    Vector3 GetBoundsCenter(GameObject g) => GetWorldBounds(g).center;
    Vector3 GetBoundsExtents(GameObject g) => GetWorldBounds(g).extents * 0.98f;

    void ApplyGhostMaterial(Material mat)
    {
        if (!mat || !ghost) return;
        foreach (var r in ghost.GetComponentsInChildren<Renderer>())
        {
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
                mats[i] = mat;
            r.sharedMaterials = mats;
        }
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform t in obj.transform)
            SetLayerRecursively(t.gameObject, layer);
    }
}
