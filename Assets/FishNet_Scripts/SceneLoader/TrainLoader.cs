using FishNet;
using FishNet.Connection;
using UnityEngine;

public class TrainLoader : MonoBehaviour
{
    private SessionManager sessionManager;
    void Awake()
    {
        sessionManager = SessionManager.Instance;
        Debug.LogWarning("🔥 MyStationLoader.Awake() 호출됨!");
        if (sessionManager != null)
        {
            Debug.Log("dd");
        }
        if (sessionManager == null)
            Debug.Log("SessionManager 인스턴스를 찾을 수 없습니다!");
    }
    // public SessionManager sessionManager; // Inspector에서 할당

    // 버튼에서 태그(string)를 받아서 세션 생성 요청
    public void OnCreateSessionButton()
    {
        // tag 예시: "Lobby", "Game"
        // sessionManager.CreateSessionFromTagServerRpc(tag);
        // NetworkConnection conn = InstanceFinder.ClientManager.Connection;
        SessionManager.Instance.CreateSessionFromTagServerRpc(SessionType.Station);
    }
}
