using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using UnityEngine;

public class ApplePlayerSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;

    public AppleGameManager agm;

    public GameObject KartPrefab;

    public AppleGameManager amg = AppleGameManager.Instance;

    public List<int> AppleList = AppleGameManager.Instance.Apple_Client;

    // This method runs on the server when the client is about to spawn this object.
    // Since the player is about to spawn this object, we know he is in this scene.
    public override void OnSpawnServer(NetworkConnection connection)
    {
        //Debug.Log("OnSpawnServer ����");
        //Debug.Log($"오브젝트명: {gameObject.name}");\\

        if (AppleList.Count == 0)
        {
            Vector3 spawnPos = new Vector3(7.8f, 15f, -45f);
            //NetworkObject obj = NetworkManager.GetPooledInstantiated(playerPrefab, asServer: true);
            NetworkObject obj = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            Spawn(obj, connection, gameObject.scene);
        }
        else if (AppleList.Count == 1)
        {
            Vector3 spawnPos = new Vector3(3f, 15f, -45f);
            //NetworkObject obj = NetworkManager.GetPooledInstantiated(playerPrefab, asServer: true);
            NetworkObject obj = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            Spawn(obj, connection, gameObject.scene);
        }

        // KartController kartController = FindAnyObjectByType<KartController>();
        // kartController.enabled = false;

        amg = AppleGameManager.Instance;
        Debug.Log("카트 비활성화 호출");
        // amg.rpcKartDisable();
        amg.Client_add(connection.ClientId);
        Debug.Log("클라이언트 추가 실행");
    }
}