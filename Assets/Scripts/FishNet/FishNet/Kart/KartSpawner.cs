using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using UnityEngine;

public class KartSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;

    public static KartSpawner Instance { get; private set; }

    public KartGameManager kmg;

    void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            Debug.Log("KartSpawner �ν��Ͻ�ȭ �Ϸ�");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    

    // The Server attribute here prevents this method from being called except on the server.
    [Server]
    public void SpawnPlayers()
    {
        if (playerPrefab == null)
        {
            Debug.LogWarning("Player prefab is not assigned and thus cannot be spawned.");
            return;
        }

        foreach (NetworkConnection client in ServerManager.Clients.Values)
        {
            // Since the ServerManager.Clients collection contains all clients (even non-authenticated ones),
            // we need to check if they are authenticated first before spawning a player object for them.
            if (!client.IsAuthenticated)
                continue;

            if (!client.Scenes.Contains(gameObject.scene))
                continue;

            // If the client isn't observing this scene, make him an observer of it.
            //if (!client.Scenes.Contains(gameObject.scene))
            //    SceneManager.AddConnectionToScene(client, gameObject.scene);

            NetworkObject obj = NetworkManager.GetPooledInstantiated(playerPrefab, asServer: true);
            Spawn(obj, client, gameObject.scene);

            // kmg.Client_add(connection);
            // Debug.Log("클라이언트 추가 실행");
        }
    }
}