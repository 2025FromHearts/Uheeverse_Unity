using UnityEngine;

public class CharacterPreviewManager : MonoBehaviour
{
    [Header("프리뷰용 캐릭터 프리팹들")]
    public GameObject[] characterPrefabs; // Basic, Long, Sports, Braid 순서로 등록

    private void Start()
    {
        // 기본 캐릭터 프리팹만 활성화 & 다른 프리팹 비활성화
        foreach (var prefab in characterPrefabs)
        {
            bool match = prefab.name == "Girl";
            prefab.SetActive(match);
        }
    }

    // 버튼에서 호출할 메서드
    public void OnSelectCharacter(string styleName)
    {
        foreach (var prefab in characterPrefabs)
        {
            bool match = prefab.name == styleName;
            prefab.SetActive(match);
        }

        Debug.Log($"캐릭터 '{styleName}' 미리보기 활성화");
    }
}
