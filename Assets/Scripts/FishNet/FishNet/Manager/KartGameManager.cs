using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using Unity.Services.Vivox;
using UnityEngine;

public class KartGameManager : NetworkBehaviour
{
    public static KartGameManager Instance { get; private set; }
    public KartSpawner kartSpawner;

    public GameObject UIRoot;

    public GameObject UIStarter;

    public List<int> Kart_Client = new List<int>();

    void Start()
    {
        print("카트 게임 매니저 활성화");
        Debug.Log($"게임 시작. IsSpawned: {IsSpawned}");

    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this; // 씬 전환 시에도 유지
            Debug.Log("Kart Game Manager 인스턴스화 완료");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void Client_add(NetworkConnection conn)
    {

        if (Kart_Client.Count < 2)
        {
            Kart_Client.Add(conn.ClientId);
            Debug.Log("클라이언트 추가 완료");
        }

        if (Kart_Client.Count == 2)
        {
            Debug.Log("클라이언트 2명");
            countClient(conn);
        }

        if (Kart_Client.Count > 2)
        {
            print("클라이언트 추가 안됨, 2명이 최대");
        }
    }

    [ServerRpc(RequireOwnership =false)]
    public void countClient(NetworkConnection conn)
    {
            Debug.Log("게임 시작");
            RpcCountdownStart(conn);
    }


    [ObserversRpc]
    public void RpcCountdownStart(NetworkConnection conn)
    {
        Debug.Log("rpc 실행됨");
        GameObject uistarter = GameObject.Find("UIStarter");
        UIStarter uiStarterScript = uistarter.GetComponent<UIStarter>();
        uiStarterScript.CountdownStart();
    }

    [ServerRpc(RequireOwnership =false)]
    public void serverKartEnable(NetworkConnection conn)
    {
        Debug.Log("ServerKartEnable 진입");
        rpcKartEnable();
    }

    [ObserversRpc]
    public void rpcKartEnable()
    {
        var conn = FishNet.InstanceFinder.ClientManager.Connection;
        Debug.Log($"ClientID : {conn.ClientId} rpcKartEnable 진입함");
        KartController kartController = FindAnyObjectByType<KartController>();

        if(kartController != null)
        {
            kartController.enabled = true;
            Debug.Log("카트 활성화됨");
        }
        else
        {
            Debug.Log("카트 컨트롤러 찾을 수 없음");
        }
    }


    [Server]
    public void serverKartDisable()
    {
        Debug.Log("ServerKartDisable 진입");
        rpcKartDisable();
    }

    [ObserversRpc]
    public void rpcKartDisable()
    {
        var conn = FishNet.InstanceFinder.ClientManager.Connection;
        Debug.Log($"ClientID : {conn.ClientId} rpcKartDisable 진입함");
        KartController kartController = FindAnyObjectByType<KartController>();
        if(kartController != null)
        {
            kartController.enabled = false;
            Debug.Log("카트 비활성화됨");
        }
        else
        {
            Debug.Log("카트 컨트롤러 찾을 수 없음");
        }
    }
}