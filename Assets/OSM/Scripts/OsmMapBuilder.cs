using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using SimpleJSON;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class OsmMapBuilder : MonoBehaviour
{
    [Header("Roots")]
    public Transform root;
    public string rootName = "OSM_Generated";

    [Header("OSM File (StreamingAssets)")]
    public string jsonFileName = "CheongsongOsm.json";

    [Header("LatLon Mapping")]
    public float baseLat = 36.4325726f;
    public float baseLon = 129.0541124f;
    public float scale   = 300000f;
    public Vector3 manualOffset = Vector3.zero;

    [Header("Exclude Settings")]
    public Transform centerTransform;
    public Vector3 center = Vector3.zero;
    public bool useCircleExclude = false;
    public float excludeRadius   = 50f;
    public bool useBoxExclude    = false;
    public Vector2 excludeBoxSize = new Vector2(220, 220);

    [Header("Lines (LineRenderer)")]
    public Material roadMaterial; public float roadWidth = 3f;
    public Material riverMaterial; public float riverWidth = 5f;
    [Tooltip("라인 길이 기준 텍스처 반복(타일) 간격(m)")]
    public float metersPerTile = 2f;
    public int cornerVertices = 2, capVertices = 2;
    [Tooltip("라인 정렬 모드. View 권장(항상 보임)")]
    public bool alignToView = true;
    [Tooltip("지면과의 간섭 방지를 위한 살짝 띄우기(+)/내리기(-)")]
    public float roadYOffset  = 0.02f;
    public float riverYOffset = 0.00f;

    [Header("Building Mapping (OSM tag -> Prefab)")]
    public GameObject defaultBuilding, schoolPrefab, hospitalPrefab, policePrefab, shopPrefab, busStopPrefab, treePrefab, parkPrefab, statuePrefab;

    [Header("Prefab Scales")]
    public float buildingScale = 1f, schoolScale = 1f, hospitalScale = 1f, policeScale = 1f, shopScale = 1f,
                 busStopScale = 1f, treeScale = 1f, parkScale = 1f, statueScale = 1f;

    // 내부 상태
    readonly Dictionary<long, Vector3>   nodePos  = new();
    readonly Dictionary<long, JSONArray> wayNodes = new();   // wayId -> node id list
    readonly List<GameObject>            spawned  = new();

    // ---------------- 공개 메서드 ----------------
    public void BuildFromOSM()
    {
        EnsureRoot();
        ClearGenerated();

        var elements = LoadElements();
        if (elements == null) return;

        int nodePOI = 0, wayPOI = 0, relPOI = 0;

        // 1) 모든 node 저장 + point POI 배치
        for (int i = 0; i < elements.Count; i++)
        {
            JSONNode e = elements[i];
            if (e["type"] != "node") continue;

            long id = e["id"].AsLong;
            float lat = e["lat"], lon = e["lon"];
            var pos = LatLonToUnity(lat, lon);
            nodePos[id] = pos;

            if (!HasKey(e, "tags")) continue;
            if (IsInsideExcludeZone(pos)) continue;
            if (TrySpawnPOI(pos, e["tags"])) nodePOI++;
        }

        // 2) way: 폴리곤 중심 POI + 라인 생성
        for (int i = 0; i < elements.Count; i++)
        {
            JSONNode e = elements[i];
            if (e["type"] != "way") continue;

            long wayId = e["id"].AsLong;
            JSONArray nodes = e["nodes"].AsArray;
            if (nodes != null) wayNodes[wayId] = nodes;

            if (!HasKey(e, "tags")) continue;
            var tags = e["tags"];

            // 폴리곤/영역 유형이면 중심에 프리팹 하나
            if (HasKey(tags, "building") || HasKey(tags, "building:part") ||
                HasKey(tags, "amenity")  || HasKey(tags, "shop") ||
                HasKey(tags, "leisure")  || HasKey(tags, "historic"))
            {
                if (TrySpawnPOIFromWay(tags, nodes)) wayPOI++;
            }

            // 라인 포인트 모으기
            var pts = new List<Vector3>(nodes?.Count ?? 0);
            if (nodes != null)
            {
                for (int j = 0; j < nodes.Count; j++)
                {
                    long nid = nodes[j].AsLong;
                    if (nodePos.TryGetValue(nid, out var p)) pts.Add(p);
                }
            }
            if (pts.Count < 2) continue;

            if (HasKey(tags, "highway") && roadMaterial)
                SpawnLine(pts, roadMaterial, roadWidth, "Road", isRiver:false);

            if ((HasKey(tags, "waterway") && tags["waterway"] == "river") ||
                (HasKey(tags, "natural")  && tags["natural"]  == "water"))
            {
                if (riverMaterial) SpawnLine(pts, riverMaterial, riverWidth, "River", isRiver:true);
            }
        }

        // 3) relation: multipolygon(주로 건물) 간단 처리
        for (int i = 0; i < elements.Count; i++)
        {
            JSONNode e = elements[i];
            if (e["type"] != "relation" || !HasKey(e, "tags")) continue;
            var tags = e["tags"];

            bool looksLikeBuilding =
                HasKey(tags, "building") || HasKey(tags, "building:part") ||
                (HasKey(tags, "type") && tags["type"] == "multipolygon");
            if (!looksLikeBuilding) continue;

            JSONArray members = e["members"].AsArray;
            if (members == null || members.Count == 0) continue;

            Vector3 sum = Vector3.zero; int cnt = 0;
            for (int m = 0; m < members.Count; m++)
            {
                var mem = members[m];
                if (mem["type"] != "way") continue;
                long wid = mem["ref"].AsLong;
                if (!wayNodes.TryGetValue(wid, out var nlist) || nlist == null) continue;

                for (int j = 0; j < nlist.Count; j++)
                {
                    long nid = nlist[j].AsLong;
                    if (nodePos.TryGetValue(nid, out var p)) { sum += p; cnt++; }
                }
            }
            if (cnt == 0) continue;

            var centerPos = sum / cnt; centerPos.y = 0.1f;
            if (!IsInsideExcludeZone(centerPos))
                if (TrySpawnPOI(centerPos, tags)) relPOI++;
        }

        MarkStatic(spawned, true);
        Debug.Log($"[OSM] Build complete. Spawned:{spawned.Count} (nodePOI:{nodePOI}, wayPOI:{wayPOI}, relPOI:{relPOI})");
    }

    public void ClearGenerated()
    {
        EnsureRoot();
        for (int i = root.childCount - 1; i >= 0; i--)
            DestroyImmediate(root.GetChild(i).gameObject);
        spawned.Clear();
        nodePos.Clear();
        wayNodes.Clear();
        Debug.Log("[OSM] Cleared.");
    }

    public void Bake()
    {
        foreach (var go in spawned) if (go) go.tag = "Untagged";
        spawned.Clear();
        nodePos.Clear();
        wayNodes.Clear();
        Debug.Log("[OSM] Baked.");
    }

    // ---------- 태그 스캐너(유지) ----------
    public void ScanTags(int topN = 20)
    {
        var elements = LoadElements();
        if (elements == null) { Debug.LogError("[OSM Scan] elements not found"); return; }

        int nodes = 0, ways = 0, rels = 0;
        var keyCount   = new Dictionary<string, int>();
        var valueCount = new Dictionary<string, Dictionary<string, int>>();

        for (int i = 0; i < elements.Count; i++)
        {
            var e = elements[i];
            string t = e["type"];
            if (t == "node") nodes++;
            else if (t == "way") ways++;
            else if (t == "relation") rels++;

            if (!HasKey(e, "tags")) continue;
            var tagsObj = e["tags"].AsObject;
            foreach (var k in tagsObj.Keys)
            {
                keyCount[k] = keyCount.TryGetValue(k, out var c1) ? c1 + 1 : 1;

                string v = tagsObj[k]?.Value ?? "(empty)";
                if (!valueCount.TryGetValue(k, out var dict))
                    valueCount[k] = dict = new Dictionary<string, int>();
                dict[v] = dict.TryGetValue(v, out var c2) ? c2 + 1 : 1;
            }
        }

        Debug.Log($"[OSM Scan] elements:{elements.Count}  nodes:{nodes}  ways:{ways}  relations:{rels}");

        var topKeys = keyCount.OrderByDescending(kv => kv.Value).Take(topN);
        var sb = new StringBuilder();
        sb.AppendLine("[OSM Scan] Top tag keys:");
        foreach (var kv in topKeys) sb.AppendLine($"- {kv.Key}: {kv.Value}");
        Debug.Log(sb.ToString());

        string[] interesting = { "building", "amenity", "shop", "highway", "waterway", "natural", "leisure", "bridge", "type" };
        foreach (var k in interesting)
        {
            if (!valueCount.TryGetValue(k, out var dict)) continue;
            var sb2 = new StringBuilder();
            sb2.AppendLine($"[OSM Scan] {k} values (top {topN}):");
            foreach (var kv in dict.OrderByDescending(x => x.Value).Take(topN))
                sb2.AppendLine($"  - {kv.Key}: {kv.Value}");
            Debug.Log(sb2.ToString());
        }
    }

    // ---------- 내부 유틸 ----------
    void EnsureRoot()
    {
        if (root) return;
        var ex = GameObject.Find(rootName);
        if (ex) root = ex.transform;
        else { var r = new GameObject(rootName); r.transform.SetParent(transform, false); root = r.transform; }
    }

    JSONArray LoadElements()
    {
        string path = Path.Combine(Application.streamingAssetsPath, jsonFileName);
        if (!File.Exists(path)) { Debug.LogError($"[OSM] File not found: {path}"); return null; }
        var json = JSON.Parse(File.ReadAllText(path));
        var arr  = json?["elements"]?.AsArray;
        if (arr == null) Debug.LogError("[OSM] elements array not found.");
        return arr;
    }

    static bool HasKey(JSONNode n, string key)
    {
        var obj = n as JSONObject;
        return obj != null && obj.HasKey(key);
    }

    bool TrySpawnPOI(Vector3 pos, JSONNode tags)
    {
        GameObject pf = null; float sc = 1f;

        if (HasKey(tags, "natural") && tags["natural"] == "tree" && treePrefab) { pf = treePrefab; sc = treeScale; }
        else if (HasKey(tags, "amenity"))
        {
            string a = tags["amenity"];
            if ((a == "school" || a == "college" || a == "university") && schoolPrefab) { pf = schoolPrefab; sc = schoolScale; }
            else if ((a == "hospital" || a == "clinic") && hospitalPrefab) { pf = hospitalPrefab; sc = hospitalScale; }
            else if (a == "police" && policePrefab) { pf = policePrefab; sc = policeScale; }
            else if ((a == "bus_station" || a == "bus_stop") && busStopPrefab) { pf = busStopPrefab; sc = busStopScale; }
            else { pf = defaultBuilding; sc = buildingScale; } // fallback
        }
        else if (HasKey(tags, "shop"))     { pf = shopPrefab   ? shopPrefab   : defaultBuilding; sc = shopPrefab   ? shopScale   : buildingScale; }
        else if (HasKey(tags, "leisure"))  { pf = parkPrefab   ? parkPrefab   : defaultBuilding; sc = parkPrefab   ? parkScale   : buildingScale; }
        else if (HasKey(tags, "historic")) { pf = statuePrefab ? statuePrefab : defaultBuilding; sc = statuePrefab ? statueScale : buildingScale; }
        else if (HasKey(tags, "building") || HasKey(tags, "building:part")) { pf = defaultBuilding; sc = buildingScale; }

        if (!pf) return false;

#if UNITY_EDITOR
        var go = (GameObject)PrefabUtility.InstantiatePrefab(pf, root);
        go.transform.position = pos;
#else
        var go = Instantiate(pf, pos, Quaternion.identity, root);
#endif
        go.transform.localScale *= sc;
        spawned.Add(go);
        return true;
    }

    bool TrySpawnPOIFromWay(JSONNode tags, JSONArray nodeRefs)
    {
        if (nodeRefs == null || nodeRefs.Count == 0) return false;
        var c = ComputeCentroid(nodeRefs);
        if (c == Vector3.zero) return false;
        if (IsInsideExcludeZone(c)) return false;
        return TrySpawnPOI(c, tags);
    }

    Vector3 ComputeCentroid(JSONArray nodeRefs)
    {
        Vector3 sum = Vector3.zero; int count = 0;
        for (int j = 0; j < nodeRefs.Count; j++)
        {
            long id = nodeRefs[j].AsLong;
            if (nodePos.TryGetValue(id, out var p)) { sum += p; count++; }
        }
        if (count == 0) return Vector3.zero;
        var c = sum / count; c.y = 0.1f;
        return c;
    }

    void SpawnLine(List<Vector3> worldPts, Material mat, float width, string name, bool isRiver)
    {
        var go = new GameObject(name);
        go.transform.SetParent(root, false);

        // ① 라인 리본을 바닥(XZ)에 눕히기: Z(법선)이 위(+Y)를 보도록 회전
        //    - 기본 리본은 XY 평면에 그려짐 → XZ 평면으로 눕히려면 X축 90도 회전
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        go.transform.localScale    = Vector3.one;

        float yOff = isRiver ? riverYOffset : roadYOffset;

        // ② 월드 → 로컬 좌표 변환 + 미세 오프셋
        var localPts = new Vector3[worldPts.Count];
        for (int i = 0; i < worldPts.Count; i++)
        {
            var wp = worldPts[i];
            // 도로 높이 살짝 띄우기 (월드 Y)
            wp.y += yOff;

            // 부모(go) 기준 로컬 좌표로 변환
            var lp = go.transform.InverseTransformPoint(wp);

            // TransformZ 정렬에서는 리본의 '법선'이 로컬 Z이므로,
            // z-fighting 방지를 위해 아주 살짝 내려줌(바닥 안쪽으로)
            lp.z -= 0.003f;

            localPts[i] = lp;
        }

        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.positionCount = localPts.Length;
        lr.SetPositions(localPts);

        lr.startWidth = width;
        lr.endWidth   = width;

        lr.numCornerVertices = Mathf.Clamp(cornerVertices, 0, 8);
        lr.numCapVertices    = Mathf.Clamp(capVertices,    0, 8);

        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows    = false;

        // ③ 카메라 빌보드(세로 리본) 금지 → 바닥 평탄 유지
        lr.alignment   = LineAlignment.TransformZ;
        lr.textureMode = LineTextureMode.Tile;

        var inst = Object.Instantiate(mat);
        lr.material = inst;

        // 텍스처 타일링(길이 기반)
        float length = 0f;
        for (int i = 0; i < worldPts.Count - 1; i++)
            length += Vector3.Distance(worldPts[i], worldPts[i + 1]);
        float repeatX = Mathf.Max(1f, length / Mathf.Max(0.01f, metersPerTile));
        SetTiling(inst, repeatX, 1f);

    #if UNITY_EDITOR
        GameObjectUtility.SetStaticEditorFlags(go,
            StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
    #endif
        spawned.Add(go);
    }

    void SetTiling(Material m, float x, float y)
    {
        if (m.HasProperty("_MainTex"))   m.SetTextureScale("_MainTex",   new Vector2(x, y));
        if (m.HasProperty("_BumpMap"))   m.SetTextureScale("_BumpMap",   new Vector2(x, y));
        if (m.HasProperty("_BaseMap"))   m.SetTextureScale("_BaseMap",   new Vector2(x, y));
        if (m.HasProperty("_NormalMap")) m.SetTextureScale("_NormalMap", new Vector2(x, y));
    }

    bool IsInsideExcludeZone(Vector3 pos)
    {
        var c = centerTransform ? centerTransform.position : center;
        if (useCircleExclude) return Vector3.Distance(pos, c) < excludeRadius;
        if (useBoxExclude)    return Mathf.Abs(pos.x - c.x) < excludeBoxSize.x * 0.5f &&
                               Mathf.Abs(pos.z - c.z) < excludeBoxSize.y * 0.5f;
        return false;
    }

    Vector3 LatLonToUnity(float lat, float lon)
    {
        float x = (lon - baseLon) * scale;
        float z = (lat - baseLat) * scale;
        return new Vector3(x, 0, z) + manualOffset;
    }

    void MarkStatic(List<GameObject> gos, bool enable)
    {
#if UNITY_EDITOR
        var flags = enable
            ? (StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic)
            : 0;
        foreach (var go in gos) if (go) GameObjectUtility.SetStaticEditorFlags(go, flags);
#endif
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, .6f, .2f, .35f);
        var c = centerTransform ? centerTransform.position : center;
        if (useCircleExclude) Gizmos.DrawSphere(c, excludeRadius);
        if (useBoxExclude)    Gizmos.DrawCube(c, new Vector3(excludeBoxSize.x, .1f, excludeBoxSize.y));
    }
#endif
}

