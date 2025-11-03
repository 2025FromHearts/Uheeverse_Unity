using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "PlaceableCatalog", menuName = "Placement/PlaceableCatalog")]
public class PlaceableCatalog : ScriptableObject
{
    [Tooltip("배치 가능한 아이템 목록입니다.")]
    public List<PlaceableEntry> entries = new();


    /// key(=item_icon)으로 아이템 찾기
    public PlaceableEntry Find(string key)
    {
        return entries.Find(e => e != null &&
            string.Equals(e.key, key, System.StringComparison.OrdinalIgnoreCase));
    }

    /// 동일한 key를 가진 항목들을 병합하고 count 누적
    /// (InventoryUI의 중복 합산 로직과 동일)
    public void MergeDuplicates()
    {
        var merged = new Dictionary<string, PlaceableEntry>();

        foreach (var entry in entries)
        {
            if (entry == null || string.IsNullOrEmpty(entry.key))
                continue;

            string key = entry.key.Trim();

            if (merged.ContainsKey(key))
            {
                merged[key].count += 1;
            }
            else
            {
                // 원본 entry 복제
                merged[key] = new PlaceableEntry
                {
                    key = key,
                    prefab = entry.prefab,
                    count = 1
                };
            }
        }

        entries = merged.Values.ToList();
    }
}

[System.Serializable]
public class PlaceableEntry
{
    [Tooltip("Inventory의 item_icon 값과 동일해야 매칭됩니다.")]
    public string key;          // ex) "GoldenApple", "Bed01"
    public GameObject prefab;   // 배치할 실제 프리팹
    [Min(1)] public int count = 1;  // 동일 아이템 개수
}
