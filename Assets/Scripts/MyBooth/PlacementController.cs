using UnityEngine;
using System;

public class PlacementController : MonoBehaviour
{
    [Header("Refs")]
    public Camera cam;
    public PlaceableCatalog catalog;

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

    GameObject ghost;
    PlaceableEntry current;
    float rotY;
    bool canPlace;

    public struct PlacementResult { public string key; public Vector3 position; public float rotY; }
    public event Action<PlacementResult> OnPlaced;
    public event Action OnPreviewCanceled;

    void Reset() { cam = Camera.main; }

    public System.Action onPlacementComplete;

    // BeginPreview - 아이템 키 + 위치 오프셋용 메서드
    public void BeginPreview(string itemKey, Vector3 positionOffset)
    {
        BeginPreviewInternal(itemKey, Vector3.zero, positionOffset);
    }

    // BeginPreview - 위치 고정 + 오프셋용 메서드 (필요 시 사용)
    public void BeginPreview(string itemKey, Vector3 fixedPos, Vector3 positionOffset)
    {
        BeginPreviewInternal(itemKey, fixedPos, positionOffset);
    }

    // 실제 내부 구현 (공통)
    private void BeginPreviewInternal(string itemKey, Vector3 fixedPos, Vector3 positionOffset)
    {
        CancelPreview();

        if (!catalog)
        {
            Debug.LogError("[Placement] Catalog not set");
            return;
        }

        current = catalog.Find(itemKey);
        if (current == null || current.prefab == null)
        {
            Debug.LogError("[Placement] Prefab not found for key: " + itemKey);
            return;
        }

        ghost = Instantiate(current.prefab);
        foreach (var c in ghost.GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }

        Vector3 basePos = fixedPos + positionOffset;
        ghost.transform.position = basePos;

        int ghostLayer = LayerMask.NameToLayer(ghostLayerName);
        if (ghostLayer >= 0)
        {
            SetLayerRecursively(ghost, ghostLayer);
        }

        ApplyGhostMaterial(ghostInvalidMat);
        rotY = 0f;
    }

    public void CancelPreview()
    {
        if (ghost) Destroy(ghost);
        ghost = null;
        current = null;
        OnPreviewCanceled?.Invoke();
    }

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
                GetBoundsCenter(ghost), GetBoundsExtents(ghost),
                ghost.transform.rotation, overlapMask, QueryTriggerInteraction.Ignore);

            ApplyGhostMaterial(canPlace ? ghostValidMat : ghostInvalidMat);

            if (Input.GetMouseButtonDown(0) && canPlace)
                Place();
        }

        if (Input.GetKeyDown(rotateKey))
        {
            rotY = Mathf.Repeat(rotY + rotationStep, 360f);
            ghost.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
        }
        if (Input.GetKeyDown(cancelKey)) CancelPreview();
    }

    void Place()
    {
        var go = Instantiate(current.prefab, ghost.transform.position, ghost.transform.rotation);
        foreach (var c in go.GetComponentsInChildren<Collider>()) c.enabled = true;

        int placedLayer = LayerMask.NameToLayer(placedLayerName);
        if (placedLayer >= 0) SetLayerRecursively(go, placedLayer);

        OnPlaced?.Invoke(new PlacementResult
        {
            key = current.key,
            position = go.transform.position,
            rotY = go.transform.eulerAngles.y
        });

        onPlacementComplete?.Invoke();
        CancelPreview();
    }

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
        var rs = g.GetComponentsInChildren<Renderer>();
        if (rs.Length > 0)
        {
            var b = rs[0].bounds;
            for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
            return b;
        }
        var cs = g.GetComponentsInChildren<Collider>();
        if (cs.Length > 0)
        {
            var b = cs[0].bounds;
            for (int i = 1; i < cs.Length; i++) b.Encapsulate(cs[i].bounds);
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
            for (int i = 0; i < mats.Length; i++) mats[i] = mat;
            r.sharedMaterials = mats;
        }
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform t in obj.transform) SetLayerRecursively(t.gameObject, layer);
    }
}
