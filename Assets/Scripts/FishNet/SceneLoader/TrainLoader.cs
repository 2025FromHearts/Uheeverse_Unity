using FishNet;
using FishNet.Connection;
using FishNet.Object;
using Unity.VisualScripting;
using UnityEngine;
using FishNet.Managing.Logging;
using UnityEngine.SceneManagement;

public class TrainLoader : MonoBehaviour
{
    // public SceneLoadingManager slm;

    [Server]
    private void OnTriggerEnter(Collider other)
    {
        if (!InstanceFinder.IsServer)
        {
            Debug.Log("서버 꺼짐");
            return;
        }
        
        Debug.Log("트리거감지"); 
        NetworkObject nob = other.GetComponent<NetworkObject>();
        if (nob != null)
            LoadScene(nob);
    }

    private void LoadScene(NetworkObject nob)
    {
        if (!nob.Owner.IsActive)
        {
            return;
        }

        Debug.Log("씬로딩 요청");

        // slm = SceneLoadingManager.Instance;

        // slm.CreateSessionFromTag(SceneType.Quiz, "MyStation", nob.Owner);
    }
    // public GameObject playerPrefab;
    // private SceneLoadingManager slm;
    // void Awake()
    // {
    //     NetworkObject nob = playerPrefab.GetComponent<NetworkObject>();

    //     slm = SceneLoadingManager.Instance;
    //     Debug.LogWarning("🔥 MyStationLoader.Awake() 호출됨!");
    //     if (slm != null)
    //     {
    //         Debug.Log("dd");
    //         // OnCreateSessionButton(nob);

    //     }
    //     if (slm == null)
    //         Debug.Log("SessionManager 인스턴스를 찾을 수 없습니다!");
    // }
    // // public SessionManager sessionManager; // Inspector에서 할당

    // // 버튼에서 태그(string)를 받아서 세션 생성 요청
    // public void OnCreateSessionButton(NetworkObject nob)
    // {
    //     Debug.Log("씬 로딩 요청");
    //     NetworkConnection conn = nob.Owner;
    //     // tag 예시: "Lobby", "Game"
    //     // sessionManager.CreateSessionFromTagServerRpc(tag);
    //     // NetworkConnection conn = InstanceFinder.ClientManager.Connection;
    //     slm.CreateSessionFromTag(SceneType.Quiz, "MyStation", conn);
    // }
}
