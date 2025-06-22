// using FishNet.Object;
// using FishNet.Connection;
// using FishNet.Managing.Scened;
// using System.Collections.Generic;
// using UnityEngine;
// using FishNet;
// using System;
// using Unity.VisualScripting;
// using FishNet.Transporting;
// using UnityEngine.SceneManagement;
// using FishNet.Component.Prediction;

// public enum SessionType
// {
//     Login,
//     Station,
//     Game,
//     Train,
//     Festival
//     // 필요하면 Train, Plaza 등도 추가 가능
// }
// public class SessionData
// {
//     public string SessionId;
//     public List<NetworkConnection> Members = new List<NetworkConnection>();
//     public SessionType Type;
// }

// public class SessionManager : NetworkBehaviour
// {
//     private NetworkConnection _pendingConn;
    
//     public GameObject playerPrefab;
//     public static SessionManager Instance { get; private set; }
//     public object PreferredActiveScene { get; private set; }
//     public ReplaceOption ReplaceScenes { get; private set; }

//     private Dictionary<string, SessionData> sessions = new Dictionary<string, SessionData>();
//     private Dictionary<NetworkConnection, NetworkObject> playerObjects = new Dictionary<NetworkConnection, NetworkObject>();

//     void Awake()
//     {

//         if (Instance == null)
//         {
//             Instance = this;
//             Debug.Log("인스턴스화 됐긔");
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//         InstanceFinder.ServerManager.OnRemoteConnectionState += OnRemoteClientConnected;
//     }

//     private void OnRemoteClientConnected(NetworkConnection conn, RemoteConnectionStateArgs args)
//     {
//         if (args.ConnectionState == RemoteConnectionState.Started)
//         {
//             _pendingConn = conn;
//             Debug.Log($"[서버] 클라이언트 {conn.ClientId} 접속됨 → 개인 StationScene 로딩 시작");
//             NetworkConnection[] connections = new NetworkConnection[] { conn };

//         }
//         if (IsServer)
//         {
//             Debug.Log("서버다");
//         }
//     }

//     public void CreateSession(NetworkConnection hostConn, SessionType type)
//     {

//         if (playerObjects.TryGetValue(hostConn, out var oldPlayer) && oldPlayer != null && oldPlayer.IsSpawned)
//         {
//             oldPlayer.Despawn();
//             playerObjects.Remove(hostConn);
//         }

//         string sessionId = System.Guid.NewGuid().ToString();

//         var session = new SessionData
//         {
//             SessionId = sessionId,
//             Members = new List<NetworkConnection> { hostConn },
//             Type = type
//         };
//         session.Members.Add(hostConn);
//         sessions.Add(sessionId, session);

//         NetworkConnection[] connections = new NetworkConnection[] { hostConn };
//         string sceneName;
//         switch (type)
//         {
//             case SessionType.Login:
//                 sceneName = "MyStation";
//                 break;
//             case SessionType.Station:
//                 sceneName = "Train";
//                 break;
//             case SessionType.Train:
//                 sceneName = "FestivalMainScene";
//                 break;
//             default:
//                 sceneName = "MyStation";
//                 break;
//         }
//         SceneLookupData lookup = new SceneLookupData(sceneName);

//         SceneLoadData sld = new SceneLoadData(sceneName);
//         sld.Options.AllowStacking = true;
//         sld.Options.AutomaticallyUnload = true;// (필요하면)

//         sld.ReplaceScenes = ReplaceOption.None;

//         SceneManager.OnLoadEnd += OnSceneLoadEnd;
//         base.SceneManager.LoadConnectionScenes(connections, sld);


//         if (sceneName == "MyStation")
//         {
//             SceneUnloadData sud = new SceneUnloadData(new string[] { "StartScene" });
//             SceneManager.UnloadConnectionScenes(connections, sud);
//             Debug.Log("씬 삭제됨");
//         }
//         else if (sceneName == "Train")
//         {
//             SceneUnloadData sud = new SceneUnloadData(new string[] { "MyStation", "StartScene" });
//             SceneManager.UnloadConnectionScenes(connections, sud);
//         }
//         else if (sceneName == "FestivalMainScene")
//         {
//             SceneUnloadData sud = new SceneUnloadData(new string[] { "MyStation", "StartScene", "Train" });
//             SceneManager.UnloadConnectionScenes(connections, sud);
//         }
        




//         Debug.Log($"[세션] {type} 세션 생성 & 씬 이동: {sessionId}, {hostConn.ClientId}");
//     }

//     private void OnSceneLoadEnd(SceneLoadEndEventArgs args)
//     {
//         SceneManager.OnLoadEnd -= OnSceneLoadEnd;

//         Scene myScene = default;
//         foreach (var loadedScene in args.LoadedScenes)
//         {
//             if (loadedScene.name == "MyStation")
//             {
//                 myScene = loadedScene;
//                 break;
//             }
//             else if (loadedScene.name == "Train")
//             {
//                 myScene = loadedScene;
//                 break;
//             }
//             else if (loadedScene.name == "FestivalMainScene")
//             {
//                 myScene = loadedScene;
//             }
//         }

//         InstanceFinder.SceneManager.AddConnectionToScene(_pendingConn, myScene);
        
//         Vector3 spawnPos = new Vector3(0, 1, 0);
//         Quaternion spawnRot = Quaternion.identity;

//         GameObject playerObj = Instantiate(playerPrefab, spawnPos, spawnRot);

//         UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(playerObj, myScene);
//         NetworkObject nob = playerObj.GetComponent<NetworkObject>();

//         // 7. 네트워크에 소유자 지정 스폰
//         InstanceFinder.ServerManager.Spawn(nob, _pendingConn);

//     }

//     [ServerRpc(RequireOwnership = false)]
//     internal void CreateSessionFromTagServerRpc(SessionType type, NetworkConnection conn = null)
//     {
//         if (conn == null) conn = base.LocalConnection;
//         CreateSession(conn, type);
//         Debug.Log($"[ServerRpc] CreateSessionFromTagServerRpc 실행됨 - IsServer: {IsServer}, IsOwner: {IsOwner}");

//     }
    
    
// }



using FishNet.Object;
using FishNet.Connection;
using FishNet.Managing.Scened;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using System;
using Unity.VisualScripting;
using FishNet.Transporting;
using UnityEngine.SceneManagement;
using FishNet.Component.Prediction;

public enum SessionType
{
    Lobby,
    Login,
    Station,
    Game,
    Train,
    Festival
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
            CreateSession(conn, SessionType.Lobby);

        }
        if (IsServer)
        {
            Debug.Log("서버다");
        }
    }

    public void CreateSession(NetworkConnection hostConn, SessionType type)
    {

    Debug.Log($"NetworkManager: {InstanceFinder.NetworkManager != null}");
    Debug.Log($"ServerManager: {InstanceFinder.ServerManager != null}");
    Debug.Log($"SceneManager: {InstanceFinder.SceneManager != null}");
    if (InstanceFinder.NetworkManager != null)
    {
        Debug.Log($"IsServerStarted: {InstanceFinder.NetworkManager.IsServerStarted}");
    }
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

        NetworkConnection[] connections = new NetworkConnection[] { hostConn };
        string sceneName;
        switch (type)
        {
            case SessionType.Lobby:
                sceneName = "StartScene";
                break;
            case SessionType.Login:
                sceneName = "MyStation";
                break;
            case SessionType.Station:
                sceneName = "Train";
                break;
            case SessionType.Train:
                sceneName = "FestivalMainScene";
                break;
            default:
                sceneName = "StartScene";
                break;
        }
        SceneLookupData lookup = new SceneLookupData(sceneName);

        SceneLoadData sld = new SceneLoadData(sceneName);
        sld.Options.AllowStacking = true;
        sld.Options.AutomaticallyUnload = true;// (필요하면)

        sld.ReplaceScenes = ReplaceOption.None;

        
        SceneManager.OnLoadEnd += OnSceneLoadEnd;
        base.SceneManager.LoadConnectionScenes(connections, sld);

        Debug.Log($"[세션] {type} 세션 생성 & 씬 이동: {sessionId}, {hostConn.ClientId}");
    }

    private void OnSceneLoadEnd(SceneLoadEndEventArgs args)
    {
        SceneManager.OnLoadEnd -= OnSceneLoadEnd;

        Scene myScene = default;
        foreach (var loadedScene in args.LoadedScenes)
        {
            if (loadedScene.name == "StartScene")
            {
                myScene = loadedScene;
                break;
            }
            if (loadedScene.name == "MyStation")
            {
                myScene = loadedScene;
                break;
            }
            else if (loadedScene.name == "Train")
            {
                myScene = loadedScene;
                break;
            }
            else if (loadedScene.name == "FestivalMainScene")
            {
                myScene = loadedScene;
            }
        }

        InstanceFinder.SceneManager.AddConnectionToScene(_pendingConn, myScene);

        Vector3 spawnPos = new Vector3(0, 1, 0);
        Quaternion spawnRot = Quaternion.identity;

        GameObject playerObj = Instantiate(playerPrefab, spawnPos, spawnRot);

        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(playerObj, myScene);
        NetworkObject nob = playerObj.GetComponent<NetworkObject>();

        // 7. 네트워크에 소유자 지정 스폰
        InstanceFinder.ServerManager.Spawn(nob, _pendingConn);
        
        UnloadOldScenes(args.LoadedScenes[0].name);

    }

    private void UnloadOldScenes(string newSceneName)
    {
        NetworkConnection[] connections = new NetworkConnection[] { _pendingConn };

        if (newSceneName == "MyStation")
        {
            SceneUnloadData sud = new SceneUnloadData(new string[] { "StartScene" });
            SceneManager.UnloadConnectionScenes(connections, sud);
            Debug.Log("씬 삭제됨");
        }
        else if (newSceneName == "Train")
        {
            SceneUnloadData sud = new SceneUnloadData(new string[] { "MyStation", "StartScene" });
            SceneManager.UnloadConnectionScenes(connections, sud);
        }
        else if (newSceneName == "FestivalMainScene")
        {
            SceneUnloadData sud = new SceneUnloadData(new string[] { "MyStation", "StartScene", "Train" });
            SceneManager.UnloadConnectionScenes(connections, sud);
        }
        

    }

    [ServerRpc(RequireOwnership = false)]
    internal void CreateSessionFromTagServerRpc(SessionType type, NetworkConnection conn = null)
    {
        if (conn == null) conn = base.LocalConnection;
        CreateSession(conn, type);
        Debug.Log($"[ServerRpc] CreateSessionFromTagServerRpc 실행됨 - IsServer: {IsServer}, IsOwner: {IsOwner}");

    }
    
    
}
