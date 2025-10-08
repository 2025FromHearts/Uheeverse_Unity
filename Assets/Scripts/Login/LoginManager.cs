// using UnityEngine;
// using UnityEngine.Networking;
// using System.Collections;
// using TMPro;
// using Newtonsoft.Json;
// using FishNet;
// using FishNet.Transporting;
// using System;
// using FishNet.Object;
// using FishNet.Connection;
// using UnityEngine.SceneManagement;

// using FishNet.Managing.Scened;
// using System.Xml.Serialization;

// public class LoginManager : MonoBehaviour
// {
//     private SceneLoadingManager slm;
//     public TMP_InputField usernameInput;
//     public TMP_InputField passwordInput;
//     private string accessToken;
//     private string characterId;

//     // 로그인 요청용 구조체
//     [System.Serializable]
//     public class LoginRequest
//     {
//         public string username;
//         public string password;
//     }

//     public class LoginResponse
//     {
//         public string access;
//         public string refresh;
//     }

//     // 사용자 정보 구조
//     public class UserInfo
//     {
//         public string user_id;
//         public string username;
//         public string character_id;
//     }

//     private void Awake()
//     {
//         Debug.Log("LoginManager Awake 실행됨");

//     }



//     [Obsolete]
//     public void OnLoginButtonClick()
//     {
//         string username = usernameInput.text;
//         string password = passwordInput.text;
//         StartCoroutine(Login(username, password));
//         Debug.Log("로그인버튼실행됨");
//     }

//     [Obsolete]
//     IEnumerator Login(string username, string password)
//     {
//         LoginRequest requestData = new LoginRequest
//         {
//             username = username,
//             password = password
//         };

//         string jsonData = JsonConvert.SerializeObject(requestData);
//         byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

//         UnityWebRequest www = new UnityWebRequest(ServerConfig.baseUrl + "/users/token/", "POST");
//         www.uploadHandler = new UploadHandlerRaw(bodyRaw);
//         www.downloadHandler = new DownloadHandlerBuffer();
//         www.SetRequestHeader("Content-Type", "application/json");

//         Debug.Log("로그인코루틴");

//         yield return www.SendWebRequest();

//         if (www.result != UnityWebRequest.Result.Success)
//         {
//             Debug.LogError("Login failed: " + www.error + "\n응답: " + www.downloadHandler.text);
//             Debug.Log("Sending JSON: " + jsonData);

//             Debug.Log("로그인응답성공");
//         }
//         else
//         {
//             // JWT 토큰 파싱 및 저장
//             LoginResponse res = JsonConvert.DeserializeObject<LoginResponse>(www.downloadHandler.text);
//             accessToken = res.access;

//             PlayerPrefs.SetString("access_token", accessToken);
//             PlayerPrefs.Save();
//             Debug.Log("Login success! Access Token: " + accessToken);

//             yield return StartCoroutine(GetUserInfo());

//             Debug.Log("유저 받아오기");


//             if (InstanceFinder.NetworkManager != null)
//             {
//                 InstanceFinder.ClientManager.StartConnection();
//                 Debug.Log("클라이언트 시작됨");
//             }
//             else
//             {
//                 Debug.LogError("NetworkManager가 없습니다");
//             }


//         }


//         IEnumerator GetUserInfo()
//         {
//             string token = PlayerPrefs.GetString("access_token");
//             UnityWebRequest www = UnityWebRequest.Get(ServerConfig.baseUrl + "/users/get_user_info/");
//             www.SetRequestHeader("Authorization", "Bearer " + token);

//             yield return www.SendWebRequest();

//             if (www.result != UnityWebRequest.Result.Success)
//             {
//                 Debug.LogError("User info request failed: " + www.error);
//             }
//             else
//             {
//                 string json = www.downloadHandler.text;
//                 Debug.Log("User info raw JSON: " + json);

//                 try
//                 {
//                     UserInfo userInfo = JsonConvert.DeserializeObject<UserInfo>(json);
//                     characterId = userInfo.character_id;
//                     Debug.Log("Parsed characterId: " + characterId);
//                     PlayerPrefs.SetString("character_id", characterId);
//                 }
//                 catch (System.Exception e)
//                 {
//                     Debug.LogError("Failed to parse user info: " + e.Message);
//                 }
//             }
//         }

//         IEnumerator CheckOrCreateInventory()
//         {
//             string url = ServerConfig.baseUrl + "/item/inventory/init/" + characterId + "/";
//             Debug.Log("Calling URL: " + url);

//             string token = PlayerPrefs.GetString("access_token");

//             UnityWebRequest www = UnityWebRequest.Get(url);
//             www.SetRequestHeader("Authorization", "Bearer " + token);

//             yield return www.SendWebRequest();

//             if (www.result != UnityWebRequest.Result.Success)
//             {
//                 Debug.LogError("Inventory init failed: " + www.error);
//             }
//             else
//             {
//                 Debug.Log("Inventory checked or created: " + www.downloadHandler.text);
//             }
//         }


//     }
//     private void LoadingMyStation()
//     {
//         slm = SceneLoadingManager.Instance;
//         Debug.Log("로딩마이스테이션 들어옴");
//         slm.CreateSessionFromTag(SceneType.Station, "StartScene");
//     }
    
// }
