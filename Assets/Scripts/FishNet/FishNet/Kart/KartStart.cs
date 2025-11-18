using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using System.Xml.Serialization;

public class KartStart : NetworkBehaviour
{
    public KartGameManager kgm;
    public static KartStart LocalInstance { get; private set; }
    private KartController kart;
    private NetworkConnection conn;

    private void Awake()
    {
        Debug.Log("카트 start awake진입");
        // server_rpc_kart();
    }

    public override void OnStartClient()
    {
        kgm = KartGameManager.Instance;
        base.OnStartClient();

        conn = LocalConnection;
        Debug.Log($"[내 정보] ClientId: {conn.ClientId}, OwnerId: {this.OwnerId}, IsOwner: {IsOwner}");

        if(OwnerId == conn.ClientId)
        {
            Debug.Log("IsOwner 성공!");
            LocalInstance = this;
            server_rpc_kart_dis();
            kgm.Client_add(conn);
        }
        else
        {
            Debug.Log($"IsOwner 실패! 내 ClientId: {conn.ClientId}, 오브젝트 OwnerId: {this.OwnerId}");
        } 
        // kgm = KartGameManager.Instance;
        // base.OnStartClient();

        // conn = LocalConnection;
        // Debug.Log($"{conn.ClientId}");

        // Debug.Log("OnstartClient 활성화");

        // if(IsOwner)
        // {

        //     // conn = LocalConnection;
        //     LocalInstance = this;
        //     Debug.Log("OnStartClient - ServerRpc 호출");
        //     server_rpc_kart_dis();
        //     kgm.Client_add(conn);
        // }
    }


    [ServerRpc(RequireOwnership =false)]
    public void server_rpc_kart_dis()
    {
        Debug.Log("server_rpc_kart_dis 진입");

        if (!IsOwner)
        {
            Debug.Log("오너 아님");
            //return;
        }
        
        // kgm = KartGameManager.Instance;

        // kgm.serverKartDisable();
        serverKartDisable();
        
    }

    [ObserversRpc]
    public void serverKartDisable()
    {
        kart = GetComponent<KartController>();
        if(kart == null)
        {
            Debug.Log("카트 컨트롤러 없어");
        }
        else
        {
            kart.enabled = false;
            Debug.Log("카트 컨트롤러 비활성화 함");
        }
        
    }

    [ServerRpc]
    public void server_rpc_kart_en()
    {
        Debug.Log("server_rpc_kart_en 진입");
        // kgm = KartGameManager.Instance;

        // kgm.serverKartEnable();

        serverKartEnable();

    }

    [ObserversRpc]
    public void serverKartEnable()
    {
        kart = GetComponent<KartController>();
        kart.enabled = true;

    }
}
