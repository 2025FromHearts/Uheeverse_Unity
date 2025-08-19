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
using System.Collections;
using Unity.Mathematics;

public enum SessionType
{
    Lobby,
    Login,
    Station,
    Game,
    Train,
    Festival
}

public class SessionData
{
    public string SessionId;
    public List<NetworkConnection> Members = new List<NetworkConnection>();
    public SessionType Type;
    public string SceneName;
}

public class SessionManager : NetworkBehaviour
{
    public GameObject playerPrefab;
    public static SessionManager Instance { get; private set; }

    private int _stackedSceneHandle = 0;

    // 서버 전용 데이터
    private Dictionary<string, SessionData> sessions = new Dictionary<string, SessionData>();
    private Dictionary<NetworkConnection, NetworkObject> playerObjects = new Dictionary<NetworkConnection, NetworkObject>();
    private Dictionary<NetworkConnection, SessionData> connectionToSession = new Dictionary<NetworkConnection, SessionData>();
    private Dictionary<NetworkConnection, string> connectionCurrentScene = new Dictionary<NetworkConnection, string>();
    private Dictionary<NetworkConnection, string> pendingSceneLoads = new Dictionary<NetworkConnection, string>();

    private bool isServerInitialized = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this; // 씬 전환 시에도 유지
            Debug.Log("SessionManager 인스턴스화 완료");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("[SessionManager] 서버 시작됨 - 이벤트 구독 시작");
        
        

        isServerInitialized = true;

        // if (IsServerInitialized)
        // {
        //     // 서버에만 모든 씬 로드 (클라이언트는 안 보임)
        //     StartCoroutine(PreloadAllScenesOnServer());
        // }
        
        // 서버 시작 후 이벤트 구독
        if (InstanceFinder.ServerManager != null)
        {
            InstanceFinder.ServerManager.OnRemoteConnectionState += OnRemoteClientConnected;
            Debug.Log("[SessionManager] 클라이언트 연결 이벤트 구독 완료");
        }
        
        if (InstanceFinder.SceneManager != null)
        {
            InstanceFinder.SceneManager.OnLoadEnd += OnSceneLoadEnd;
            Debug.Log("[SessionManager] 씬 로드 이벤트 구독 완료");
        }
        
        Debug.Log("[SessionManager] 서버 초기화 완료");
    }


    public override void OnStopServer()
    {
        base.OnStopServer();
        Debug.Log("[SessionManager] 서버 중지됨 - 이벤트 구독 해제");

        isServerInitialized = false;

        // 이벤트 구독 해제
        if (InstanceFinder.ServerManager != null)
        {
            InstanceFinder.ServerManager.OnRemoteConnectionState -= OnRemoteClientConnected;
        }

        if (InstanceFinder.SceneManager != null)
        {
            InstanceFinder.SceneManager.OnLoadEnd -= OnSceneLoadEnd;
        }

        // 모든 데이터 정리
        ClearAllData();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        InstanceFinder.SceneManager.OnLoadEnd += OnSceneLoadEnd;
        Debug.Log("[SessionManager] 씬 로드 이벤트 클라이언트 시점 구독 완료");
        Debug.Log("[SessionManager] 클라이언트 시작됨");
    }

    private void ClearAllData()
    {
        sessions.Clear();
        playerObjects.Clear();
        connectionToSession.Clear();
        connectionCurrentScene.Clear();
        pendingSceneLoads.Clear();
        Debug.Log("[SessionManager] 모든 데이터 정리 완료");
    }

    private void OnRemoteClientConnected(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (!isServerInitialized || !IsServerStarted) return;

        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            Debug.Log($"[서버] 클라이언트 {conn.ClientId} 접속됨 - 로비 세션 생성 시작");
            
            // 약간의 지연 후 세션 생성 (FishNet 초기화 완료 대기)
            StartCoroutine(DelayedSessionCreation(conn));
        }
        else if (args.ConnectionState == RemoteConnectionState.Stopped)
        {
            Debug.Log($"[서버] 클라이언트 {conn.ClientId} 연결 해제됨");
            HandleClientDisconnected(conn);
        }
    }

    private System.Collections.IEnumerator DelayedSessionCreation(NetworkConnection conn)
    {
        yield return new WaitForSeconds(0.1f); // 짧은 지연
        
        if (conn != null && conn.IsActive)
        {
            CreateSession(conn, SessionType.Lobby);
        }
    }

    private void HandleClientDisconnected(NetworkConnection conn)
    {
        if (!IsServerStarted || !isServerInitialized) return;

        Debug.Log($"[서버] 클라이언트 {conn.ClientId} 연결 해제 처리 시작");

        // 플레이어 오브젝트 정리
        if (playerObjects.TryGetValue(conn, out var playerObj) && playerObj != null)
        {
            if (playerObj.IsSpawned) 
            {
                playerObj.Despawn();
                Debug.Log($"[서버] 클라이언트 {conn.ClientId} 플레이어 오브젝트 디스폰");
            }
            playerObjects.Remove(conn);
        }

        // 세션에서 제거
        if (connectionToSession.TryGetValue(conn, out var session))
        {
            session.Members.Remove(conn);
            connectionToSession.Remove(conn);
            connectionCurrentScene.Remove(conn);
            pendingSceneLoads.Remove(conn);
            
            Debug.Log($"[서버] 클라이언트 {conn.ClientId}를 세션 {session.SessionId}에서 제거");
            
            // 세션이 비어있으면 정리
            if (session.Members.Count == 0)
            {
                sessions.Remove(session.SessionId);
                Debug.Log($"[서버] 빈 세션 제거: {session.SessionId}");
            }
        }
    }

    public void CreateSession(NetworkConnection hostConn, SessionType type)
    {
        if (!IsServerStarted || !isServerInitialized)
        {
            Debug.LogWarning("[SessionManager] 서버가 초기화되지 않음 - 세션 생성 중단");
            return;
        }

        Debug.Log($"[세션] 클라이언트 {hostConn.ClientId}에 대한 {type} 세션 생성 시작");

        // 기존 플레이어 오브젝트 정리
        if (playerObjects.TryGetValue(hostConn, out var oldPlayer) && oldPlayer != null && oldPlayer.IsSpawned)
        {
            oldPlayer.Despawn();
            playerObjects.Remove(hostConn);
            Debug.Log($"[세션] 클라이언트 {hostConn.ClientId} 기존 플레이어 오브젝트 정리");
        }

        string sessionId = System.Guid.NewGuid().ToString();
        string sceneName = GetSceneNameByType(type);

        var session = new SessionData
        {
            SessionId = sessionId,
            Members = new List<NetworkConnection> { hostConn },
            Type = type,
            SceneName = sceneName
        };

        sessions.Add(sessionId, session);
        connectionToSession[hostConn] = session;

        Debug.Log($"[세션] 세션 데이터 생성 완료: {sessionId}, 타입: {type}, 씬: {sceneName}");

        UnloadOldScenesForConnection(hostConn, sceneName);
        LoadSceneForConnection(hostConn, sceneName, session);

        // 이전 씬 언로드 (해당 클라이언트만)


        // 새 씬 로드 (해당 클라이언트만)

    }

    private string GetSceneNameByType(SessionType type)
    {
        switch (type)
        {
            case SessionType.Lobby: return "StartScene";
            case SessionType.Login: return "MyStation";
            case SessionType.Station: return "Train";
            case SessionType.Train: return "FestivalMainScene";
            default: return "StartScene";
        }
    }

    private void UnloadOldScenesForConnection(NetworkConnection conn, string newSceneName)
    {
        Debug.Log("언로드 진입함");
        string currentSceneName = null;
        

        if (!connectionCurrentScene.TryGetValue(conn, out currentSceneName))
        {
            currentSceneName = "StartScene";
            Debug.Log($"[언로드] 클라이언트 {conn.ClientId}의 현재 씬 정보 없음 - StartScene으로 가정");
        }

        // 현재 씬과 새 씬이 다를 때만 언로드
        if (currentSceneName != newSceneName)
        {
            
            SceneUnloadData sud = new SceneUnloadData(new string[] { (currentSceneName) });
            
            //NetworkConnection conn = base.Owner; 
            base.SceneManager.UnloadConnectionScenes(conn, sud);
            Debug.Log($"[언로드] 클라이언트 {conn.ClientId}: {currentSceneName} → {newSceneName}");
        }
        else
        {
            Debug.Log($"[언로드] 클라이언트 {conn.ClientId}: 동일한 씬이므로 언로드 스킵 ({currentSceneName})");
        }
    }


    private void LoadSceneForConnection(NetworkConnection conn, string sceneName, SessionData session)
    {
        Debug.Log($"🚀 [씬 로드 시작] LoadSceneForConnection 호출됨 - 씬: {sceneName}");
        NetworkConnection[] connections = new NetworkConnection[] { conn };
        

        if (sceneName == "FestivalMainScene")
        {
            Debug.Log("페스티벌 로드");
            SceneLookupData lookupFestival = new SceneLookupData(_stackedSceneHandle, sceneName);
            SceneLoadData sldFestival = new SceneLoadData(lookupFestival);

            sldFestival.Options.AllowStacking = true;
            sldFestival.Options.AutomaticallyUnload = false;
            sldFestival.ReplaceScenes = ReplaceOption.None;

            pendingSceneLoads[conn] = sceneName;

            sldFestival.Options.LocalPhysics = LocalPhysicsMode.Physics3D;

            try
            {
                InstanceFinder.SceneManager.LoadConnectionScenes(connections, sldFestival);
                Debug.Log($"✅ [씬 로드] LoadConnectionScenes 호출 완료 - 씬: {sceneName}");
            }
            catch (System.Exception ex)
            {
                Debug.Log("씬 로드 실패");
            }
        }
        else {
            Debug.Log("일반씬 로드");
            SceneLookupData lookup = new SceneLookupData(sceneName);
            SceneLoadData sld = new SceneLoadData(lookup);
            sld.Options.AllowStacking = true;
            sld.Options.AutomaticallyUnload = false;
            sld.ReplaceScenes = ReplaceOption.None;

            pendingSceneLoads[conn] = sceneName;

            sld.Options.LocalPhysics = LocalPhysicsMode.Physics3D;

            try
            {
                InstanceFinder.SceneManager.LoadConnectionScenes(connections, sld);
                Debug.Log($"✅ [씬 로드] LoadConnectionScenes 호출 완료 - 씬: {sceneName}");
            }
            catch (System.Exception ex)
            {
                Debug.Log("씬 로드 실패");
            }
        }
            

        //if (InstanceFinder.SceneManager != null)
        //{
        //    // 기존 이벤트 구독 해제 후 다시 구독 (중복 방지)
        //    InstanceFinder.SceneManager.OnLoadEnd -= OnSceneLoadEnd;
        //    InstanceFinder.SceneManager.OnLoadEnd += OnSceneLoadEnd;
        //    Debug.Log("✅ OnSceneLoadEnd 이벤트 재구독 완료");
        //}
        
        // 펜딩 씬 로드 추가


        Debug.Log($"[씬 로드] 클라이언트 {conn.ClientId}에 씬 {sceneName} 로드 시작");
    }

    private void OnSceneLoadEnd(SceneLoadEndEventArgs args)
    {
        Debug.Log($"🎯 OnSceneLoadEnd 호출됨!");
        if (!IsServer || !isServerInitialized) return;

        Debug.Log($"[씬 로드 완료] {args.LoadedScenes.Length}개 씬 로드됨");

        // 로드 완료된 씬들을 확인하여 펜딩 중인 연결 찾기
        foreach (var loadedScene in args.LoadedScenes)
        {
            Debug.Log($"[씬 로드 완료] 씬: {loadedScene.name}");

            var connectionsToProcess = new List<NetworkConnection>();

            foreach (var pendingKvp in pendingSceneLoads)
            {
                if (pendingKvp.Value == loadedScene.name)
                {
                    connectionsToProcess.Add(pendingKvp.Key);
                }
            }

            // 해당 씬을 기다리던 연결들 처리
            foreach (var conn in connectionsToProcess)
            {
                if (connectionToSession.TryGetValue(conn, out var session))
                {
                    HandleSceneLoadComplete(conn, loadedScene, session);
                }
                pendingSceneLoads.Remove(conn);
            }
        }

        if (args.LoadedScenes.Length > 0)
        {
            _stackedSceneHandle = args.LoadedScenes[0].handle;
        }
    }

    private void HandleSceneLoadComplete(NetworkConnection conn, Scene targetScene, SessionData session)
    {
        // 연결을 씬에 추가
        InstanceFinder.SceneManager.AddConnectionToScene(conn, targetScene);
        
        // 연결의 현재 씬 업데이트
        connectionCurrentScene[conn] = targetScene.name;
        
        Debug.Log($"[씬 로드 완료] 클라이언트 {conn.ClientId}를 씬 {targetScene.name}에 추가");
        Debug.Log(connectionCurrentScene[conn]);

        // 플레이어 스폰
        SpawnPlayerInScene(conn, targetScene);
    }

    private void SpawnPlayerInScene(NetworkConnection conn, Scene targetScene)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("[스폰 오류] PlayerPrefab이 설정되지 않음!");
            return;
        }

        Vector3 spawnPos = GetSpawnPosition(targetScene.name);
        Quaternion spawnRot = Quaternion.identity;

        GameObject playerObj = Instantiate(playerPrefab, spawnPos, spawnRot);
        
        // 플레이어를 해당 씬으로 이동
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(playerObj, targetScene);
        
        NetworkObject nob = playerObj.GetComponent<NetworkObject>();
        if (nob == null)
        {
            Debug.LogError("[스폰 오류] PlayerPrefab에 NetworkObject 컴포넌트가 없음!");
            Destroy(playerObj);
            return;
        }

        // 네트워크 스폰 (해당 연결에만)
        InstanceFinder.ServerManager.Spawn(nob, conn);
        playerObjects[conn] = nob;

        Debug.Log($"[스폰 완료] 클라이언트 {conn.ClientId} 플레이어를 씬 {targetScene.name}({spawnPos})에 스폰");
    }

    private Vector3 GetSpawnPosition(string sceneName)
    {
        switch (sceneName)
        {
            case "StartScene": return new Vector3(0, 1, 0);
            case "MyStation": return new Vector3(5, 1, 0);
            case "Train": return new Vector3(10, 1, 0);
            case "FestivalMainScene": return new Vector3(15, 1, 0);
            default: return new Vector3(0, 1, 0);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    internal void CreateSessionFromTagServerRpc(SessionType type, NetworkConnection conn = null)
    {
        if (!IsServer || !isServerInitialized) 
        {
            Debug.LogWarning("[ServerRpc] 서버가 초기화되지 않음");
            return;
        }
        
        if (conn == null) conn = base.LocalConnection;
        CreateSession(conn, type);
        Debug.Log($"[ServerRpc] CreateSessionFromTagServerRpc 실행 - 타입: {type}, 클라이언트: {conn.ClientId}");
    }

    // 클라이언트용 씬 전환 요청
    // public void RequestSceneChange(SessionType type)
    // {
    //     Debug.Log($"[요청] 씬 전환 요청: {type}");
        
    //     if (IsServer)
    //     {
    //         if (isServerInitialized)
    //         {
    //             CreateSession(base.LocalConnection, type);
    //         }
    //         else
    //         {
    //             Debug.LogWarning("[요청] 서버가 아직 초기화되지 않음");
    //         }
    //     }
    //     else
    //     {
    //         CreateSessionFromTagServerRpc(type);
    //     }
    // }

    // 디버그용 메서드들
    [ContextMenu("Debug Sessions")]
    public void DebugSessions()
    {
        if (!IsServer) 
        {
            Debug.Log("클라이언트에서는 세션 정보를 볼 수 없습니다.");
            return;
        }
        
        Debug.Log($"=== SessionManager 상태 ===");
        Debug.Log($"서버 초기화: {isServerInitialized}");
        Debug.Log($"IsServer: {IsServer}");
        Debug.Log($"활성 세션: {sessions.Count}개");
        Debug.Log($"플레이어 오브젝트: {playerObjects.Count}개");
        Debug.Log($"연결-세션 매핑: {connectionToSession.Count}개");
        Debug.Log($"현재 씬 정보: {connectionCurrentScene.Count}개");
        Debug.Log($"펜딩 씬 로드: {pendingSceneLoads.Count}개");
        
        foreach (var kvp in sessions)
        {
            var session = kvp.Value;
            Debug.Log($"세션 {session.SessionId}: {session.Type}, 멤버 {session.Members.Count}명, 씬: {session.SceneName}");
        }
        
        foreach (var kvp in connectionCurrentScene)
        {
            Debug.Log($"클라이언트 {kvp.Key.ClientId}: {kvp.Value}");
        }
    }

    [ContextMenu("Force Create Lobby Session")]
    public void ForceCreateLobbySession()
    {
        if (IsServer && isServerInitialized && base.LocalConnection != null)
        {
            CreateSession(base.LocalConnection, SessionType.Lobby);
        }
        else
        {
            Debug.LogWarning("서버가 준비되지 않았거나 연결이 없습니다.");
        }
    }
}