using UnityEngine;

public class KartStarter : MonoBehaviour
{
    public KartSpawner ks;

    void Awake() 
    {

        Debug.Log("카트 스폰 시작");
        Debug.Log($"오브젝트명: {gameObject.name}");
        ks = KartSpawner.Instance;

        ks.SpawnPlayers();
    }
}
