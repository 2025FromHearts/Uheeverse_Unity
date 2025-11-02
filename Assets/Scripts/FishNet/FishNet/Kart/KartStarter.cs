using UnityEngine;

public class KartStarter : MonoBehaviour
{
    public KartSpawner ks;

    void Awake() 
    {

        Debug.Log("카트 스타터 시작");
        ks = KartSpawner.Instance;

        ks.SpawnPlayers();
    }
}
