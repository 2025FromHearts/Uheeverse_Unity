using FishNet.Object;
using FishNet.Connection;
using FishNet.Managing.Scened;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using System;
using Unity.VisualScripting;
using FishNet.Transporting;

public enum SessionType
{
    Login,
    Station,
    Game
    // 필요하면 Train, Plaza 등도 추가 가능
}
public class SessionData
{
    public string SessionId;
    public List<NetworkConnection> Members = new List<NetworkConnection>();
    public SessionType Type;
}

public class SessionManager : NetworkBehaviour
{
    private NetworkConnection _pendingConn;
    
    public GameObject playerPrefab;
    public static SessionManager Instance { get; private set; }
    public object PreferredActiveScene { get; private set; }
    public ReplaceOption ReplaceScenes { get; private set; }

    private Dictionary<string, SessionData> sessions = new Dictionary<string, SessionData>();
    private Dictionary<NetworkConnection, NetworkObject> playerObjects = new Dictionary<NetworkConnection, NetworkObject>();

    void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            Debug.Log("인스턴스화 됐긔");
        }
        else
        {
            Destroy(gameObject);
        }
        InstanceFinder.ServerManager.OnRemoteConnectionState += OnRemoteClientConnected;
    }

    private void OnRemoteClientConnected(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            _pendingConn = conn;
            Debug.Log($"[서버] 클라이언트 {conn.ClientId} 접속됨 → 개인 StationScene 로딩 시작");
            NetworkConnection[] connections = new NetworkConnection[] { conn };

        }
        if (IsServer)
        {
            Debug.Log("서버다");
        }
    }

    public void CreateSession(NetworkConnection hostConn, SessionType type)
    {

        if (playerObjects.TryGetValue(hostConn, out var oldPlayer) && oldPlayer != null && oldPlayer.IsSpawned)
        {
            oldPlayer.Despawn();
            playerObjects.Remove(hostConn);
        }

        string sessionId = System.Guid.NewGuid().ToString();

        var session = new SessionData
        {
            SessionId = sessionId,
            Members = new List<NetworkConnection> { hostConn },
            Type = type
        };
        session.Members.Add(hostConn);
        sessions.Add(sessionId, session);

        // 타입에 따라 씬 이름 결정
        // string sceneName = type switch
        // {
        //     SessionType.Login => "MyStaion", // 로비 세션이면 Train(열차) 씬
        //     SessionType.Station => "GameScene", // 게임 세션이면 Game 씬
        //     _ => "MyStation"
        // };
        NetworkConnection[] connections = new NetworkConnection[] { hostConn };
        string sceneName;
        switch (type)
        {
            case SessionType.Login:
                sceneName = "MyStation";
                break;
            case SessionType.Station:
                sceneName = "Train";
                break;
            default:
                sceneName = "MyStation";
                break;
        }

        SceneLoadData sld = new SceneLoadData(sceneName); // (필요하면)
        sld.Options.AllowStacking = true;
        sld.Options.AutomaticallyUnload = true;
        sld.ReplaceScenes = ReplaceOption.None;

        SceneManager.OnLoadEnd += OnSceneLoadEnd;
        SceneManager.LoadConnectionScenes(connections, sld);

        if (sceneName == "MyStation")
        { 
            SceneUnloadData sud = new SceneUnloadData(new string[] { "StartScene" });
            SceneManager.UnloadConnectionScenes(connections, sud);
        }
        else if(sceneName == "Train") { 
            SceneUnloadData sud = new SceneUnloadData(new string[] { "MyStation", "StartScene" });
            SceneManager.UnloadConnectionScenes(connections, sud);
        }
        




        Debug.Log($"[세션] {type} 세션 생성 & 씬 이동: {sessionId}, {hostConn.ClientId}");
    }

    private void OnSceneLoadEnd(SceneLoadEndEventArgs args)
    {
        SceneManager.OnLoadEnd -= OnSceneLoadEnd;

        // foreach (var conn in args.LoadConnectionScenes)
        // {
        //     Vector3 spawnPos = new Vector3(0, 1, 0);
        //     Quaternion spawnRot = Quaternion.identity;

        //     GameObject playerObj = Instantiate(playerPrefab, spawnPos, spawnRot);
        //     var nob = playerObj.GetComponent<NetworkObject>();
        //     nob.SpawnAsPlayerObject(conn);

        //     playerObjects[conn] = nob;
        // }
    }

    [ServerRpc(RequireOwnership = false)]
    internal void CreateSessionFromTagServerRpc(SessionType type, NetworkConnection conn = null)
    {
        if (conn == null) conn = base.Owner;
        CreateSession(base.Owner, type);
        Debug.Log($"[ServerRpc] CreateSessionFromTagServerRpc 실행됨 - IsServer: {IsServer}, IsOwner: {IsOwner}");

        // 서버라면 세션 생성

        // Owner 대신 네트워크 연결자 사용
        // CreateSession(base.Owner, type);

        // throw new NotImplementedException();
    }
    
    
}




