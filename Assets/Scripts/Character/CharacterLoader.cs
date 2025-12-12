using UnityEngine;
using System.Collections;

public class CharacterLoader : MonoBehaviour
{
    [Header("CharacterRoot 하위에 등록된 캐릭터 프리팹들")]
    public GameObject[] characterPrefabs;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.4f);

        PlayerInputController inputController = GetComponent<PlayerInputController>();
        GameObject activeCharacter = null;

        // 1) 서버에서 이미 저장한 값을 PlayerPrefs에서 가져오기
        string savedStyle = PlayerPrefs.GetString("character_style", "");
        Debug.Log($"🎨 최종 적용될 캐릭터 스타일 (서버 기반): {savedStyle}");

        // 2) 모든 프리팹 비활성화
        foreach (var prefab in characterPrefabs)
            prefab.SetActive(false);

        // 3) savedStyle과 이름이 일치하는 프리팹 활성화
        foreach (var prefab in characterPrefabs)
        {
            if (prefab.name.Equals(savedStyle, System.StringComparison.OrdinalIgnoreCase))
            {
                prefab.SetActive(true);
                activeCharacter = prefab;
                Debug.Log($"✅ 프리팹 활성화: '{prefab.name}' (서버 스타일 기준)");
                break;
            }
        }

        // 4) 해당 스타일의 프리팹을 찾지 못했을 경우 기본값으로 대체
        if (activeCharacter == null && characterPrefabs.Length > 0)
        {
            activeCharacter = characterPrefabs[0];
            activeCharacter.SetActive(true);
            Debug.LogWarning($"⚠️ '{savedStyle}' 프리팹을 찾을 수 없어 기본값 '{activeCharacter.name}' 사용");
        }

        // 5) PlayerInputController에 AnimHandler 등록
        if (inputController != null && activeCharacter != null)
        {
            inputController.SetActiveCharacter(activeCharacter);
            Debug.Log($"🎬 AnimHandler 연결 완료: '{activeCharacter.name}'");
        }
    }
}
