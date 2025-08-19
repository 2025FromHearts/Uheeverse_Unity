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
    public Vector3 center = Vector3.zero;

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

    [Header("Road Prefab Mode")]
    public bool useRoadPrefab = true;
    public GameObject roadPrefab;
    public float roadSegmentLength = 4f;

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
                    if (IsInsideExcludeZone(position)) continue;

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
                        go.tag = "GeneratedFromOSM";
                    }
                }
            }
        }

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
                        p.y = 0.1f;
                        positions.Add(p);
                    }
                }

                if (positions.Count < 2) continue;

                if (tags.HasKey("highway"))
                {
                    Debug.Log($"[도로] {tags["highway"]} 노드 {positions.Count}개");
                    if (useRoadPrefab && roadPrefab != null)
                    {
                        PlaceRoadPrefab(positions);
                    }
                    else if (roadMaterial != null)
                    {
                        DrawLine(positions, roadMaterial, roadWidth, "Road", "GeneratedLine");
                    }
                }
                else if ((tags.HasKey("waterway") && tags["waterway"] == "river") ||
                         (tags.HasKey("natural") && tags["natural"] == "water"))
                {
                    if (riverMaterial != null)
                    {
                        Debug.Log($"[강] waterway/river 노드 {positions.Count}개");
                        DrawLine(positions, riverMaterial, riverWidth, "River", "GeneratedLine");
                    }
                }
            }
        }
    }

    void PlaceRoadPrefab(List<Vector3> path)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 start = path[i];
            Vector3 end = path[i + 1];
            Vector3 dir = end - start;
            float dist = dir.magnitude;
            Quaternion rot = Quaternion.LookRotation(dir.normalized);
            int segmentCount = Mathf.CeilToInt(dist / roadSegmentLength);
            Vector3 step = dir / segmentCount;

            for (int j = 0; j < segmentCount; j++)
            {
                Vector3 pos = start + step * j;
                GameObject road = Instantiate(roadPrefab, pos, rot, this.transform);
                road.tag = "GeneratedFromOSM";
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
        lineObj.tag = tag;
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
