using UnityEngine;
using System.Collections;

public class CharacterLoader : MonoBehaviour
{
    [Header("CharacterRoot 하위에 등록된 캐릭터 프리팹들")]
    public GameObject[] characterPrefabs;

    void Awake()
    {
        // 모든 캐릭터의 Renderer만 끄기
        foreach (var c in characterPrefabs)
        {
            var visual = c.GetComponent<CharacterVisual>();
            if (visual != null)
                visual.SetVisible(false);
        }
    }

    public void ApplyCharacter(int index)
    {
        for (int i = 0; i < characterPrefabs.Length; i++)
        {
            var visual = characterPrefabs[i].GetComponent<CharacterVisual>();
            if (visual != null)
                visual.SetVisible(i == index);
        }
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.1f);

        PlayerInputController inputController = GetComponent<PlayerInputController>();
        GameObject activeCharacter = null;

        string savedStyle = PlayerPrefs.GetString("character_style", "");
        Debug.Log($"🎨 최종 적용될 캐릭터 스타일: {savedStyle}");

        foreach (var prefab in characterPrefabs)
        {
            var visual = prefab.GetComponent<CharacterVisual>();
            if (visual != null)
                visual.SetVisible(false);

            if (prefab.name.Equals(savedStyle, System.StringComparison.OrdinalIgnoreCase))
            {
                if (visual != null)
                    visual.SetVisible(true);

                activeCharacter = prefab;
                break;
            }
        }

        if (activeCharacter == null && characterPrefabs.Length > 0)
        {
            activeCharacter = characterPrefabs[0];
            var visual = activeCharacter.GetComponent<CharacterVisual>();
            if (visual != null)
                visual.SetVisible(true);
        }

        if (inputController != null && activeCharacter != null)
        {
            inputController.SetActiveCharacter(activeCharacter);
        }
    }
}