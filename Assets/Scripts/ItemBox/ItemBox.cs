using UnityEngine;

public enum ItemEffectType
{
    SpeedUp,
    SlowDown,
    Spin,
    ReverseControl,
    Banana
}

public class ItemBox : MonoBehaviour
{
    public ItemEffectType effectType; // Inspector에서 직접 선택 가능
}
