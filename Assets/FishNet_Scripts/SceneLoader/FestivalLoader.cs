using FishNet;
using FishNet.Connection;
using UnityEngine;

public class FestivalLoader : MonoBehaviour
{
    private SceneLoadingManager slm;
    void Awake()
    {
        slm = SceneLoadingManager.Instance;
        Debug.LogWarning("🔥 MyStationLoader.Awake() 호출됨!");
        if (slm != null)
        {
            Debug.Log("dd");
        }
        if (slm == null)
            Debug.Log("SessionManager 인스턴스를 찾을 수 없습니다!");
    }
    // public SessionManager sessionManager; // Inspector에서 할당

    // 버튼에서 태그(string)를 받아서 세션 생성 요청
    public void OnCreateSessionButton()
    {
        // tag 예시: "Lobby", "Game"
        // sessionManager.CreateSessionFromTagServerRpc(tag);
        // NetworkConnection conn = InstanceFinder.ClientManager.Connection;
        slm.CreateSessionFromTagServerRpc(SceneType.Festival, "Train");
    }
}
