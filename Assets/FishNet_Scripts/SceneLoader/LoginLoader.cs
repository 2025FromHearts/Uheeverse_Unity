// using FishNet;
// using FishNet.Connection;
// using FishNet.Managing;
// using FishNet.Managing.Server;
// using FishNet.Managing.Scened;
// using UnityEngine;
// using FishNet.Transporting;
// using System;
// using FishNet.Object;
// using UnityEngine.SceneManagement;

// public class LoginSceneLoader : MonoBehaviour
// {
//     private NetworkConnection _pendingConn;

//     private void Awake()
//     {
//         if (Application.isBatchMode)
//         {
//             Destroy(this);
//             return;
//         }
//         Debug.Log("로그인로더 실행");

//         // 클라이언트 연결 콜백 등록
//         InstanceFinder.ServerManager.OnRemoteConnectionState += OnRemoteClientConnected;
//         // InstanceFinder.ClientManager.OnClientConnectionState += HandleClientConnected;
//     }



//     private void OnRemoteClientConnected(NetworkConnection conn, RemoteConnectionStateArgs args)
//     {
//         if (args.ConnectionState == RemoteConnectionState.Started)
//         {
//             _pendingConn = conn;
//             Debug.Log($"[서버] 클라이언트 {conn.ClientId} 접속됨 → 개인 StationScene 로딩 시작");


//             SceneLoadData sld = new SceneLoadData("StartScene");
//             sld.Options.AllowStacking = true;
//             sld.ReplaceScenes = ReplaceOption.OnlineOnly;



//             InstanceFinder.SceneManager.OnLoadEnd += OnSceneLoadEnd;

//             NetworkConnection[] connections = new NetworkConnection[] { conn };

            

//             InstanceFinder.SceneManager.LoadConnectionScenes(connections, sld);
//         }
//     }

//     private void OnSceneLoadEnd(SceneLoadEndEventArgs args)
//     {
//         if (!InstanceFinder.IsServerStarted)
//             return;

//         InstanceFinder.SceneManager.OnLoadEnd -= OnSceneLoadEnd;
//         Scene StartScene = default;
//         foreach (var loadedScene in args.LoadedScenes)
//         {
//             if (loadedScene.name == "StartScene")
//             {
//                 StartScene = loadedScene;
//                 break;
//             }
//         }
//         if (!StartScene.IsValid())
//         {
//             Debug.LogError("login 씬을 찾을 수 없습니다!");
//             return;
//         }

//         InstanceFinder.SceneManager.AddConnectionToScene(_pendingConn, StartScene);
//     }


//     private void OnDestroy()
//     {
//         if (InstanceFinder.ServerManager != null)
//             InstanceFinder.ServerManager.OnRemoteConnectionState -= OnRemoteClientConnected;
//     }


// }

// using FishNet;
// using FishNet.Connection;
// using FishNet.Managing;
// using FishNet.Managing.Server;
// using FishNet.Managing.Scened;
// using UnityEngine;
// using FishNet.Transporting;
// using System;
// using FishNet.Object;
// using UnityEngine.SceneManagement;
// using System.Collections.Generic;

// public class LoginSceneLoader : MonoBehaviour
// {
//     // 여러 클라이언트를 처리하기 위한 딕셔너리
//     private Dictionary<int, NetworkConnection> _pendingConnections = new Dictionary<int, NetworkConnection>();
    
//     private void Awake()
//     {
//         if (Application.isBatchMode)
//         {
//             Destroy(this);
//             return;
//         }
//         Debug.Log("로그인로더 실행");
        
//         // 서버에서만 실행되도록 확인
//         if (!InstanceFinder.IsServerStarted && !InstanceFinder.IsHostStarted)
//         {
//             // 이벤트 등록을 서버 시작 후에 하도록 지연
//             InstanceFinder.ServerManager.OnServerConnectionState += OnServerStarted;
//         }
//         else
//         {
//             RegisterEvents();
//         }
//     }
    
//     private void OnServerStarted(ServerConnectionStateArgs args)
//     {
//         if (args.ConnectionState == LocalConnectionState.Started)
//         {
//             RegisterEvents();
//             InstanceFinder.ServerManager.OnServerConnectionState -= OnServerStarted;
//         }
//     }
    
//     private void RegisterEvents()
//     {
//         // 이벤트 중복 등록 방지
//         InstanceFinder.ServerManager.OnRemoteConnectionState -= OnRemoteClientConnected;
//         InstanceFinder.ServerManager.OnRemoteConnectionState += OnRemoteClientConnected;
        
//         InstanceFinder.SceneManager.OnLoadEnd -= OnSceneLoadEnd;
//         InstanceFinder.SceneManager.OnLoadEnd += OnSceneLoadEnd;
//     }
    
//     private void OnRemoteClientConnected(NetworkConnection conn, RemoteConnectionStateArgs args)
//     {
//         if (args.ConnectionState == RemoteConnectionState.Started)
//         {
//             Debug.Log($"[서버] 클라이언트 {conn.ClientId} 접속됨 → 개인 StartScene 로딩 시작");
            
//             // 각 클라이언트별로 저장
//             _pendingConnections[conn.ClientId] = conn;
            
//             // 씬 로드 설정
//             SceneLoadData sld = new SceneLoadData("StartScene");
//             sld.Options.AllowStacking = true;
//             sld.ReplaceScenes = ReplaceOption.OnlineOnly;
            
//             // 특정 클라이언트에게만 씬 로드
//             NetworkConnection[] connections = new NetworkConnection[] { conn };
//             InstanceFinder.SceneManager.LoadConnectionScenes(connections, sld);
//         }
//         else if (args.ConnectionState == RemoteConnectionState.Stopped)
//         {
//             // 클라이언트 연결 해제 시 정리
//             if (_pendingConnections.ContainsKey(conn.ClientId))
//             {
//                 _pendingConnections.Remove(conn.ClientId);
//                 Debug.Log($"[서버] 클라이언트 {conn.ClientId} 연결 해제됨");
//             }
//         }
//     }
    
//     private void OnSceneLoadEnd(SceneLoadEndEventArgs args)
//     {
//         if (!InstanceFinder.IsServerStarted)
//             return;
            
//         Debug.Log($"[서버] 씬 로딩 완료 - 연결된 클라이언트 수: {args.QueueData.Connections.Length}");
        
//         Scene startScene = default;
//         foreach (var loadedScene in args.LoadedScenes)
//         {
//             if (loadedScene.name == "StartScene")
//             {

//                 Debug.Log("찾았다 내씬");
//                 startScene = loadedScene;
//                 break;
//             }
//         }
        
//         if (!startScene.IsValid())
//         {
//             Debug.LogError("StartScene을 찾을 수 없습니다!");
//             return;
//         }
        
//         // 씬 로딩이 완료된 모든 클라이언트를 씬에 추가
//         foreach (NetworkConnection conn in args.QueueData.Connections)
//         {
//             if (_pendingConnections.ContainsKey(conn.ClientId))
//             {
//                 Debug.Log($"[서버] 클라이언트 {conn.ClientId}를 StartScene에 추가");
//                 InstanceFinder.SceneManager.AddConnectionToScene(conn, startScene);
                
//                 // 처리 완료된 연결 제거 (선택사항)
//                 // _pendingConnections.Remove(conn.ClientId);
//             }
//         }
//     }
    
//     private void OnDestroy()
//     {
//         // 모든 이벤트 해제
//         if (InstanceFinder.ServerManager != null)
//         {
//             InstanceFinder.ServerManager.OnRemoteConnectionState -= OnRemoteClientConnected;
//             InstanceFinder.ServerManager.OnServerConnectionState -= OnServerStarted;
//         }
        
//         if (InstanceFinder.SceneManager != null)
//         {
//             InstanceFinder.SceneManager.OnLoadEnd -= OnSceneLoadEnd;
//         }
        
//         _pendingConnections.Clear();
//     }
// }

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
using System.Collections.Generic;

public class LoginSceneLoader : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "StartScene"; // 실제 로드할 씬 이름을 인스펙터에서 설정 가능
    
    // 여러 클라이언트를 처리하기 위한 딕셔너리
    private Dictionary<int, NetworkConnection> _pendingConnections = new Dictionary<int, NetworkConnection>();
    
    private void Awake()
    {
        if (Application.isBatchMode)
        {
            Destroy(this);
            return;
        }
        Debug.Log("로그인로더 실행");
        
        // 서버에서만 실행되도록 확인
        if (!InstanceFinder.IsServerStarted && !InstanceFinder.IsHostStarted)
        {
            // 이벤트 등록을 서버 시작 후에 하도록 지연
            InstanceFinder.ServerManager.OnServerConnectionState += OnServerStarted;
        }
        else
        {
            RegisterEvents();
        }
    }
    
    private void OnServerStarted(ServerConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            RegisterEvents();
            InstanceFinder.ServerManager.OnServerConnectionState -= OnServerStarted;
        }
    }
    
    private void RegisterEvents()
    {
        // 이벤트 중복 등록 방지
        InstanceFinder.ServerManager.OnRemoteConnectionState -= OnRemoteClientConnected;
        InstanceFinder.ServerManager.OnRemoteConnectionState += OnRemoteClientConnected;
        
        InstanceFinder.SceneManager.OnLoadEnd -= OnSceneLoadEnd;
        InstanceFinder.SceneManager.OnLoadEnd += OnSceneLoadEnd;
    }
    
    private void OnRemoteClientConnected(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            Debug.Log($"[서버] 클라이언트 {conn.ClientId} 접속됨 → {targetSceneName} 씬 로딩 시작");
            
            // 각 클라이언트별로 저장
            _pendingConnections[conn.ClientId] = conn;
            
            // 씬 로드 설정
            SceneLoadData sld = new SceneLoadData(targetSceneName);
            sld.Options.AllowStacking = true;
            sld.ReplaceScenes = ReplaceOption.OnlineOnly; // OnlineOnly 대신 All로 변경해보기
            
            // 디버깅용 로그
            Debug.Log($"[서버] 씬 로드 요청: {targetSceneName}, 클라이언트: {conn.ClientId}");
            
            // 특정 클라이언트에게만 씬 로드
            NetworkConnection[] connections = new NetworkConnection[] { conn };
            InstanceFinder.SceneManager.LoadConnectionScenes(connections, sld);
        }
        else if (args.ConnectionState == RemoteConnectionState.Stopped)
        {
            // 클라이언트 연결 해제 시 정리
            if (_pendingConnections.ContainsKey(conn.ClientId))
            {
                _pendingConnections.Remove(conn.ClientId);
                Debug.Log($"[서버] 클라이언트 {conn.ClientId} 연결 해제됨");
            }
        }
    }
    
    private void OnSceneLoadEnd(SceneLoadEndEventArgs args)
    {
        if (!InstanceFinder.IsServerStarted)
            return;
            
        Debug.Log($"[서버] 씬 로딩 완료 - 연결된 클라이언트 수: {args.QueueData.Connections.Length}");
        Debug.Log($"[서버] 로드된 씬 목록:");
        
        foreach (var scene in args.LoadedScenes)
        {
            Debug.Log($"  - 씬 이름: {scene.name}, 빌드 인덱스: {scene.buildIndex}, IsValid: {scene.IsValid()}");
        }
        
        Scene startScene = default;
        foreach (var loadedScene in args.LoadedScenes)
        {
            Debug.Log($"[디버그] 씬 비교: '{loadedScene.name}' == 'StartScene' ? {loadedScene.name == "StartScene"}");
            Debug.Log($"[디버그] 씬 상태: IsValid = {loadedScene.IsValid()}, IsLoaded = {loadedScene.isLoaded}");
            
            if (loadedScene.name == "StartScene")
            {
                Debug.Log("찾았다 내씬");
                startScene = loadedScene;
                Debug.Log($"[디버그] startScene 설정 후: IsValid = {startScene.IsValid()}, IsLoaded = {startScene.isLoaded}");
                break;
            }
        }
        
        Debug.Log($"[디버그] 루프 완료 후 startScene: IsValid = {startScene.IsValid()}");
        
        if (!startScene.IsValid())
        {
            Debug.LogError("StartScene을 찾을 수 없습니다!");
            Debug.LogError("Build Settings에서 씬이 포함되어 있는지 확인하세요!");
            return;
        }
        
        // 씬 로딩이 완료된 모든 클라이언트를 씬에 추가
        foreach (NetworkConnection conn in args.QueueData.Connections)
        {
            if (_pendingConnections.ContainsKey(conn.ClientId))
            {
                Debug.Log($"[서버] 클라이언트 {conn.ClientId}를 StartScene에 추가");
                InstanceFinder.SceneManager.AddConnectionToScene(conn, startScene);
                
                // 클라이언트 측 확인용 RPC 호출
                if (conn.FirstObject != null)
                {
                    // 플레이어 객체가 있다면 씬 로드 완료 알림
                    Debug.Log($"[서버] 클라이언트 {conn.ClientId}의 플레이어 객체 확인됨");
                }
            }
        }
    }
    
    private void OnDestroy()
    {
        // 모든 이벤트 해제
        if (InstanceFinder.ServerManager != null)
        {
            InstanceFinder.ServerManager.OnRemoteConnectionState -= OnRemoteClientConnected;
            InstanceFinder.ServerManager.OnServerConnectionState -= OnServerStarted;
        }
        
        if (InstanceFinder.SceneManager != null)
        {
            InstanceFinder.SceneManager.OnLoadEnd -= OnSceneLoadEnd;
        }
        
        _pendingConnections.Clear();
    }
}