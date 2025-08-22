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

public class SceneLoadingManager : MonoBehaviour
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

    [Server]
    public void CreateSessionFromTag(SceneType type, string currentScene, NetworkConnection conn)
    {
        if (!InstanceFinder.IsServer)
    {
        Debug.LogWarning("서버가 아님");
        return;
    }
        // if (!InstanceFinder.IsServer || !isServerInitialized)
        // {
        //     Debug.LogWarning("서버 아님 - 씬 로딩 불가");
        //     return;
        // }

        Debug.Log($"[ServerRpc] caller: {conn.ClientId}");
        SceneLoading(conn, type, currentScene);

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
            case SceneType.Festival: return "Django_FestivalMainScene";
            case SceneType.Game: return "InGame";
            default: return "StartScene";
        }
    }

    public void currentSceneUnloading(NetworkConnection conn, string currentScene)
    {
        Debug.Log("언로드 진입");
        SceneUnloadData sud = new SceneUnloadData(new string[] { (currentScene) });
        InstanceFinder.SceneManager.UnloadConnectionScenes(conn, sud);
        Debug.Log($"언로드 완료 {conn.ClientId} : {currentScene}");
    }

    public void newSceneLoading(NetworkConnection conn, string newScene)
    {
        Debug.Log($"로딩 시작 {newScene}");

        GameObject obj = GameObject.Find("CharacterRoot(Clone)");
        NetworkObject nob = obj.GetComponent<NetworkObject>();

        SceneLookupData lookup = new SceneLookupData(newScene);
        SceneLoadData sld = new SceneLoadData(lookup);
        sld.Options.AllowStacking = true;
        sld.Options.AutomaticallyUnload = false;
        sld.ReplaceScenes = ReplaceOption.None;
        sld.MovedNetworkObjects = new NetworkObject[] { nob };

        // sld.Options.LocalPhysics = LocalPhysicsMode.Physics2D;
        sld.Options.LocalPhysics = LocalPhysicsMode.None;

        NetworkConnection[] connections = new NetworkConnection[] { conn };
        
        InstanceFinder.SceneManager.LoadConnectionScenes(nob.Owner, sld);
        Debug.Log($"✅ [씬 로드] LoadConnectionScenes 호출 완료 - 씬: {newScene}");

        // GameObject go = Instantiate(playerPrefab);
        // InstanceFinder.ServerManager.Spawn(go, conn);
    }
}
