using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class ProfileUI : MonoBehaviour
{
    public TMP_Text nicknameText;
    public TMP_Text coinText;
    public TMP_Text introText;
    private string baseUrl;
    private string accessToken;
    private Coroutine coinRefreshCoroutine;

    void Start()
    {
        baseUrl = ServerConfig.baseUrl;
        accessToken = PlayerPrefs.GetString("access_token", "");
        coinRefreshCoroutine = StartCoroutine(PeriodicCharacterInfoRefresh());
    }

    // 일정 간격마다 캐릭터 정보 반복 갱신
    IEnumerator PeriodicCharacterInfoRefresh()
    {
        while (true)
        {
            yield return LoadCharacterInfo();
            yield return new WaitForSeconds(2f);
        }
    }

    IEnumerator LoadCharacterInfo()
    {
        string url = $"{baseUrl}/users/get_my_character_name/";
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ 캐릭터 정보 로드 실패: " + www.error);
            yield break;
        }

        CharacterInfo info = JsonUtility.FromJson<CharacterInfo>(www.downloadHandler.text);
        nicknameText.text = info.character_name;
        introText.text = info.character_intro;
        coinText.text = info.character_coin.ToString();
    }

    [System.Serializable]
    public class CharacterInfo
    {
        public string character_name;
        public string character_intro;
        public int character_coin;
    }

    // 필요하다면 게임 종료나 씬 이동시 코루틴 정리
    void OnDestroy()
    {
        if (coinRefreshCoroutine != null)
            StopCoroutine(coinRefreshCoroutine);
    }
}
