using UnityEngine;

public class ItemAttacher : MonoBehaviour
{
    public Transform backAttachPoint;
    public Transform hatAttachPoint;

    public Transform GetAttachPoint(string itemType)
    {
        switch (itemType)
        {
            case "Back":
                return backAttachPoint;
            case "Hat":
                return hatAttachPoint;
            default:
                Debug.LogWarning($"[ItemAttacher] Unknown item_type: {itemType}");
                return null;
        }
    }
}
