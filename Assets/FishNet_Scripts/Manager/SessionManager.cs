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
            Debug.Log($"[서버] 클라이언트 {conn.ClientId} 접속됨 → 개인 StationScene 로딩 시작");

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
        string sceneName;
        switch (type)
        { 
            case SessionType.Login:
                sceneName = "MyStation";
                break;
            case SessionType.Station:
                sceneName = "GameScene";
                break;
            default:
                sceneName = "MyStation";
                break;
        }

        SceneLoadData sld = new SceneLoadData(sceneName); // (필요하면)
        sld.Options.AllowStacking = true;
        sld.Options.AutomaticallyUnload = true;
        sld.ReplaceScenes = ReplaceOption.OnlineOnly;

        NetworkConnection[] connections = new NetworkConnection[] { hostConn };
        SceneManager.OnLoadEnd += OnSceneLoadEnd;
        SceneManager.LoadConnectionScenes(connections, sld);



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
        CreateSession(conn, type);

        // 서버라면 세션 생성
       
        // Owner 대신 네트워크 연결자 사용
        // CreateSession(base.Owner, type);
        
        // throw new NotImplementedException();
    }
    
    
}




