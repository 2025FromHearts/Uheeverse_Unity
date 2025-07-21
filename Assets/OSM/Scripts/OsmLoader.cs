using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SimpleJSON;

public class OsmLoader : MonoBehaviour
{
    [Header("Prefabs")]
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

    [Header("Line Materials")]
    public Material roadMaterial;
    public Material riverMaterial;

    [Header("Line Widths")]
    public float roadWidth = 3f;
    public float riverWidth = 5f;

    [Header("Exclude Zone Center")]
    public Transform centerTransform;
    public Vector3 center = new Vector3(0, 0, 0);

    [Header("Exclude Settings")]
    public bool useCircleExclude = false;
    public float excludeRadius = 50f;
    public bool useBoxExclude = true;
    public Vector2 excludeBoxSize = new Vector2(220, 220);

    [Header("LatLon Mapping")]
    public float baseLat = 36.4325726f;
    public float baseLon = 129.0541124f;
    public float scale = 300000f;
    public Vector3 manualOffset = Vector3.zero;

    public string jsonFileName = "CheongsongOsm.json";

    private Dictionary<long, Vector3> nodeLookup = new Dictionary<long, Vector3>();

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
        Debug.Log("OSM 요소 개수: " + osmData["elements"].Count);

        // NODE 처리
        foreach (JSONNode element in osmData["elements"].AsArray)
        {
            if (element["type"] == "node")
            {
                long id = element["id"].AsLong;
                float lat = element["lat"];
                float lon = element["lon"];
                Vector3 position = LatLonToUnity(lat, lon);
                nodeLookup[id] = position;

                if (element.HasKey("tags"))
                {
                    var tags = element["tags"];
                    if (IsInsideExcludeZone(position))
                        continue;

                    GameObject go = null;
                    float scaleFactor = 1f;

                    if (tags.HasKey("natural") && tags["natural"] == "tree")
                    {
                        go = Instantiate(treePrefab, position, Quaternion.identity, null);
                        scaleFactor = treeScale;
                    }
                    else if (tags.HasKey("building"))
                    {
                        go = Instantiate(buildingPrefab, position, Quaternion.identity, null);
                        scaleFactor = buildingScale;
                    }
                    else if (tags.HasKey("shop"))
                    {
                        go = Instantiate(shopPrefab, position, Quaternion.identity, null);
                        scaleFactor = shopScale;
                    }
                    else if (tags.HasKey("amenity"))
                    {
                        string amenity = tags["amenity"];
                        if (amenity == "school" || amenity == "college" || amenity == "university")
                        {
                            go = Instantiate(schoolPrefab, position, Quaternion.identity, null);
                            scaleFactor = schoolScale;
                        }
                        else if (amenity == "hospital" || amenity == "clinic")
                        {
                            go = Instantiate(hospitalPrefab, position, Quaternion.identity, null);
                            scaleFactor = hospitalScale;
                        }
                        else if (amenity == "police")
                        {
                            go = Instantiate(policePrefab, position, Quaternion.identity, null);
                            scaleFactor = policeScale;
                        }
                        else if (amenity == "bus_station" || amenity == "bus_stop")
                        {
                            go = Instantiate(busStopPrefab, position, Quaternion.identity, null);
                            scaleFactor = busStopScale;
                        }
                        else
                        {
                            go = Instantiate(buildingPrefab, position, Quaternion.identity, null);
                            scaleFactor = buildingScale;
                        }
                    }

                    if (go != null)
                    {
                        go.transform.localScale *= scaleFactor;
                        go.tag = "GeneratedFromOSM"; // 일반 프리팹들은 이 태그
                    }
                }
            }
        }

        // WAY 처리 (LineRenderer)
        foreach (JSONNode element in osmData["elements"].AsArray)
        {
            if (element["type"] == "way" && element.HasKey("tags"))
            {
                var tags = element["tags"];
                JSONArray nodeRefs = element["nodes"].AsArray;

                List<Vector3> positions = new List<Vector3>();
                foreach (JSONNode nodeId in nodeRefs)
                {
                    long id = nodeId.AsLong;
                    if (nodeLookup.ContainsKey(id))
                    {
                        Vector3 p = nodeLookup[id];
                        p.y = 0.1f; // 살짝 띄워서 Z-fighting 방지
                        positions.Add(p);
                    }
                }

                if (positions.Count < 2) continue;

                if (tags.HasKey("highway") && roadMaterial != null)
                {
                    Debug.Log($"[로드] {tags["highway"]} 길 그리기, 노드 수: {positions.Count}");
                    DrawLine(positions, roadMaterial, roadWidth, "Road", "GeneratedLine");
                }
                else if ((tags.HasKey("waterway") && tags["waterway"] == "river") ||
                         (tags.HasKey("natural") && tags["natural"] == "water"))
                {
                    if (riverMaterial != null)
                    {
                        Debug.Log($"[강] waterway/river 그리기, 노드 수: {positions.Count}");
                        DrawLine(positions, riverMaterial, riverWidth, "River", "GeneratedLine");
                    }
                }
            }
        }
    }

    void CleanupCenterArea()
    {
        Vector3 centerPos = GetActualCenter();
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("GeneratedFromOSM"))
        {
            if (IsInsideExcludeZone(obj.transform.position))
            {
                Destroy(obj);
            }
        }
    }

    bool IsInsideExcludeZone(Vector3 pos)
    {
        Vector3 actualCenter = GetActualCenter();
        if (useCircleExclude)
            return Vector3.Distance(pos, actualCenter) < excludeRadius;
        else if (useBoxExclude)
            return Mathf.Abs(pos.x - actualCenter.x) < excludeBoxSize.x / 2 &&
                   Mathf.Abs(pos.z - actualCenter.z) < excludeBoxSize.y / 2;
        return false;
    }

    Vector3 GetActualCenter()
    {
        return centerTransform != null ? centerTransform.position : center;
    }

    void DrawLine(List<Vector3> points, Material mat, float width, string name, string tag)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.tag = tag; // LineRenderer는 다른 태그
        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
        line.startWidth = width;
        line.endWidth = width;
        line.material = mat;
        line.useWorldSpace = true;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
    }

    Vector3 LatLonToUnity(float lat, float lon)
    {
        float x = (lon - baseLon) * scale;
        float z = (lat - baseLat) * scale;
        return new Vector3(x, 0, z) + manualOffset;
    }
}
