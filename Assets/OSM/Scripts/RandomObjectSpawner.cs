using System.Collections.Generic;
using UnityEngine;

public class RandomObjectSpawner : MonoBehaviour
{
    public List<GameObject> prefabs;
    public GameObject groundObject;
    public int objectCount = 50;
    public float areaSize = 100f;
    public float minDistance = 5f;
    public Vector2 groundCenter = new Vector2(40f, 40f);
    public float avoidDistanceFromCenter = 10f;

    void Start()
    {
        if (groundObject == null || prefabs.Count == 0) return;

        for (int i = 0; i < objectCount; i++)
        {
            Vector3 randomPosition;
            int attempts = 0;

            do
            {
                float x = Random.Range(0f, areaSize);
                float z = Random.Range(0f, areaSize);
                randomPosition = new Vector3(x, 0, z);
                attempts++;

                float distFromCenter = Vector2.Distance(new Vector2(x, z), groundCenter);

                // 중심 영역 피하기
                if (distFromCenter > avoidDistanceFromCenter)
                    break;

            } while (attempts < 100);

            GameObject prefab = prefabs[Random.Range(0, prefabs.Count)];
            Vector3 spawnPos = groundObject.transform.position + randomPosition;
            Instantiate(prefab, spawnPos, Quaternion.identity);
        }
    }
}
