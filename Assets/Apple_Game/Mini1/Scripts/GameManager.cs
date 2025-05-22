using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("게임 설정")]
    [SerializeField] private string _gameId;
    public string CurrentGameID { get; private set; }

    void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeGame()
    {
        StartCoroutine(LoadGameConfigCoroutine());
    }

    private IEnumerator LoadGameConfigCoroutine()
    {
        //string gameId = "a62122b87d5d45cfa660dc75a30dfc28"; //
        string apiUrl = $"http://localhost:8000/map/config/?game_id={_gameId}";

        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Raw JSON: " + request.downloadHandler.text); // 추가
                ProcessGameConfig(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"Error: {request.error}\nURL: {apiUrl}");
                HandleLoadError(request.error);
            }
        }
    }

    private void ProcessGameConfig(string json)
    {
        try
        {
            GameConfig config = JsonUtility.FromJson<GameConfig>(json);

            if (!string.IsNullOrEmpty(config.game_id))
            {
                CurrentGameID = config.game_id;
                Debug.Log($"✅ 게임 설정 로드 완료: {CurrentGameID}");
            }
            else
            {
                throw new System.Exception("서버에서 유효한 game_id를 받지 못함");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❗ JSON 파싱 오류: {e.Message}");
            SetFallbackID();
        }
    }

    private void HandleLoadError(string error)
    {
        Debug.LogError($"❌ 게임 설정 로드 실패: {error}");
        SetFallbackID();
    }

    private void SetFallbackID()
    {
#if UNITY_EDITOR
        CurrentGameID = "EDITOR_FALLBACK_ID"; // 에디터 테스트용
        Debug.LogWarning("서버 연결 실패 - 에디터 폴백 ID 사용");
#else
        Debug.LogError("게임을 시작할 수 없습니다. 인터넷 연결을 확인해 주세요.");
        Application.Quit();
#endif
    }
}

// Django 응답 구조와 매칭되는 클래스
[System.Serializable]
public class GameConfig
{
    public string game_id;
    public string game_name;
    public string game_description;
}
