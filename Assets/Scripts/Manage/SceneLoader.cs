using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneByIndex(int index)
    {
        SceneManager.LoadScene(index);
    }

    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("게임 종료");
    }

    private string getCharacterUrl;
    private string characterCreateSceneName = "CreateCharacter"; 
    private string myStationSceneName = "MyStation";  

    public void LoadSceneByCharacterCheck()
    {
        StartCoroutine(CheckCharacterAndLoadScene());
    }

    private IEnumerator CheckCharacterAndLoadScene()
    {
        string token = PlayerPrefs.GetString("access_token", "");
        getCharacterUrl = ServerConfig.baseUrl + "/users/get_my_character/";

        UnityWebRequest request = UnityWebRequest.Get(getCharacterUrl);
        request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ 캐릭터 조회 결과: " + request.downloadHandler.text);

            CharacterResponse res = JsonUtility.FromJson<CharacterResponse>(request.downloadHandler.text);

            if (res == null || string.IsNullOrEmpty(res.character_id))
            {
                // 캐릭터 없음 → 캐릭터 생성 씬으로 이동
                LoadSceneByName(characterCreateSceneName);
            }
            else
            {
                // 캐릭터 있음 → 정보 저장 후 MyStation으로 이동
                PlayerPrefs.SetString("character_id", res.character_id);
                PlayerPrefs.SetString("character_name", res.characterName);
                PlayerPrefs.SetString("character_style", res.characterStyle);
                PlayerPrefs.Save();

                LoadSceneByName(myStationSceneName);
            }
        }
        else
        {
            Debug.LogError($"❌ 캐릭터 정보 조회 실패: {request.error}\n응답: {request.downloadHandler.text}");
        }
    }

    [System.Serializable]
    public class CharacterResponse
    {
        public string character_id;
        public string characterName;
        public string characterStyle;
    }
}
