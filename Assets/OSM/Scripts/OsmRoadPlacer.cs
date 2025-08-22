using System.Collections.Generic;
using UnityEngine;

public class OsmRoadPlacer : MonoBehaviour
{
    [Header("Road Prefab Settings")]
    public GameObject roadPrefab;          // 사용할 도로 프리팹
    public float segmentLength = 4f;       // 프리팹 길이

    public void PlaceRoadAlongPath(List<Vector3> path)
    {
        if (roadPrefab == null || path.Count < 2) return;

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 start = path[i];
            Vector3 end = path[i + 1];
            Vector3 dir = end - start;

            float totalDistance = dir.magnitude;
            Quaternion rotation = Quaternion.LookRotation(dir.normalized);

            int segmentCount = Mathf.CeilToInt(totalDistance / segmentLength);
            Vector3 step = dir / segmentCount;

            for (int j = 0; j < segmentCount; j++)
            {
                Vector3 pos = start + step * j;
                GameObject roadSegment = Instantiate(roadPrefab, pos, rotation, transform);
                roadSegment.tag = "GeneratedFromOSM";
            }
        }
    }
}
