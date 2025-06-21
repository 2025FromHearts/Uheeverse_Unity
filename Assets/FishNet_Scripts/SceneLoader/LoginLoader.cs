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
        Debug.Log("로그인로더 실행");

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


            NetworkObject myPlayerObj = conn.FirstObject;


            SceneLoadData sld = new SceneLoadData("StartScene");
            sld.Options.AllowStacking = true;
            sld.ReplaceScenes = ReplaceOption.OnlineOnly;

            if (myPlayerObj != null)
                sld.MovedNetworkObjects = new NetworkObject[] { myPlayerObj };

            // InstanceFinder.SceneManager.OnLoadEnd += OnSceneLoadEnd;
            NetworkConnection[] connections = new NetworkConnection[] { conn };

            InstanceFinder.SceneManager.OnLoadEnd += OnSceneLoadEnd;

            InstanceFinder.SceneManager.LoadConnectionScenes(connections, sld);
        }
    }

    private void OnSceneLoadEnd(SceneLoadEndEventArgs args)
    {
        if (!InstanceFinder.IsServerStarted)
            return;

        InstanceFinder.SceneManager.OnLoadEnd -= OnSceneLoadEnd;
        foreach (var scene in args.LoadedScenes)
        {
            Debug.Log($"[서버] 클라 씬 로딩 완료: {scene.name}");
        }
    }


    private void OnDestroy()
    {
        if (InstanceFinder.ServerManager != null)
            InstanceFinder.ServerManager.OnRemoteConnectionState -= OnRemoteClientConnected;
    }
}

