using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;

//This is made by Bobsi Unity - Youtube
public class FestivalLoader : NetworkBehaviour
{

        public SceneLoadingManager slm;

    
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
        {
            Debug.Log("서버 꺼짐");
            //return;
        }
        
        Debug.Log("트리거감지");

        if (other.CompareTag("FestivalLoader"))
        {
            NetworkObject nob = GetComponent<NetworkObject>();
            if (nob != null)
                LoadScene(nob);
            Debug.Log("씬로드 함수 호출");
        }
        
    }

    [ServerRpc]
    private void LoadScene(NetworkObject nob)
    {
        if (!nob.Owner.IsActive)
        {
            return;
        }

        Debug.Log("씬로딩 요청");

        slm = SceneLoadingManager.Instance;

        slm.LoadingFestival(SceneType.Festival, "Train", nob.Owner);
    }
}