using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    private string accessToken;

    // JWT 응답 파싱용 구조
    [System.Serializable]
    public class LoginResponse
    {
        public string access;
        public string refresh;
    }

    public void OnLoginButtonClick()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;
        StartCoroutine(Login(username, password));
    }

    IEnumerator Login(string username, string password)
    {
        // 로그인 요청 JSON 데이터 생성
        LoginRequest requestData = new LoginRequest
        {
            username = username,
            password = password
        };

        string jsonData = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest www = new UnityWebRequest(ServerConfig.baseUrl + "/users/token/", "POST");
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
<<<<<<< Updated upstream
            Debug.LogError("Login failed: " + www.error);
            Debug.Log("Sending JSON: " + jsonData);
=======
            Debug.LogError("Login failed: " + www.error + "\n응답: " + www.downloadHandler.text);
>>>>>>> Stashed changes
        }
        else
        {
            // JWT 토큰 파싱 및 저장
            LoginResponse res = JsonUtility.FromJson<LoginResponse>(www.downloadHandler.text);
            accessToken = res.access;

            // 토큰 저장 추가
            PlayerPrefs.SetString("access_token", res.access);
            PlayerPrefs.Save();
            Debug.Log("Login success! Access Token: " + accessToken);
            StartCoroutine(GetUserInfo());
            UnityEngine.SceneManagement.SceneManager.LoadScene("MyStation");
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
<<<<<<< Updated upstream
            Debug.Log("User info: " + www.downloadHandler.text);
=======
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

    IEnumerator CheckOrCreateInventory()
    {
        string url = ServerConfig.baseUrl + "/item/inventory/init/" + characterId + "/";
        Debug.Log("Calling URL: " + url);

        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Inventory init failed: " + www.error);
        }
        else
        {
            Debug.Log("Inventory checked or created: " + www.downloadHandler.text);
>>>>>>> Stashed changes
        }
    }
}
