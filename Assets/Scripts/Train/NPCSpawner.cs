using UnityEngine;
using System.Collections.Generic;

public class NPCSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] npcPrefabs; // 2명
    [SerializeField] private Transform[] spawnPoints; // 빈 오브젝트 4개

    private void Start()
    {
        SpawnNPCs();
    }

    private void SpawnNPCs()
    {
        // 위치를 랜덤하게 섞기
        List<Transform> shuffledPoints = new List<Transform>(spawnPoints);
        Shuffle(shuffledPoints);

        for (int i = 0; i < npcPrefabs.Length && i < shuffledPoints.Count; i++)
        {
            Instantiate(npcPrefabs[i], shuffledPoints[i].position, shuffledPoints[i].rotation);
        }
    }

    // Fisher–Yates 셔플
    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}