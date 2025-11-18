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
            Vector3 spawnPos = new Vector3(7.8f, 15f, -45f);
            //NetworkObject obj = NetworkManager.GetPooledInstantiated(playerPrefab, asServer: true);
            NetworkObject obj = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            Spawn(obj, connection, gameObject.scene);
            Debug.Log("첫번째 클라이언트 스폰");
            //kmg = KartGameManager.Instance;
            // kmg.serverKartDisable();
            //kmg.Client_add(connection);
            //Debug.Log("클라이언트 추가 실행");
        }
        else if (KartList.Count == 1)
        {
            Debug.Log($"{connection.ClientId}");
            Vector3 spawnPos = new Vector3(3f, 15f, -45f);
            //NetworkObject obj = NetworkManager.GetPooledInstantiated(playerPrefab, asServer: true);
            NetworkObject obj = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            Spawn(obj, connection, gameObject.scene);
            Debug.Log($"Spawn 완료 - OwnerId: {obj.OwnerId}, ClientId: {connection.ClientId}");
            Debug.Log("두번째 클라이언트 스폰");

            //kmg = KartGameManager.Instance;
            // kmg.serverKartDisable();
            //kmg.Client_add(connection);
            //Debug.Log("클라이언트 추가 실행");
        }
    }
}