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

    [Header("Line Materials")]
    public Material roadMaterial;
    public Material riverMaterial;

    [Header("Line Widths")]
    public float roadWidth = 3f;
    public float riverWidth = 5f;

    public string jsonFileName = "CheongsongOsm.json";

    private Dictionary<long, Vector3> nodeLookup = new Dictionary<long, Vector3>();

    void Start()
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

        // 모든 노드 저장
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

                    GameObject go = null;

                    if (tags.HasKey("natural") && tags["natural"] == "tree")
                        go = Instantiate(treePrefab, position, Quaternion.identity);
                    else if (tags.HasKey("building"))
                        go = Instantiate(buildingPrefab, position, Quaternion.identity);
                    else if (tags.HasKey("shop"))
                        go = Instantiate(shopPrefab, position, Quaternion.identity);
                    else if (tags.HasKey("amenity"))
                    {
                        string amenity = tags["amenity"];
                        if (amenity == "school" || amenity == "college" || amenity == "university")
                            go = Instantiate(schoolPrefab, position, Quaternion.identity);
                        else if (amenity == "hospital" || amenity == "clinic")
                            go = Instantiate(hospitalPrefab, position, Quaternion.identity);
                        else if (amenity == "police")
                            go = Instantiate(policePrefab, position, Quaternion.identity);
                        else if (amenity == "bus_station" || amenity == "bus_stop")
                            go = Instantiate(busStopPrefab, position, Quaternion.identity);
                        else if (amenity == "place_of_worship" || amenity == "statue")
                            go = Instantiate(statuePrefab, position, Quaternion.identity);
                        else
                            go = Instantiate(buildingPrefab, position, Quaternion.identity);
                    }
                    else if (tags.HasKey("man_made"))
                    {
                        string type = tags["man_made"];
                        if (type == "statue" || type == "flower_pot")
                            go = Instantiate(statuePrefab, position, Quaternion.identity);
                    }

                    // 태그 설정
                    if (go != null)
                        go.tag = "GeneratedFromOSM";                      
                }
            }
        }

        // 도로, 하천 등 way 처리
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
                        p.y = 0f;
                        positions.Add(p);
                    }
                }

                if (positions.Count < 2) continue;

                if (tags.HasKey("highway") && roadMaterial != null)
                {
                    DrawLine(positions, roadMaterial, roadWidth, "Road");
                }
                else if ((tags.HasKey("waterway") || tags["natural"] == "water") && riverMaterial != null)
                {
                    DrawLine(positions, riverMaterial, riverWidth, "River");
                }
            }
        }
    }

    void DrawLine(List<Vector3> points, Material mat, float width, string name)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.tag = "GeneratedFromOSM"; // ← 태그 지정
        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        line.positionCount = points.Count;

        for (int i = 0; i < points.Count; i++)
            points[i] = new Vector3(points[i].x, 0f, points[i].z);

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
        float baseLat = 36.4325726f;
        float baseLon = 129.0541124f;
        float scale = 300000f;

        float x = (lon - baseLon) * scale;
        float z = (lat - baseLat) * scale;

        return new Vector3(x, 0, z);
    }
}
