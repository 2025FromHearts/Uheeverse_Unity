using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<CharacterManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("CharacterManager");
                    _instance = obj.AddComponent<CharacterManager>();
                }
            }
            return _instance;
        }
    }
    private static CharacterManager _instance;

    [Header("현재 캐릭터 정보")]
    public string character_id;
    public string character_style;
    public CharacterStatus characterStatus;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // PlayerPrefs 값 불러오기
        character_id = PlayerPrefs.GetString("character_id", "");
        character_style = PlayerPrefs.GetString("character_style", "");

        Debug.Log($"CharacterManager 초기화 완료: ID={character_id}, STYLE={character_style}");
    }

    // 서버 응답으로 캐릭터 데이터 설정할 때 호출
    public void SetCharacterData(string id, string style)
    {
        character_id = id;
        character_style = style;

        PlayerPrefs.SetString("character_id", id);
        PlayerPrefs.SetString("character_style", style);
        PlayerPrefs.Save();
    }
}
