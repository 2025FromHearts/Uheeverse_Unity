using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using UnityEngine;

public class ScenePlayerSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;

    // This method runs on the server when the client is about to spawn this object.
    // Since the player is about to spawn this object, we know he is in this scene.
    public override void OnSpawnServer(NetworkConnection connection)
    {
        Debug.Log("OnSpawnServer ¡¯¿‘");
        NetworkObject obj = NetworkManager.GetPooledInstantiated(playerPrefab, asServer: true);
        Spawn(obj, connection, gameObject.scene);
    }
}