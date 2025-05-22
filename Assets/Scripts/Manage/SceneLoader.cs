using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    // 기존 메서드들
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

    // 추가: 캐릭터 유무에 따라 분기 이동
    public string getCharacterUrl = "http://localhost:8000/users/get_my_character/";
    public string characterCreateSceneName = "CreateCharacter"; // 실제 씬 이름에 맞게 수정
    public string myStationSceneName = "MyStation";             // 실제 씬 이름에 맞게 수정

    public void LoadSceneByCharacterCheck()
    {
        StartCoroutine(CheckCharacterAndLoadScene());
    }

    private IEnumerator CheckCharacterAndLoadScene()
    {
        string token = PlayerPrefs.GetString("access_token", "");
        UnityWebRequest request = UnityWebRequest.Get(getCharacterUrl);
        request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("캐릭터 조회 응답: " + request.downloadHandler.text);
            CharacterResponse res = JsonUtility.FromJson<CharacterResponse>(request.downloadHandler.text);

            if (string.IsNullOrEmpty(res.character_id))
            {
                // 캐릭터 없음 → 캐릭터 생성 씬으로
                SceneManager.LoadScene(characterCreateSceneName);
            }
            else
            {
                // 캐릭터 있음 → MyStation 씬으로
                SceneManager.LoadScene(myStationSceneName);
            }
        }
        else
        {
            Debug.LogError("캐릭터 정보 조회 실패: " + request.error);
            // 필요하다면 에러 안내 UI 등 처리
        }
    }

    [System.Serializable]
    public class CharacterResponse
    {
        public string character_id;
        // 필요하다면 추가 필드 선언
    }
}
