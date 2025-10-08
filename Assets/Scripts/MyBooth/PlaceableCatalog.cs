using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PlaceableCatalog", menuName = "Placement/PlaceableCatalog")]
public class PlaceableCatalog : ScriptableObject
{
    public List<PlaceableEntry> entries = new();

    public PlaceableEntry Find(string key)
    {
        return entries.Find(e => e != null &&
            string.Equals(e.key, key, System.StringComparison.OrdinalIgnoreCase));
    }
}

[System.Serializable]
public class PlaceableEntry
{
    [Tooltip("Inventory의 item_icon 값과 동일해야 매칭됩니다.")]
    public string key;          // ex) "GoldenApple", "Bed01"
    public GameObject prefab;   // 배치할 실제 프리팹
}
