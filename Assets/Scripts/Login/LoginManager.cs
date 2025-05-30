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

        UnityWebRequest www = new UnityWebRequest("http://localhost:8000/users/token/", "POST");
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Login failed: " + www.error);
        }
        else
        {
            LoginResponse res = JsonConvert.DeserializeObject<LoginResponse>(www.downloadHandler.text);
            accessToken = res.access;

            PlayerPrefs.SetString("access_token", accessToken);
            PlayerPrefs.Save();
            Debug.Log("Login success! Access Token: " + accessToken);

            yield return StartCoroutine(GetUserInfo());
            yield return StartCoroutine(CheckOrCreateInventory());

            UnityEngine.SceneManagement.SceneManager.LoadScene("MyStation");
        }
    }

    IEnumerator GetUserInfo()
    {
        UnityWebRequest www = UnityWebRequest.Get("http://127.0.0.1:8000/users/get_user_info/");
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

    IEnumerator CheckOrCreateInventory()
    {
        string url = "http://127.0.0.1:8000/item/inventory/init/" + characterId + "/";
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
        }
    }
}
