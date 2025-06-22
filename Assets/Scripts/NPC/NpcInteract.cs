using System.Net.Http;
using UnityEngine;

public enum NpcType
{
    Guide,
    Minigame,
    Vendor
}

public class NpcInteract : MonoBehaviour
{
    public string npcId;
    public string npcName;
    public NpcType npcType;
    public float interactionRadius = 3f;

    [Tooltip("이 NPC가 실행할 미니게임 씬 이름 (Minigame 타입일 경우만 사용)")]
    public string minigameSceneName;

    [Tooltip("이 NPC가 사용할 NpcGameManager (Minigame 타입일 경우만 사용)")]
    public NpcGameManager npcGameManager;

    [Tooltip("이 NPC가 사용할 NpcShopManager (Vendor 타입일 경우만 사용)")]
    public NpcShopManager npcShopManager;

    [Tooltip("상점 NPC가 표시할 안내 메시지")]
    public string responseMessage;

    private Transform player;
    private string basePrompt;
    void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        Debug.Log("NpcInteract 시작");
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(player.position, transform.position);
        if (dist <= interactionRadius && Input.GetKeyDown(KeyCode.Space))
        {
            switch (npcType)
            {
                case NpcType.Guide:
                    var talkManager = FindAnyObjectByType<NpcTalkManager>();
                    if (talkManager != null)
                        talkManager.TalkToNpc(npcId, npcName);
                    break;

                case NpcType.Minigame:
                    if (npcGameManager != null)
                    {
                        npcGameManager.ShowMinigameDialogue(npcName, minigameSceneName);
                    }
                    break;

                case NpcType.Vendor:
                    if (npcShopManager != null)
                    {
                        npcShopManager.ShowShopDialogue(npcName);
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// 외부 JSON 데이터를 기반으로 NPC 정보 설정
    /// </summary>
    public void SetNpcData(NpcData data)
    {
        npcId = data.npc_id;
        npcName = data.npc_name;
        basePrompt = data.base_prompt;

        string type = data.npc_type.ToLower();
        Debug.Log($"[NPC 설정] {npcName}, 타입: {type}");

        if (type == "minigame")
        {
            npcType = NpcType.Minigame;
            minigameSceneName = data.scene_name ?? "";
        }
        else if (type == "guide")
        {
            npcType = NpcType.Guide;
            minigameSceneName = "";
        }
        else if (type == "vendor")
        {
            npcType = NpcType.Vendor;
            minigameSceneName = "";
        }
        else
        {
            Debug.LogWarning($"⚠️ 알 수 없는 NPC 타입: {type}");
        }
    }
}
