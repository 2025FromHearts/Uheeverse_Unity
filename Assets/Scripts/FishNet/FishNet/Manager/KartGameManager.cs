using System.Collections.Generic;
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

    public void Client_add(int ClientId)
    {

        if (Kart_Client.Count < 2)
        {
            Kart_Client.Add(ClientId);
            Debug.Log("클라이언트 추가 완료");
        }

        if (Kart_Client.Count == 2)
        {
            Debug.Log("클라이언트 2명");
            countClient();
        }

        if (Kart_Client.Count > 2)
        {
            print("클라이언트 추가 안됨, 2명이 최대");
        }
    }

    [Server]
    public void countClient()
    {
        
            Debug.Log("게임 시작");
            RpcCountdownStart();
        
    }


    [ObserversRpc(ExcludeServer = false)]
    public void RpcCountdownStart()
    {
        Debug.Log("rpc 실행됨");
        //CountdownUI countdownUI = FindAnyObjectByType<CountdownUI>();
        GameObject uistarter = GameObject.Find("UIStarter");
        UIStarter uiStarterScript = uistarter.GetComponent<UIStarter>();
        uiStarterScript.CountdownStart();
        // if (uiRoot != null)
        // {
        //     uiRoot.SetActive(true);
        //     Debug.Log("클라이언트 카운트다운 rpc 실행");
        // }
        // else
        // {
        //     Debug.Log("uiroot 없음");
        // }
        


        //countdownUI.gameObject.SetActive(true);
        //kartSpawner.gameObject.SetActive(true);
    }

    [ObserversRpc(ExcludeServer = false)]
    public void rpcKartEnable()
    {
        KartController kartController = FindAnyObjectByType<KartController>();
        kartController.enabled = true;
    }

    [ObserversRpc(ExcludeServer = false)]
    public void rpcKartDisable()
    {
        KartController kartController = FindAnyObjectByType<KartController>();
        kartController.enabled = false;
    }
}
