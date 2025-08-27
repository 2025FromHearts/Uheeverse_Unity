using UnityEngine;

public class NpcSpawnPoint : MonoBehaviour
{
    [Header("NPC 고정 정보")]
    public string npcId;      // 고정 ID (예: npc_guide, npc_apple)
    public string npcName;    // UI에 표시할 이름
    public bool track = true; // 티켓 체크 대상이면 true
}