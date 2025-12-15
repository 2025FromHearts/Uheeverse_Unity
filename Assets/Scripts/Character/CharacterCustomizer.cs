using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class CharacterCustomizer : MonoBehaviour
{
    [Header("UI 설정")]
    public TMP_InputField nameInput;

    [Header("캐릭터 스타일 (4가지 중 선택)")]
    private string selectedStyle = "";

    [Header("서버 설정")]
    public string nextSceneName = "MyStation"; // 저장 성공 후 이동할 씬 이름

    private string accessToken;
    private string baseUrl;

    void Start()
    {
        // PlayerPrefs에서 토큰 동적 로드
        accessToken = PlayerPrefs.GetString("access_token", "");
        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.LogError("⚠️ 토큰이 없습니다. 로그인 필요");
            var loader = FindAnyObjectByType<SceneLoader>();
            if (loader != null)
                loader.LoadSceneByName("StartScene");
        }
    }

    // 스타일 선택 버튼이 눌렸을 때 호출
    public void OnSelectStyle(string characterStyle)
    {
        selectedStyle = characterStyle;
        Debug.Log($"🎨 선택된 스타일: " + selectedStyle);
    }

    // 저장 버튼 클릭 시 호출
    public void OnSaveButtonClick()
    {
        if (string.IsNullOrWhiteSpace(nameInput.text))
        {
            Debug.LogWarning("❗ 캐릭터 이름을 입력하세요!");
            return;
        }

        if (selectedStyle == "")
        {
            Debug.LogWarning("❗ 캐릭터 스타일을 선택하세요!");
            return;
        }

        CharacterStatus status = new CharacterStatus()
        {
            characterName = nameInput.text.Trim(),
            characterStyle = selectedStyle
        };

        string jsonBody = JsonUtility.ToJson(status);
        Debug.Log("📦 전송 JSON: " + jsonBody);

        StartCoroutine(SaveCharacter(jsonBody));
    }

    IEnumerator SaveCharacter(string jsonBody)
    {
        baseUrl = ServerConfig.baseUrl;
        accessToken = PlayerPrefs.GetString("access_token", "");

        string url = baseUrl + "/users/save_character/";
        UnityWebRequest request = new UnityWebRequest(url, "POST");

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ 캐릭터 저장 성공: " + request.downloadHandler.text);

            // 서버 응답에서 character_id 추출
            string json = request.downloadHandler.text;

            try
            {
                CharacterSaveResponse res = JsonUtility.FromJson<CharacterSaveResponse>(json);

                if (!string.IsNullOrEmpty(res.character_id))
                {
                    PlayerPrefs.SetString("character_id", res.character_id);
                    PlayerPrefs.SetString("character_style", selectedStyle);
                    PlayerPrefs.Save();

                    Debug.Log("🎯 PlayerPrefs 저장됨: " + res.character_id);
                }
                else
                {
                    Debug.LogError("❌ 서버 응답에 character_id가 없습니다.");
                }
            }
            catch
            {
                Debug.LogError("❌ character_id 파싱 실패 → JSON 구조 확인 필요");
            }

            var loader = FindAnyObjectByType<SceneLoader>();
            if (loader != null)
                loader.LoadSceneByName(nextSceneName);
        }
        else
        {
            Debug.LogError($"❌ 저장 실패: {request.error}\n응답: {request.downloadHandler.text}");
            if (request.responseCode == 401)
            {
                PlayerPrefs.DeleteKey("access_token");
                var loader = FindAnyObjectByType<SceneLoader>();
                if (loader != null)
                    loader.LoadSceneByName("StartScene");
            }
        }
    }

    [System.Serializable]
    public class CharacterStatus
    {
        public string characterName;
        public string characterStyle;
    }

    [System.Serializable]
    public class CharacterSaveResponse
    {
        public string status;
        public string character_id;
    }
}
