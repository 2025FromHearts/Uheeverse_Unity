using System.Collections.Generic;
using UnityEngine;

public class ScenerySpawner : MonoBehaviour
{
    [Header("Scenery Prefabs")]
    public List<GameObject> sceneryPrefabs;

    [Header("Spawn Settings")]
    public int sceneryCount = 100;
    public float spawnRangeX = 100f;
    public float spawnRangeZ = 100f;

    [Header("Exclude Zone")]
    public float excludeMinX = -10f;
    public float excludeMaxX = 10f;
    public float excludeMinZ = -50f;
    public float excludeMaxZ = 50f;

    [Header("Train Reference")]
    public Transform trainTransform;
    public float removeDistance = 10f;

    void Start()
    {
        int spawned = 0;
        int maxTries = sceneryCount * 10;

        for (int i = 0; i < maxTries && spawned < sceneryCount; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(-spawnRangeX, spawnRangeX),
                Random.Range(-1.5f, -0.05f), // ✅ 살짝 떠있거나 묻히도록 Y 위치 랜덤 지정
                Random.Range(-spawnRangeZ, spawnRangeZ)
            );

            // ❌ 특정 구역 제외
            if (pos.x > excludeMinX && pos.x < excludeMaxX &&
                pos.z > excludeMinZ && pos.z < excludeMaxZ)
            {
                continue;
            }

            GameObject prefab = sceneryPrefabs[Random.Range(0, sceneryPrefabs.Count)];
            GameObject spawnedObj = Instantiate(prefab, pos, Quaternion.identity, this.transform);
            spawnedObj.transform.localScale *= 0.2f;

            var remover = spawnedObj.AddComponent<AutoRemoveNearTrain>();
            remover.trainTransform = trainTransform;
            remover.removeDistance = removeDistance;

            spawned++;
        }

        Debug.Log($"[ScenerySpawner] 생성 완료: {spawned}/{sceneryCount}개");
    }
}
