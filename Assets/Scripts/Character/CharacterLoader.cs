using UnityEngine;
using System.Collections;

public class CharacterLoader : MonoBehaviour
{
    [Header("CharacterRoot 하위에 등록된 캐릭터 프리팹들")]
    public GameObject[] characterPrefabs;

    IEnumerator Start()
    {
        yield return null;

        // PlayerPrefs에서 서버에서 저장된 캐릭터 스타일 이름 불러오기
        string savedStyle = PlayerPrefs.GetString("character_style", "");
        Debug.Log($"🎨 저장된 캐릭터 스타일: {savedStyle}");

        PlayerInputController inputController = GetComponent<PlayerInputController>();
        GameObject activeCharacter = null;

        // 모든 캐릭터 프리팹 비활성화
        foreach (var prefab in characterPrefabs)
            prefab.SetActive(false);

        // 저장된 스타일 이름과 일치하는 프리팹만 활성화
        foreach (var prefab in characterPrefabs)
        {
            if (prefab.name.Equals(savedStyle, System.StringComparison.OrdinalIgnoreCase))
            {
                prefab.SetActive(true);
                activeCharacter = prefab;
                Debug.Log($"✅ '{prefab.name}' 프리팹 활성화 (PlayerPrefs 기준)");
                break;
            }
        }

        // PlayerPrefs 값이 비었거나 매칭 안 됐을 경우 기본값 사용
        if (activeCharacter == null && characterPrefabs.Length > 0)
        {
            activeCharacter = characterPrefabs[0];
            activeCharacter.SetActive(true);
            Debug.LogWarning($"⚠️ '{savedStyle}' 프리팹을 찾지 못해 기본값 '{activeCharacter.name}' 사용");
        }

        // PlayerInputController 연결
        if (inputController != null && activeCharacter != null)
        {
            inputController.SetActiveCharacter(activeCharacter);
            Debug.Log($"🎬 PlayerInputController에 '{activeCharacter.name}' AnimHandler 연결 완료");
        }
    }
}
