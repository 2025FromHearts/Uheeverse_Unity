using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using UnityEngine;

public class ScenePlayerSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;

    public KartGameManager kgm;

    public GameObject KartPrefab;

    public KartGameManager kmg = KartGameManager.Instance;

    public List<int> KartList = KartGameManager.Instance.Kart_Client;

    // This method runs on the server when the client is about to spawn this object.
    // Since the player is about to spawn this object, we know he is in this scene.
    public override void OnSpawnServer(NetworkConnection connection)
    {
        //Debug.Log("OnSpawnServer ����");
        //Debug.Log($"오브젝트명: {gameObject.name}");\\

        if (KartList.Count == 0)
        {
            NetworkObject obj = NetworkManager.GetPooledInstantiated(playerPrefab, asServer: true);
            Spawn(obj, connection, gameObject.scene);
        }
        

        KartController kartController = FindAnyObjectByType<KartController>();
        kartController.enabled = false;

        kmg = KartGameManager.Instance;
        kmg.Client_add(connection.ClientId);
        Debug.Log("클라이언트 추가 실행");
    }
}