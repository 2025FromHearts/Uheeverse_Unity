using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class CharacterCustomizer : MonoBehaviour
{
    [Header("UI 설정")]
    public TMP_InputField nameInput;
    public int selectedHairIndex;
    public ColorChanger hairColorChanger;
    public ColorChanger eyeColorChanger;
    public ColorChanger cheekColorChanger;
    public ColorChanger lipColorChanger;

    [Header("서버 설정")]
    public string saveCharacterUrl = "http://localhost:8000/users/save_character/";
    public string nextSceneName = "MyStationScene"; // 저장 성공 후 이동할 씬 이름

    private string accessToken;

    void Start()
    {
        // PlayerPrefs에서 토큰 동적 로드
        accessToken = PlayerPrefs.GetString("access_token", "");
        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.LogError("⚠️ 토큰이 없습니다. 로그인 필요");
            // 씬 이동도 SceneLoader로!
            var loader = FindAnyObjectByType<SceneLoader>();
            if (loader != null)
                loader.LoadSceneByName("StartScene");
        }
    }

    public void OnSaveButtonClick()
    {
        if (string.IsNullOrWhiteSpace(nameInput.text))
        {
            Debug.LogWarning("❗ 캐릭터 이름을 입력하세요!");
            return;
        }

        CharacterStatus status = new CharacterStatus()
        {
            characterName = nameInput.text.Trim(),
            hairStyle = selectedHairIndex,
            hairColor = string.IsNullOrEmpty(hairColorChanger.selectedHexColor) ? "#FFFFFFFF" : hairColorChanger.selectedHexColor,
            eyeColor = string.IsNullOrEmpty(eyeColorChanger.selectedHexColor) ? "#FFFFFFFF" : eyeColorChanger.selectedHexColor,
            cheekColor = string.IsNullOrEmpty(cheekColorChanger.selectedHexColor) ? "#FFFFFFFF" : cheekColorChanger.selectedHexColor,
            lipColor = string.IsNullOrEmpty(lipColorChanger.selectedHexColor) ? "#FFFFFFFF" : lipColorChanger.selectedHexColor
        };

        string jsonBody = JsonUtility.ToJson(status);
        StartCoroutine(SaveCharacter(jsonBody));
    }

    IEnumerator SaveCharacter(string jsonBody)
    {
        using (UnityWebRequest request = new UnityWebRequest(saveCharacterUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ 캐릭터 저장 성공: " + request.downloadHandler.text);
                // SceneLoader를 통해 씬 이동
                var loader = FindAnyObjectByType<SceneLoader>();
                if (loader != null)
                    loader.LoadSceneByName(nextSceneName);
                else
                    Debug.LogError("SceneLoader를 찾을 수 없습니다!");
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
    }

    [System.Serializable]
    public class CharacterStatus
    {
        public string characterName;
        public int hairStyle;
        public string hairColor;
        public string eyeColor;
        public string cheekColor;
        public string lipColor;
    }
}
