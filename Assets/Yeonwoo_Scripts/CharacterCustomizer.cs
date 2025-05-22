using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;


public class CharacterCustomizer : MonoBehaviour
{
    public TMP_InputField nameInput;
    public int selectedHairIndex;
    public ColorChanger hairColorChanger;
    public ColorChanger eyeColorChanger;
    public ColorChanger cheekColorChanger;
    public ColorChanger lipColorChanger;

    private string accessToken = ""; // 토큰 불러올 공간

    // 씬 시작되자마자 PlayerPrefs에서 토큰 읽어오기
    void Start()
    {
        accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ0b2tlbl90eXBlIjoiYWNjZXNzIiwiZXhwIjoxNzQ2OTgwODgyLCJpYXQiOjE3NDY5NzcyODIsImp0aSI6IjA3OWY2ZGZjMmU3YzQxN2RhMzIwYWM2NzQ4ZWMzZTAwIiwidXNlcl9pZCI6ImVjZjY5NTBiLWNmNGEtNDY3YS1hODRjLTJjYTAxYTA3MzkzYiIsInVzZXJuYW1lIjoidGVzdHVzZXIiLCJsb2dpbl9jbnQiOjN9.zrIQ0L4UD7oybISqcIDVEs-hHd4RXbCDe7OWHSIxneM";

        Debug.Log("받아온 토큰: " + accessToken);
    }

    public void OnSaveButtonClick()
    {
        CharacterStatus status = new CharacterStatus()
        {
            characterName = nameInput.text,
            hairStyle = selectedHairIndex,
            hairColor = hairColorChanger.selectedHexColor,
            eyeColor = eyeColorChanger.selectedHexColor,
            cheekColor = cheekColorChanger.selectedHexColor,
            lipColor = lipColorChanger.selectedHexColor
        };

        string jsonBody = JsonUtility.ToJson(status);
        StartCoroutine(SaveCharacter(jsonBody));
    }

    IEnumerator SaveCharacter(string jsonBody)
    {
        UnityWebRequest request = new UnityWebRequest("http://localhost:8000/users/save_character/", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // 토큰 사용
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            Debug.Log("캐릭터 저장 성공!");
        else
            Debug.LogError("캐릭터 저장 실패: " + request.error);
    }
}
