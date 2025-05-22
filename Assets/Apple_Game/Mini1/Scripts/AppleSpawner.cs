using UnityEngine;

public class AppleSpawner : MonoBehaviour
{
    public GameObject Apple;
    public GameObject Banana;
    public GameObject Bomb;

    public float spawnRangeX = 10f;
    public float spawnRangeZ = 10f;
    public float spawnHeight = 15f;

    public float minDelay = 0.5f;
    public float maxDelay = 2f;

    public float minFallSpeed = 10f;
    public float maxFallSpeed = 30f;

    [HideInInspector]
    public bool isGameOver = false; // 🔑 외부에서 접근할 게임 종료 상태

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    System.Collections.IEnumerator SpawnLoop()
    {
        while (true)
        {
            // 🔒 게임 종료 시 스폰 중지
            if (isGameOver)
                yield break;

            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);

            Vector3 spawnPos = new Vector3(
                Random.Range(-spawnRangeX, spawnRangeX),
                spawnHeight,
                Random.Range(-spawnRangeZ, spawnRangeZ)
            );

            GameObject obj;

            int chance = Random.Range(0, 100);
            if (chance < 20)
                obj = Instantiate(Banana, spawnPos, Quaternion.identity);
            else if (chance < 70)
                obj = Instantiate(Apple, spawnPos, Quaternion.identity);
            else
                obj = Instantiate(Bomb, spawnPos, Quaternion.identity);

            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                float fallSpeed = Random.Range(minFallSpeed, maxFallSpeed);
                rb.linearVelocity = new Vector3(0, -fallSpeed, 0);
            }
        }
    }
}
