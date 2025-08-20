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
using FishNet.Managing.Logging;

public enum SceneType
{
    Login,
    Station,
    Quiz,
    Festival,
    Game
}

public class SceneLoadingManager : NetworkBehaviour
{
    public GameObject playerPrefab;

    // public static SceneLoadingManager slm{ get; private set; }
    public static SceneLoadingManager Instance { get; private set; }

    private bool isServerInitialized = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this; // 씬 전환 시에도 유지
            Debug.Log("SceneLoadingManager 인스턴스화 완료");
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
        Debug.Log("서버 시작됨");

        isServerInitialized = true;

        if (InstanceFinder.ServerManager != null)
        {
            InstanceFinder.ServerManager.OnAuthenticationResult += OnClientAuthenticated;

            Debug.Log("이벤트 구독됨");
        }

    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"SceneLoadingManager IsClientInitialized: {IsClientInitialized}");
        Debug.Log("클라이언트 시작됨");
    }

    private void OnClientAuthenticated(NetworkConnection conn, bool authenticated)
    {
        if (authenticated) // 인증 성공한 경우만
        {
            Debug.Log($"클라이언트 {conn.ClientId} 인증 완료 - 로그인 씬으로 이동 시작");

            // 잠시 지연 후 로그인 씬으로 이동
            StartCoroutine(DelayedLoginSceneLoad(conn));
        }
    }

    private System.Collections.IEnumerator DelayedLoginSceneLoad(NetworkConnection conn)
    {
        yield return new WaitForSeconds(0.2f); // 짧은 지연

        if (conn != null && conn.IsActive)
        {
            if (!IsServerStarted || !isServerInitialized)
            {
                Debug.Log("서버 끊김 - 연결 중단");
                yield break;
            }

            Debug.Log("startScene 로딩 시작");

            string sceneName = "StartScene";

            SceneLookupData lookup = new SceneLookupData(sceneName);
            SceneLoadData sld = new SceneLoadData(lookup);
            sld.ReplaceScenes = ReplaceOption.None;
            sld.Options.LocalPhysics = LocalPhysicsMode.Physics2D;
            InstanceFinder.SceneManager.LoadConnectionScenes(conn, sld);

            Debug.Log($"✅ [씬 로드] LoadConnectionScenes 호출 완료 - 씬: {sceneName}");

        }
    }

    // [ServerRpc(RequireOwnership = false)]
    internal void CreateSessionFromTagServerRpc(SceneType type, string currentScene)
    {
        if (!IsServer || !isServerInitialized)
        {
            Debug.LogWarning("서버 아님 - 씬 로딩 불가");
            return;
        }


        NetworkConnection caller = base.LocalConnection;
        Debug.Log($"[ServerRpc] caller: {caller.ClientId}");
        SceneLoading(caller, type, currentScene);

    }

    public void SceneLoading(NetworkConnection conn, SceneType type, string currentScene)
    {
        string newScene = GetSceneNameByType(type);
        currentSceneUnloading(conn, currentScene);
        newSceneLoading(conn, newScene);
        
    }
    
    private string GetSceneNameByType(SceneType type)
    {
        switch (type)
        {
            case SceneType.Login: return "StartScene";
            case SceneType.Station: return "MyStation";
            case SceneType.Quiz: return "Train";
            case SceneType.Festival: return "FestivalMainScene";
            case SceneType.Game: return "InGame";
            default: return "StartScene";
        }
    }

    public void currentSceneUnloading(NetworkConnection conn, string currentScene)
    {
        Debug.Log("언로드 진입");
        SceneUnloadData sud = new SceneUnloadData(new string[] { (currentScene) });
        base.SceneManager.UnloadConnectionScenes(conn, sud);
        Debug.Log($"언로드 완료 {conn.ClientId} : {currentScene}");
    }

    public void newSceneLoading(NetworkConnection conn, string newScene)
    {
        Debug.Log($"로딩 시작 {newScene}");

        SceneLookupData lookup = new SceneLookupData(newScene);
        SceneLoadData sld = new SceneLoadData(lookup);
        sld.Options.AllowStacking = true;
        sld.Options.AutomaticallyUnload = false;
        sld.ReplaceScenes = ReplaceOption.None;

        sld.Options.LocalPhysics = LocalPhysicsMode.Physics2D;

        NetworkConnection[] connections = new NetworkConnection[] { conn };
        InstanceFinder.SceneManager.LoadConnectionScenes(conn, sld);
        Debug.Log($"✅ [씬 로드] LoadConnectionScenes 호출 완료 - 씬: {newScene}");

        GameObject go = Instantiate(playerPrefab);
        InstanceFinder.ServerManager.Spawn(go, conn);
    }
}
