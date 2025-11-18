using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using Newtonsoft.Json;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    private string accessToken;
    private string characterId;

    // 로그인 요청용 구조체
    [System.Serializable]
    public class LoginRequest
    {
        public string username;
        public string password;
    }

    public class LoginResponse
    {
        public string access;
        public string refresh;
    }

    // 사용자 정보 구조
    public class UserInfo
    {
        public string user_id;
        public string username;
        public string character_id;
    }

    public void OnLoginButtonClick()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;
        StartCoroutine(Login(username, password));
    }

    IEnumerator Login(string username, string password)
    {
        LoginRequest requestData = new LoginRequest
        {
            username = username,
            password = password
        };

        string jsonData = JsonConvert.SerializeObject(requestData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest www = new UnityWebRequest(ServerConfig.baseUrl + "/users/token/", "POST");
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Login failed: " + www.error + "\n응답: " + www.downloadHandler.text);
            Debug.Log("Sending JSON: " + jsonData);
        }
        else
        {
            // JWT 토큰 파싱 및 저장
            LoginResponse res = JsonConvert.DeserializeObject<LoginResponse>(www.downloadHandler.text);
            accessToken = res.access;

            PlayerPrefs.SetString("access_token", accessToken);
            PlayerPrefs.Save();
            Debug.Log("Login success! Access Token: " + accessToken);

            yield return StartCoroutine(GetUserInfo());
            FindObjectOfType<SceneLoader>().LoadSceneByCharacterCheck();
        }
    }

    IEnumerator GetUserInfo()
    {
        UnityWebRequest www = UnityWebRequest.Get(ServerConfig.baseUrl + "/users/get_user_info/");
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("User info request failed: " + www.error);
        }
        else
        {
            string json = www.downloadHandler.text;
            Debug.Log("User info raw JSON: " + json);

            try
            {
                UserInfo userInfo = JsonConvert.DeserializeObject<UserInfo>(json);
                characterId = userInfo.character_id;
                Debug.Log("Parsed characterId: " + characterId);
                PlayerPrefs.SetString("character_id", characterId);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to parse user info: " + e.Message);
            }
        }
    }


}
