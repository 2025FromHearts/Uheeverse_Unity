using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class AuthManager : MonoBehaviour
{ 

    public void RegisterUser(string username, string password, string email, bool marketingAgree)
    {
        StartCoroutine(RegisterCoroutine(username, password, email));
    }

    IEnumerator RegisterCoroutine(string username, string password, string email)
    {
        string url = "http://localhost:8000/users/register/";

        // JSON 데이터 구성 (약관 3개는 true, 마케팅은 선택적으로 설정 가능)
        string json = JsonUtility.ToJson(new RegisterPayload
        {
            username = username,
            password = password,
            email = email,
            agree_service_terms = true,
            agree_privacy_use = true,
            agree_privacy_third_party = true,
            agree_marketing = false
        });

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("회원가입 성공: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("회원가입 실패: " + request.error + " / " + request.downloadHandler.text);
        }
    }

    [System.Serializable]
    public class RegisterPayload
    {
        public string username;
        public string password;
        public string email;
        public bool agree_service_terms;
        public bool agree_privacy_use;
        public bool agree_privacy_third_party;
        public bool agree_marketing;
    }
}
