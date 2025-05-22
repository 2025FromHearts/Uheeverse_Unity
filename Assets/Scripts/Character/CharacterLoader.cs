using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class CharacterLoader : MonoBehaviour
{
    public ColorChanger hairChanger, eyeChanger, cheekChanger, lipChanger;
    public string characterCreateSceneName = "CreateCharacter"; // 씬 이름 확인 필수

    void Start()
    {
        string token = PlayerPrefs.GetString("access_token", "");
        if (!string.IsNullOrEmpty(token))
        {
            StartCoroutine(LoadCharacter(token));
        }
        else
        {
            Debug.LogWarning("❌ 토큰 없음: 로그인 필요");
        }
    }

    IEnumerator LoadCharacter(string token)
    {
        using (UnityWebRequest request = UnityWebRequest.Get("http://localhost:8000/users/get_my_character/"))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);

            yield return request.SendWebRequest();

            // 1. 네트워크 오류 처리 (404 포함)
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"❌ 오류: {request.error} (Status: {request.responseCode})");

                // 404인 경우 캐릭터 생성 씬으로 이동
                if (request.responseCode == 404)
                {
                    Debug.Log("캐릭터 없음 → 생성 씬 이동");
                    SceneManager.LoadScene(characterCreateSceneName);
                }
                yield break;
            }

            // 2. JSON 파싱
            string json = request.downloadHandler.text;
            Debug.Log("Raw JSON: " + json);
            CharacterWrapper data = JsonUtility.FromJson<CharacterWrapper>(json);

            // 3. 데이터 유효성 검사
            if (data == null || string.IsNullOrEmpty(data.character_id))
            {
                Debug.Log("캐릭터 없음 → 생성 씬 이동");
                SceneManager.LoadScene(characterCreateSceneName);
                yield break;
            }

            // 4. 정상 데이터 처리
            if (CharacterManager.Instance == null)
            {
                new GameObject("CharacterManager").AddComponent<CharacterManager>();
            }

            CharacterManager.Instance.characterStatus = data.character_status;
            CharacterManager.Instance.character_id = data.character_id;

            // 5. UI 업데이트
            if (hairChanger != null) hairChanger.SetColorFromHex(data.character_status.hairColor);
            if (eyeChanger != null) eyeChanger.SetColorFromHex(data.character_status.eyeColor);
            if (cheekChanger != null) cheekChanger.SetColorFromHex(data.character_status.cheekColor);
            if (lipChanger != null) lipChanger.SetColorFromHex(data.character_status.lipColor);
        }
    }

    // 서버 응답 구조
    [System.Serializable]
    public class CharacterWrapper
    {
        public string character_id;
        public CharacterStatus character_status;
    }
}
