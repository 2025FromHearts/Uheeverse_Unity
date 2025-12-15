using UnityEngine;
using System;

public class PlacementController : MonoBehaviour
{
    [Header("Refs")]
    public Camera cam;

    [Header("Layers & Masks")]
    public LayerMask groundMask;
    public LayerMask overlapMask;
    public string ghostLayerName = "Ghost";
    public string placedLayerName = "PlacedItem";

    [Header("Placement Settings")]
    public float cellSize = 1f;
    public float rotationStep = 90f;
    public bool enableOverlapCheck = true;
    public float heightOffset = -0.1f;

    [Header("Ghost Visuals")]
    public Material ghostValidMat;
    public Material ghostInvalidMat;


    [Header("Pad Input")]
    public PadInputEventRouter padInput;
    public UdpPadReceiver pad;
    public float padMoveSpeed = 10f;
    public float padDeadZone = 0.25f;

    public KeyCode rotateKey = KeyCode.R;

    GameObject ghost;
    GameObject currentPrefab;
    float rotY;
    bool canPlace;
    bool isPlacingWithPad;

    Vector3 padPosition;
    float prefabBottomOffset;
    public Action onPlacementComplete;

    // ---------- Result ----------
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

    // 기존 호환용
    public void BeginPreview(string prefabName, Vector3 fixedPos, Vector3 offset)
    {
        BeginPreview(prefabName, fixedPos, offset, false);
    }

    // =========================
    // Preview Start
    // =========================
    public void BeginPreview(string prefabName, Vector3 fixedPos, Vector3 offset, bool fromPad)
    {
        CancelPreviewInternal();

        isPlacingWithPad = fromPad;

        if (isPlacingWithPad && padInput != null)
        {
            padInput.currentMode = PadInputEventRouter.InputMode.Placement;

            var player = FindAnyObjectByType<PlayerInputController>();
            if (player != null)
                player.canMove = false;
        }

        var prefab = Resources.Load<GameObject>("ItemModels/" + prefabName);
        if (!prefab)
        {
            Debug.LogError($"[Placement] Prefab not found: {prefabName}");
            return;
        }

        currentPrefab = prefab;
        ghost = Instantiate(prefab);

        CachePrefabBottomOffset();

        foreach (var c in ghost.GetComponentsInChildren<Collider>())
            c.enabled = false;

        padPosition = fixedPos + offset;
        ghost.transform.position = padPosition;
        ghost.transform.rotation = Quaternion.identity;
        rotY = 0f;

        int ghostLayer = LayerMask.NameToLayer(ghostLayerName);
        if (ghostLayer >= 0)
            SetLayerRecursively(ghost, ghostLayer);

        ApplyGhostMaterial(ghostInvalidMat);
    }

    // =========================
    // Cancel
    // =========================
    public void CancelPreview()
    {
        CancelPreviewInternal();

        if (padInput != null)
            padInput.currentMode = PadInputEventRouter.InputMode.Player;

        var player = FindAnyObjectByType<PlayerInputController>();
        if (player != null)
            player.canMove = true;

        OnPreviewCanceled?.Invoke();
    }

    void CancelPreviewInternal()
    {
        if (ghost) Destroy(ghost);
        ghost = null;
        currentPrefab = null;
        canPlace = false;
        isPlacingWithPad = false;
    }

    void Update()
    {
        if (!ghost) return;

        if (isPlacingWithPad)
            UpdatePadPlacement();
        else
            UpdateMousePlacement();
    }
    void CachePrefabBottomOffset()
    {
        // 1. PlacementBase가 있으면 최우선
        Transform basePoint = ghost.transform.Find("PlacementBase");
        if (basePoint != null)
        {
            prefabBottomOffset = basePoint.position.y - ghost.transform.position.y;
            return;
        }

        // 2. 없으면 BoxCollider 기준 (fallback)
        var box = ghost.GetComponentInChildren<BoxCollider>();
        if (box != null)
        {
            Vector3 centerWorld = box.transform.TransformPoint(box.center);
            float bottomY = centerWorld.y - box.size.y * 0.5f * box.transform.lossyScale.y;
            prefabBottomOffset = bottomY - ghost.transform.position.y;
            return;
        }

        // 3. 그래도 없으면 0
        prefabBottomOffset = 0f;
    }


    // =========================
    // Mouse Placement
    // =========================
    void UpdateMousePlacement()
    {
        var camUse = cam ? cam : Camera.main;
        if (!camUse) return;

        if (Input.GetKeyDown(rotateKey))
        {
            rotY = Mathf.Repeat(rotY + rotationStep, 360f);
            ghost.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
        }

        if (Physics.Raycast(camUse.ScreenPointToRay(Input.mousePosition), out var hit, 2000f, groundMask))
        {
            Vector3 pos = SnapXZ(hit.point, cellSize);
            pos.y = hit.point.y;

            ghost.transform.SetPositionAndRotation(
                pos,
                Quaternion.Euler(0f, rotY, 0f)
            );

            UpdateCanPlace();

            if (Input.GetMouseButtonDown(0) && canPlace)
                Place();
        }
    }

    // =========================
    // Pad Placement
    // =========================
    bool isMovingWithPad;

    void UpdatePadPlacement()
    {
        if (pad == null || pad.latest == null) return;

        float lx = pad.latest.lx;
        float ly = pad.latest.ly;

        Vector3 move = new Vector3(lx, 0f, ly);

        if (move.magnitude > padDeadZone)
        {
            isMovingWithPad = true;

            padPosition += move * padMoveSpeed * Time.unscaledDeltaTime;

            if (Physics.Raycast(padPosition + Vector3.up * 5f, Vector3.down, out var hit, 50f, groundMask))
                padPosition.y = hit.point.y - prefabBottomOffset + heightOffset;

            ghost.transform.position = padPosition;
        }
        else if (isMovingWithPad)
        {
            isMovingWithPad = false;
            padPosition = SnapXZ(padPosition, cellSize);
            ghost.transform.position = padPosition;
        }

        ghost.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
        UpdateCanPlace();
    }


    void UpdateCanPlace()
    {
        canPlace = !enableOverlapCheck || !Physics.CheckBox(
            GetBoundsCenter(ghost),
            GetBoundsExtents(ghost),
            ghost.transform.rotation,
            overlapMask,
            QueryTriggerInteraction.Ignore
        );

        ApplyGhostMaterial(canPlace ? ghostValidMat : ghostInvalidMat);
    }

    // =========================
    // Pad Buttons
    // =========================
    void OnEnable()
    {
        if (!padInput) return;

        padInput.OnAPressed += OnPadConfirm;
        padInput.OnBPressed += OnPadRotate;
        padInput.OnXPressed += OnPadCancel;
    }

    void OnDisable()
    {
        if (!padInput) return;

        padInput.OnAPressed -= OnPadConfirm;
        padInput.OnBPressed -= OnPadRotate;
        padInput.OnXPressed -= OnPadCancel;
    }

    void OnPadConfirm()
    {
        if (!isPlacingWithPad || !ghost || !canPlace) return;
        Place();
    }

    void OnPadRotate()
    {
        if (!isPlacingWithPad || !ghost) return;
        rotY = Mathf.Repeat(rotY + rotationStep, 360f);
    }

    void OnPadCancel()
    {
        if (!isPlacingWithPad) return;
        CancelPreview();
        onPlacementComplete?.Invoke();
        if (padInput != null)
            padInput.currentMode = PadInputEventRouter.InputMode.Player;
    }

    // =========================
    // Place
    // =========================
    void Place()
    {
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

        var player = FindAnyObjectByType<PlayerInputController>();
        if (player != null)
            player.canMove = true;

        if (padInput != null)
            padInput.currentMode = PadInputEventRouter.InputMode.Player;
    }

    // =========================
    // Utils
    // =========================
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
        if (!ghost || !mat) return;

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
