using System.Collections.Generic;
using UnityEngine;

public class RandomObjectSpawner : MonoBehaviour
{
    [Header("Prefabs & Area Settings")]
    public List<GameObject> prefabs;      // 랜덤 생성할 프리팹들
    public GameObject groundObject;       // 중심 ground plane
    public int objectCount = 50;          // 생성할 오브젝트 수
    public float areaSize = 100f;         // 생성할 전체 구역 범위
    public float minDistance = 5f;        // OSM 객체와 최소 거리
    public float extraMargin = 20f;       // plane 바깥으로 추가로 못 들어오게 할 margin

    [Header("Prefab Scale Settings")]
    public float randomScaleMin = 0.8f;   // 최소 스케일
    public float randomScaleMax = 1.5f;   // 최대 스케일

    private Bounds groundBounds;          // ground의 Bounds (MeshRenderer 기준)

    void Start()
    {
        if (groundObject == null || prefabs.Count == 0) return;

        // ground mesh의 bounds 가져와서 margin만큼 확장
        groundBounds = groundObject.GetComponent<MeshRenderer>().bounds;
        groundBounds.Expand(extraMargin * 2f);
        Debug.Log($"[Ground Bounds] center={groundBounds.center}, size={groundBounds.size}");

        for (int i = 0; i < objectCount; i++)
        {
            Vector3 randomPosition = Vector3.zero;
            bool found = false;
            int attempts = 0;

            while (attempts < 200)
            {
                float x = Random.Range(0f, areaSize);
                float z = Random.Range(0f, areaSize);
                randomPosition = new Vector3(x, 0, z);

                if (!IsInsideGround(randomPosition) && IsFarEnoughFromExistingObjects(randomPosition))
                {
                    found = true;
                    break;
                }
                attempts++;
            }

            if (!found)
            {
                // 실패하면 ground 밖으로 밀어냄
                float x = Random.Range(0f, areaSize);
                float z = Random.Range(0f, areaSize);
                Vector3 dir = (new Vector3(x, 0, z) - groundBounds.center).normalized;
                randomPosition = groundBounds.center + dir * (groundBounds.extents.magnitude + extraMargin);
            }

            // 랜덤 prefab 선택
            GameObject prefab = prefabs[Random.Range(0, prefabs.Count)];
            GameObject go = Instantiate(prefab, randomPosition, Quaternion.identity);

            // 랜덤 scale 적용
            float randomScale = Random.Range(randomScaleMin, randomScaleMax);
            go.transform.localScale *= randomScale;

            Debug.Log($"[Spawn] {prefab.name} at {randomPosition}, scale={randomScale}");
        }
    }

    bool IsInsideGround(Vector3 pos)
    {
        return groundBounds.Contains(pos);
    }

    bool IsFarEnoughFromExistingObjects(Vector3 position)
    {
        GameObject[] existing = GameObject.FindGameObjectsWithTag("GeneratedFromOSM");

        foreach (GameObject obj in existing)
        {
            float dist = Vector3.Distance(new Vector3(position.x, 0, position.z),
                                          new Vector3(obj.transform.position.x, 0, obj.transform.position.z));
            if (dist < minDistance)
                return false;
        }
        return true;
    }
}
