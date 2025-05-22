using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;

public class CharacterChecker : MonoBehaviour
{
    public string getCharacterUrl = "http://localhost:8000/users/get_my_character/";

    public void CheckCharacterAndMove()
    {
        StartCoroutine(CheckCharacterCoroutine());
    }

    private IEnumerator CheckCharacterCoroutine()
    {
        string token = PlayerPrefs.GetString("access_token", "");
        UnityWebRequest request = UnityWebRequest.Get(getCharacterUrl);
        request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("캐릭터 조회 응답: " + request.downloadHandler.text);
            // 예시: {"character_id": null, ...} 또는 {"character_id": "efdfd..."}
            CharacterResponse res = JsonUtility.FromJson<CharacterResponse>(request.downloadHandler.text);

            if (string.IsNullOrEmpty(res.character_id))
            {
                // 캐릭터 없음 → 캐릭터 생성 씬으로
                SceneManager.LoadScene("CharacterCreateScene");
            }
            else
            {
                // 캐릭터 있음 → MyStation 씬으로
                SceneManager.LoadScene("MyStationScene");
            }
        }
        else
        {
            Debug.LogError("캐릭터 정보 조회 실패: " + request.error);
            // 디버깅 처리
        }
    }

    [System.Serializable]
    public class CharacterResponse
    {
        public string character_id;
        // 필요할 경우 추가 필드 선언
    }
}
