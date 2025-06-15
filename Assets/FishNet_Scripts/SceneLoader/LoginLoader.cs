using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Server;
using FishNet.Managing.Scened;
using UnityEngine;
using FishNet.Transporting;
using System;
using FishNet.Object;
using UnityEngine.SceneManagement;

public class LoginSceneLoader : MonoBehaviour
{
    private NetworkConnection _pendingConn;
    private bool _hasLoaded = false;

    private void Awake()
    {
        // 서버는 절대 이 코드 실행 안 함
        if (Application.isBatchMode)
        {
            Destroy(this);
            return;
        }

        // 클라이언트 연결 콜백 등록
        InstanceFinder.ServerManager.OnRemoteConnectionState += OnRemoteClientConnected;
        // InstanceFinder.ClientManager.OnClientConnectionState += HandleClientConnected;
    }



    private void OnRemoteClientConnected(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            _pendingConn = conn;
            Debug.Log($"[서버] 클라이언트 {conn.ClientId} 접속됨 → 개인 StationScene 로딩 시작");

            if (conn.FirstObject != null)
            {
                conn.FirstObject.Despawn();
            }
            SceneLoadData sld = new SceneLoadData("StartScene");
            sld.Options.AllowStacking = true;
            sld.ReplaceScenes = ReplaceOption.OnlineOnly;

            // InstanceFinder.SceneManager.OnLoadEnd += OnSceneLoadEnd;
            NetworkConnection[] connections = new NetworkConnection[] { conn };

        InstanceFinder.SceneManager.LoadConnectionScenes(connections, sld);
        }
    }

    // private void OnRemoteClientConnected(NetworkConnection conn, RemoteConnectionStateArgs args)
    // {
    //     if (args.ConnectionState == RemoteConnectionState.Started)
    //     {
    //         _pendingConn = conn;
    //         Debug.Log($"[서버] 클라이언트 {conn.ClientId} 접속됨 → 개인 StationScene 로딩 시작");
    //         NetworkConnection[] connections = new NetworkConnection[] { conn };
    //     }
    //     if (SceneManager.GetActiveScene().name != "StartScene")
    //         {
    //             SceneManager.LoadScene("StartScene", LoadSceneMode.Additive);
    //         }
    // }

    // private void HandleClientConnected(ClientConnectionStateArgs args)
    // {
    //     if (_hasLoaded) return;

    //     if (args.ConnectionState == LocalConnectionState.Started)
    //     {
    //         _hasLoaded = true;

    //         Debug.Log("[클라이언트 연결됨] → 로그인 씬 로드 시작");

    //         if (SceneManager.GetActiveScene().name != "StartScene")
    //         {
    //             SceneManager.LoadScene("StartScene", LoadSceneMode.Additive);
    //         }
    //     }
    // }


    // private void OnSceneLoadEnd(SceneLoadEndEventArgs args)
    // {
    //     if (!InstanceFinder.IsServerStarted)
    //     return;

    //     InstanceFinder.SceneManager.OnLoadEnd -= OnSceneLoadEnd;
        
    //     Scene mystationScene = default;
    //     foreach (var loadedScene in args.LoadedScenes)
    //     {
    //         if (loadedScene.name == "StartScene")
    //         {
    //             mystationScene = loadedScene;
    //             break;
    //         }
    //     }
    //     if (!mystationScene.IsValid())
    //     {
    //         Debug.LogError("Mystation 씬을 찾을 수 없습니다!");
    //         return;
    //     }

    //     InstanceFinder.SceneManager.AddConnectionToScene(_pendingConn, mystationScene);
        
    //     // // 6. 캐릭터 스폰
    //     // Vector3 spawnPos = new Vector3(0, 1, 0);
    //     // Quaternion spawnRot = Quaternion.identity;

    //     // GameObject playerObj = Instantiate(playerPrefab, spawnPos, spawnRot);

    //     // UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(playerObj, mystationScene);
    //     // NetworkObject nob = playerObj.GetComponent<NetworkObject>();

    //     // // 7. 네트워크에 소유자 지정 스폰
    //     // InstanceFinder.ServerManager.Spawn(nob, _pendingConn);
    // }

    private void OnDestroy()
    {
        if (InstanceFinder.ServerManager != null)
            InstanceFinder.ServerManager.OnRemoteConnectionState -= OnRemoteClientConnected;
    }
}
