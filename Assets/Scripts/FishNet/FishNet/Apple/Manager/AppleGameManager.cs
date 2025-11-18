using System.Collections.Generic;
using FishNet.Object;
using Unity.Services.Vivox;
using UnityEngine;

public class AppleGameManager : NetworkBehaviour
{
    public static AppleGameManager Instance { get; private set; }
    public ApplePlayerSpawner aps;

    public GameObject UIRoot;

    public GameObject UIStarter;

    public List<int> Apple_Client = new List<int>();

    void Start()
    {
        print("애플 게임 매니저 활성화");
        Debug.Log($"게임 시작. IsSpawned: {IsSpawned}");

    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this; // 씬 전환 시에도 유지
            Debug.Log("Apple Game Manager 인스턴스화 완료");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void Client_add(int ClientId)
    {

        if (Apple_Client.Count < 2)
        {
            Apple_Client.Add(ClientId);
            Debug.Log("클라이언트 추가 완료");
        }

        if (Apple_Client.Count == 2)
        {
            Debug.Log("클라이언트 2명");
            countClient();
        }

        if (Apple_Client.Count > 2)
        {
            print("클라이언트 추가 안됨, 2명이 최대");
        }
    }

    [Server]
    public void countClient()
    {
        
            Debug.Log("게임 시작");
            // RpcCountdownStart();
        
    }


    // [ObserversRpc(ExcludeServer = false)]
    // public void RpcCountdownStart()
    // {
    //     Debug.Log("rpc 실행됨");
    //     //CountdownUI countdownUI = FindAnyObjectByType<CountdownUI>();
    //     GameObject uistarter = GameObject.Find("UIStarter");
    //     UIStarter uiStarterScript = uistarter.GetComponent<UIStarter>();
    //     uiStarterScript.CountdownStart();
    //     // if (uiRoot != null)
    //     // {
    //     //     uiRoot.SetActive(true);
    //     //     Debug.Log("클라이언트 카운트다운 rpc 실행");
    //     // }
    //     // else
    //     // {
    //     //     Debug.Log("uiroot 없음");
    //     // }
        


    //     //countdownUI.gameObject.SetActive(true);
    //     //kartSpawner.gameObject.SetActive(true);
    // }

    // [ObserversRpc(ExcludeServer = false)]
    // public void rpcKartEnable()
    // {
    //     var conn = FishNet.InstanceFinder.ClientManager.Connection;
    //     Debug.Log($"ClientID : {conn.ClientId} rpcKartEnable 진입함");
    //     KartController kartController = FindAnyObjectByType<KartController>();

    //     if(kartController != null)
    //     {
    //         kartController.enabled = true;
    //         Debug.Log("카트 활성화됨");
    //     }
    //     else
    //     {
    //         Debug.Log("카트 컨트롤러 찾을 수 없음");
    //     }
    // }

    // [ObserversRpc(ExcludeServer = false)]
    // public void rpcKartDisable()
    // {
    //     var conn = FishNet.InstanceFinder.ClientManager.Connection;
    //     Debug.Log($"ClientID : {conn.ClientId} rpcKartDisable 진입함");
    //     KartController kartController = FindAnyObjectByType<KartController>();
    //     if(kartController != null)
    //     {
    //         kartController.enabled = false;
    //         Debug.Log("카트 비활성화됨");
    //     }
    //     else
    //     {
    //         Debug.Log("카트 컨트롤러 찾을 수 없음");
    //     }
    // }
}