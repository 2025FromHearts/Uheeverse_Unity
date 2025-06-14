using UnityEngine;
using UnityEngine.SceneManagement;
using FishNet;
using FishNet.Managing.Client;
using FishNet.Transporting;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Server;
using System;
using FishNet.Object;

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
            NetworkConnection[] connections = new NetworkConnection[] { conn };
        }
        if (SceneManager.GetActiveScene().name != "StartScene")
            {
                SceneManager.LoadScene("StartScene", LoadSceneMode.Additive);
            }
    }

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

    private void OnDestroy()
    {
        if (InstanceFinder.ServerManager != null)
            InstanceFinder.ServerManager.OnRemoteConnectionState -= OnRemoteClientConnected;
    }
}
