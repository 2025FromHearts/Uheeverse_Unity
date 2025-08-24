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

public class BootstrapManager : NetworkBehaviour
{
    private bool isServerInitialized = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
        Debug.Log($"OnClientAuthenticated: ClientId={conn.ClientId}, authenticated={authenticated}");
        if (authenticated) // 인증 성공한 경우만
        {
            Debug.Log($"클라이언트 {conn.ClientId} 인증 완료 - 로그인 씬으로 이동 시작");

            // 잠시 지연 후 로그인 씬으로 이동
            StartCoroutine(DelayedLoginSceneLoad(conn));
        }
    }

    private System.Collections.IEnumerator DelayedLoginSceneLoad(NetworkConnection conn)
    {
        Debug.Log($"[Bootstrap] coroutine START for conn={conn?.ClientId}");
        yield return new WaitForSeconds(0.2f); // 짧은 지연

        if (conn != null && conn.IsActive)
        {
            if (!IsServerStarted || !isServerInitialized)
            {
                Debug.Log("서버 끊김 - 연결 중단");
                yield break;
            }

            while (conn.IsActive && conn.FirstObject == null)
                yield return null;

            Debug.Log("startScene 로딩 시작");

            string sceneName = "MyStation";

            // GameObject obj = GameObject.Find("CharacterRoot(Clone)");
            // NetworkObject nob = obj.GetComponent<NetworkObject>();
            NetworkObject nob = conn.FirstObject;

            SceneLookupData lookup = new SceneLookupData(sceneName);
            SceneLoadData sld = new SceneLoadData(lookup);
            sld.MovedNetworkObjects = new NetworkObject[] { nob };
            sld.ReplaceScenes = ReplaceOption.None;
            sld.Options.AllowStacking = true;
            sld.Options.LocalPhysics = LocalPhysicsMode.Physics2D;
            InstanceFinder.SceneManager.LoadConnectionScenes(nob.Owner, sld);

            Debug.Log($"✅ [씬 로드] LoadConnectionScenes 호출 완료 - 씬: {sceneName}");

        }
    }
}
