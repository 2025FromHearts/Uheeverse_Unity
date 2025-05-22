using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CanvasTransition;

public class RegisterUI : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_InputField password2Input;
    public TMP_InputField emailInput;

    public TMP_Text alarmText;

    private AuthManager authManager;

    private void Start()
    {
        authManager = gameObject.GetComponent<AuthManager>();
        if (authManager == null)
            authManager = gameObject.AddComponent<AuthManager>();
    }

    public void OnClickRegisterButton()
    {
        bool marketingAgree = AgreeState.marketingConsent;
        string username = usernameInput.text;
        string password = passwordInput.text;
        string password2 = password2Input.text;
        string email = emailInput.text;

        //비밀번호 일치 검사
        if (password != password2)
        {
            Debug.LogError("비밀번호가 일치하지 않습니다.");
            alarmText.text = "비밀번호가 일치하지 않습니다.";
            return; // 서버로 요청 보내지 않음
        }

        //서버로 회원가입 요청
        alarmText.text = "";
        authManager.RegisterUser(username, password, email, marketingAgree);
    }
}
