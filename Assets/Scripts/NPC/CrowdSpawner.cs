using UnityEngine;
using UnityEngine.AI;

public class CrowdSpawner : MonoBehaviour
{
    [Header("관중 NPC 프리팹")]
    public GameObject[] crowdPrefab;

    [Header("스폰 포인트")]
    public Transform[] spawnPoints;

    [Header("각 캐릭터당 스폰 수")]
    public int minPerChar = 2;
    public int maxPerChar = 3;

    void Start()
    {

        SpawnCrowd();
    }

    private void SpawnCrowd()
    {
        int spawnIndex = 0;

        foreach (GameObject prefab in crowdPrefab)
        {
            // 랜덤 생성
            int count = Random.Range(minPerChar, maxPerChar + 1);

            for (int i = 0; i < count; i++)
            {
                // 순환 스폰
                Transform spawnPoint = spawnPoints[spawnIndex % spawnPoints.Length];

                GameObject npc = Instantiate(
                    prefab,
                    spawnPoint.position,
                    spawnPoint.rotation
                );

                spawnIndex++;
            }
        }
    }
}
