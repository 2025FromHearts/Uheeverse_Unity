using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class CharacterLoader : MonoBehaviour
{
    public ColorChanger hairChanger, eyeChanger, cheekChanger, lipChanger;

    void Start()
    {
        string token = PlayerPrefs.GetString("access_token", "");
        if (!string.IsNullOrEmpty(token))
        {
            StartCoroutine(LoadCharacter(token));
        }
        else
        {
            Debug.LogWarning("❌ 토큰 없음: 로그인 필요");
        }
    }

    IEnumerator LoadCharacter(string token)
    {
        using (UnityWebRequest request = UnityWebRequest.Get("http://localhost:8000/users/get_my_character/"))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("❌ 네트워크 오류: " + request.error);
                yield break;
            }

            string json = request.downloadHandler.text;
            Debug.Log("Raw JSON: " + json); // 실제 데이터 확인

            CharacterWrapper data = JsonUtility.FromJson<CharacterWrapper>(json);
            if (data == null || data.character_status == null)
            {
                Debug.LogError("❗ 데이터 구조 불일치");
                yield break;
            }

            // 싱글톤 인스턴스 강제 생성
            if (CharacterManager.Instance == null)
            {
                new GameObject("CharacterManager").AddComponent<CharacterManager>();
            }

            CharacterManager.Instance.characterStatus = data.character_status;
            CharacterManager.Instance.character_id = data.character_id;

            // 컴포넌트 NULL 체크
            if (hairChanger != null) hairChanger.SetColorFromHex(data.character_status.hairColor);
            else Debug.LogError("hairChanger 미할당");
            // ... 다른 컴포넌트도 동일하게 처리
        }
    }
}