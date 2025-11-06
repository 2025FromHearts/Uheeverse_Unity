using UnityEngine;

public class CharacterLoader : MonoBehaviour
{
    [Header("CharacterRoot 하위에 등록된 캐릭터 프리팹들")]
    public GameObject[] characterPrefabs; // Basic, Girl, Sports, Braid 등

    void Start()
    {
        // 저장된 스타일 불러오기
        string savedStyle = PlayerPrefs.GetString("character_style", "");

        // 모든 캐릭터 프리팹 비활성화
        foreach (var prefab in characterPrefabs)
            prefab.SetActive(false);

        // 저장된 이름과 일치하는 프리팹만 활성화
        foreach (var prefab in characterPrefabs)
        {
            if (prefab.name == savedStyle)
            {
                prefab.SetActive(true);
                Debug.Log($"✅ '{prefab.name}' 프리팹 활성화");
                return;
            }
        }

        // 저장된 값이 없거나 매칭 안 될 경우 기본값 (첫 번째)
        if (characterPrefabs.Length > 0)
        {
            characterPrefabs[0].SetActive(true);
            Debug.LogWarning($"⚠️ '{savedStyle}' 프리팹을 찾지 못해 기본값 '{characterPrefabs[0].name}' 사용");
        }
    }
}
