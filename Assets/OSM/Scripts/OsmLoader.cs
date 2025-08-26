using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SimpleJSON;

public class OsmLoader : MonoBehaviour
{
    [Header("Prefabs (OSM 포인트 오브젝트들)")]
    public GameObject treePrefab;
    public GameObject buildingPrefab;
    public GameObject shopPrefab;
    public GameObject schoolPrefab;
    public GameObject policePrefab;
    public GameObject hospitalPrefab;
    public GameObject parkPrefab;
    public GameObject busStopPrefab;
    public GameObject statuePrefab;

    [Header("Prefab Scales")]
    public float treeScale = 1f;
    public float buildingScale = 1f;
    public float shopScale = 1f;
    public float schoolScale = 1f;
    public float policeScale = 1f;
    public float hospitalScale = 1f;
    public float parkScale = 1f;
    public float busStopScale = 1f;
    public float statueScale = 1f;

    [Header("Line Materials (LineRenderer 전용)")]
    public Material roadMaterial;
    public Material riverMaterial;

    [Header("Line Widths")]
    public float roadWidth = 3f;
    public float riverWidth = 5f;

    [Header("Line Tiling (길이 기반 자동 반복)")]
    [Tooltip("텍스처 1타일이 실제 몇 미터 길이로 보일지 (1.5~3 권장)")]
    public float metersPerTile = 2f;
    [Tooltip("코너/끝 부분 버텍스(부드러움)")]
    public int cornerVertices = 2;
    public int capVertices = 2;

    [Header("Exclude Zone Center")]
    public Transform centerTransform;
    public Vector3 center = Vector3.zero;

    [Header("Exclude Settings")]
    public bool useCircleExclude = false;
    public float excludeRadius = 50f;
    public bool useBoxExclude = true;
    public Vector2 excludeBoxSize = new Vector2(220, 220);

    [Header("LatLon Mapping")]
    public float baseLat = 36.4325726f;
    public float baseLon = 129.0541124f;
    [Tooltip("경위도 → Unity 미터 좌표 변환 스케일")]
    public float scale = 300000f;
    public Vector3 manualOffset = Vector3.zero;

    [Header("OSM")]
    [Tooltip("StreamingAssets 폴더에 있는 OSM JSON 파일명")]
    public string jsonFileName = "CheongsongOsm.json";

    private readonly Dictionary<long, Vector3> nodeLookup = new Dictionary<long, Vector3>();

    void Start()
    {
        LoadOSMData();
        CleanupCenterArea();
    }

    void LoadOSMData()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonFileName);
        if (!File.Exists(filePath))
        {
            Debug.LogError("OSM JSON 파일을 찾을 수 없습니다: " + filePath);
            return;
        }

        string jsonText = File.ReadAllText(filePath);
        var osmData = JSON.Parse(jsonText);
        if (osmData == null || osmData["elements"] == null)
        {
            Debug.LogError("OSM JSON 파싱 실패 또는 elements 없음");
            return;
        }

        Debug.Log("OSM 요소 개수: " + osmData["elements"].Count);

        // 1) node 수집 + 태그가 있는 포인트 프리팹 배치
        foreach (JSONNode element in osmData["elements"].AsArray)
        {
            if (element["type"] == "node")
            {
                long id = element["id"].AsLong;
                float lat = element["lat"];
                float lon = element["lon"];
                Vector3 position = LatLonToUnity(lat, lon);
                nodeLookup[id] = position;

                if (!element.HasKey("tags")) continue;
                if (IsInsideExcludeZone(position)) continue;

                var tags = element["tags"];
                GameObject go = null;
                float scaleFactor = 1f;

                if (tags.HasKey("natural") && tags["natural"] == "tree" && treePrefab != null)
                {
                    go = Instantiate(treePrefab, position, Quaternion.identity);
                    scaleFactor = treeScale;
                }
                else if (tags.HasKey("building") && buildingPrefab != null)
                {
                    go = Instantiate(buildingPrefab, position, Quaternion.identity);
                    scaleFactor = buildingScale;
                }
                else if (tags.HasKey("shop") && shopPrefab != null)
                {
                    go = Instantiate(shopPrefab, position, Quaternion.identity);
                    scaleFactor = shopScale;
                }
                else if (tags.HasKey("amenity"))
                {
                    string amenity = tags["amenity"];
                    if ((amenity == "school" || amenity == "college" || amenity == "university") && schoolPrefab != null)
                    {
                        go = Instantiate(schoolPrefab, position, Quaternion.identity);
                        scaleFactor = schoolScale;
                    }
                    else if ((amenity == "hospital" || amenity == "clinic") && hospitalPrefab != null)
                    {
                        go = Instantiate(hospitalPrefab, position, Quaternion.identity);
                        scaleFactor = hospitalScale;
                    }
                    else if (amenity == "police" && policePrefab != null)
                    {
                        go = Instantiate(policePrefab, position, Quaternion.identity);
                        scaleFactor = policeScale;
                    }
                    else if ((amenity == "bus_station" || amenity == "bus_stop") && busStopPrefab != null)
                    {
                        go = Instantiate(busStopPrefab, position, Quaternion.identity);
                        scaleFactor = busStopScale;
                    }
                    else if (buildingPrefab != null)
                    {
                        go = Instantiate(buildingPrefab, position, Quaternion.identity);
                        scaleFactor = buildingScale;
                    }
                }

                if (go != null)
                {
                    go.transform.localScale *= scaleFactor;
                    go.tag = "GeneratedFromOSM";
                }
            }
        }

        // 2) way(도로/수로) → LineRenderer로 그리기
        foreach (JSONNode element in osmData["elements"].AsArray)
        {
            if (element["type"] != "way" || !element.HasKey("tags")) continue;

            var tags = element["tags"];
            JSONArray nodeRefs = element["nodes"].AsArray;

            List<Vector3> positions = new List<Vector3>(nodeRefs.Count);
            foreach (JSONNode nodeId in nodeRefs)
            {
                long id = nodeId.AsLong;
                if (nodeLookup.TryGetValue(id, out var p))
                {
                    p.y = 0.1f; // z-fighting 방지
                    positions.Add(p);
                }
            }
            if (positions.Count < 2) continue;

            if (tags.HasKey("highway"))
            {
                if (roadMaterial != null)
                    DrawLine(positions, roadMaterial, roadWidth, "Road", "GeneratedLine");
            }
            else if ((tags.HasKey("waterway") && tags["waterway"] == "river") ||
                     (tags.HasKey("natural") && tags["natural"] == "water"))
            {
                if (riverMaterial != null)
                    DrawLine(positions, riverMaterial, riverWidth, "River", "GeneratedLine");
            }
        }
    }

    void CleanupCenterArea()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("GeneratedFromOSM"))
        {
            if (IsInsideExcludeZone(obj.transform.position))
                Destroy(obj);
        }
    }

    bool IsInsideExcludeZone(Vector3 pos)
    {
        Vector3 c = GetActualCenter();
        if (useCircleExclude)
            return Vector3.Distance(pos, c) < excludeRadius;
        if (useBoxExclude)
            return Mathf.Abs(pos.x - c.x) < excludeBoxSize.x * 0.5f &&
                   Mathf.Abs(pos.z - c.z) < excludeBoxSize.y * 0.5f;
        return false;
    }

    Vector3 GetActualCenter()
    {
        return centerTransform ? centerTransform.position : center;
    }

    // -------- LineRenderer: 타일 반복 + 자동 타일링 --------
    void DrawLine(List<Vector3> points, Material baseMat, float width, string name, string tag)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.tag = tag;

        var line = lineObj.AddComponent<LineRenderer>();
        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
        line.startWidth = width;
        line.endWidth = width;
        line.useWorldSpace = true;

        // 시각 품질
        line.numCornerVertices = Mathf.Clamp(cornerVertices, 0, 8);
        line.numCapVertices = Mathf.Clamp(capVertices, 0, 8);
        line.alignment = LineAlignment.View;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;

        // 핵심: 길이 방향으로 타일 반복
        line.textureMode = LineTextureMode.Tile;

        // 라인마다 개별 머티리얼 인스턴스(반복 수가 라인별로 다름)
        Material matInst = Instantiate(baseMat);
        line.material = matInst;

        // 라인 총 길이 → 반복 수 자동 계산
        float length = 0f;
        for (int i = 0; i < points.Count - 1; i++)
            length += Vector3.Distance(points[i], points[i + 1]);

        float tileMeters = Mathf.Max(0.01f, metersPerTile);
        float repeatX = Mathf.Max(1f, length / tileMeters);

        SetTextureTiling(matInst, repeatX, 1f);
    }

    // 파이프라인(빌트인/URP) 모두 대응해 타일링 적용
    void SetTextureTiling(Material m, float x, float y)
    {
        if (m.HasProperty("_MainTex"))   m.SetTextureScale("_MainTex",   new Vector2(x, y));
        if (m.HasProperty("_BumpMap"))   m.SetTextureScale("_BumpMap",   new Vector2(x, y));
        if (m.HasProperty("_BaseMap"))   m.SetTextureScale("_BaseMap",   new Vector2(x, y));
        if (m.HasProperty("_NormalMap")) m.SetTextureScale("_NormalMap", new Vector2(x, y));
    }

    // 경위도 → Unity 좌표(평면 근사)
    Vector3 LatLonToUnity(float lat, float lon)
    {
        float x = (lon - baseLon) * scale;
        float z = (lat - baseLat) * scale;
        return new Vector3(x, 0, z) + manualOffset;
    }
}

