using UnityEngine;
using UnityEngine.SceneManagement;
using FishNet;
using FishNet.Managing.Client;
using FishNet.Transporting;

public class LoginSceneLoader : MonoBehaviour
{
    private bool _hasLoaded = false;

    private void Awake()
    {
        // 서버는 절대 이 코드 실행 안 함
        if (Application.isBatchMode)
        {
            Destroy(this);
            return;
        }

        // 클라이언트 연결 콜백 등록
        InstanceFinder.ClientManager.OnClientConnectionState += HandleClientConnected;
    }

    private void HandleClientConnected(ClientConnectionStateArgs args)
    {
        if (_hasLoaded) return;

        if (args.ConnectionState == LocalConnectionState.Started)
        {
            _hasLoaded = true;

            Debug.Log("[클라이언트 연결됨] → 로그인 씬 로드 시작");

            if (SceneManager.GetActiveScene().name != "StartScene")
            {
                SceneManager.LoadScene("StartScene", LoadSceneMode.Additive);
            }
        }
    }

    private void OnDestroy()
    {
        if (InstanceFinder.ClientManager != null)
            InstanceFinder.ClientManager.OnClientConnectionState -= HandleClientConnected;
    }
}
