using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System;

public class U_ProfileEditUI : MonoBehaviour
{
    [Header("UI Reference")]
    public TMP_InputField introInputField;   // 수정 입력창
    public TMP_Text profileIntroText;        // 프로필 패널의 한줄소개 텍스트
    public GameObject editPanel;             // 수정 패널 (저장 후 닫힘)
    private string updateIntroUrl;

    void Awake()
    {
        updateIntroUrl = $"{ServerConfig.baseUrl}/users/update_character_intro/";
    }

    // 🔹 “저장하기” 버튼에 연결
    public void OnClickSaveIntro()
    {
        string newIntro = introInputField.text.Trim();
        if (string.IsNullOrEmpty(newIntro))
        {
            Debug.LogWarning("❗ 한줄소개가 비어 있습니다.");
            return;
        }

        StartCoroutine(UpdateCharacterIntro(newIntro));
    }

    IEnumerator UpdateCharacterIntro(string newIntro)
    {
        string token = PlayerPrefs.GetString("access_token", "");
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("❌ 로그인 토큰 없음. 다시 로그인 필요.");
            yield break;
        }

        // JSON Body
        string jsonBody = $"{{\"character_intro\": \"{newIntro}\"}}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest www = new UnityWebRequest(updateIntroUrl, "PUT");
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + token);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Intro 수정 실패: " + www.error);
        }
        else
        {
            try
            {
                IntroUpdateResponse data = JsonUtility.FromJson<IntroUpdateResponse>(www.downloadHandler.text);
                Debug.Log("✅ 한줄소개 수정 완료: " + data.character_intro);

                // 🔹 프로필 패널 텍스트 갱신
                if (profileIntroText != null)
                    profileIntroText.text = data.character_intro;

                // 🔹 수정 패널만 닫기 (프로필 패널은 그대로)
                if (editPanel != null)
                    editPanel.SetActive(false);
            }
            catch (Exception e)
            {
                Debug.LogError("❌ JSON 파싱 실패: " + e.Message);
            }
        }
    }

    [System.Serializable]
    public class IntroUpdateResponse
    {
        public string message;
        public string character_id;
        public string character_intro;
    }
}
