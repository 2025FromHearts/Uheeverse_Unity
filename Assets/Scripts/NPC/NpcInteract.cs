//using System.Net.Http;
//using TMPro;
//using UnityEngine;


//public enum NpcType
//{
//    Guide,
//    Minigame,
//    Vendor
//}

//public class NpcInteract : MonoBehaviour
//{
//    public string npcId;
//    public string npcName;
//    public NpcType npcType;
//    public float interactionRadius = 3f;
//    public bool isTalking = false;
//    public float nameShowDistance = 8f;

//    [Tooltip("이 NPC가 실행할 미니게임 씬 이름 (Minigame 타입일 경우만 사용)")]
//    public string minigameSceneName;

//    [Tooltip("이 NPC가 사용할 NpcGameManager (Minigame 타입일 경우만 사용)")]
//    public NpcGameManager npcGameManager;

//    [Tooltip("이 NPC가 사용할 NpcShopManager (Vendor 타입일 경우만 사용)")]
//    public NpcShopManager npcShopManager;

//    [Tooltip("상점 NPC가 표시할 안내 메시지")]
//    public string responseMessage;

//    [Header("NPC UI")]
//    public GameObject roleUI;  
//    public TMP_Text roleText;

//    private Transform player;
//    private string basePrompt;
//    void Start()
//    {
//        player = GameObject.FindWithTag("Player")?.transform;
//        Debug.Log("NpcInteract 시작");
//    }

//    void Update()
//    {
//        if (player == null) return;

//        float dist = Vector3.Distance(player.position, transform.position);

//        // 이름 UI 표시 여부
//        if (roleUI != null)
//        {
//            bool show = dist <= nameShowDistance;
//            if (roleUI.activeSelf != show)
//                roleUI.SetActive(show);
//        }

//        // 상호작용 처리
//        if (dist <= interactionRadius && Input.GetKeyDown(KeyCode.Space))
//        {
//            if (isTalking) return;

//            switch (npcType)
//            {
//                case NpcType.Guide:
//                    var talkManager = FindAnyObjectByType<NpcTalkManager>();
//                    if (talkManager != null)
//                    {
//                        talkManager.TalkToNpc(npcId, npcName);
//                        isTalking = true;
//                    }
//                    break;

//                case NpcType.Minigame:
//                    if (npcGameManager != null)
//                    {
//                        npcGameManager.ShowMinigameDialogue(npcName, minigameSceneName);
//                        isTalking = true;
//                    }
//                    break;

//                case NpcType.Vendor:
//                    if (npcShopManager != null)
//                    {
//                        npcShopManager.ShowShopDialogue(npcName);
//                        isTalking = true;
//                    }
//                    break;
//            }
//        }
//    }

//    private string GetNpcRoleName(NpcType type)
//    {
//        switch (type)
//        {
//            case NpcType.Guide: return "안내 NPC";
//            case NpcType.Minigame: return "미니게임 NPC";
//            case NpcType.Vendor: return "상점 NPC";
//            default: return "NPC";
//        }
//    }

//    public void ResetTalkState()
//    {
//        isTalking = false;
//    }


//    /// <summary>
//    /// 외부 JSON 데이터를 기반으로 NPC 정보 설정
//    /// </summary>
//    public void SetNpcData(NpcData data)
//    {
//        npcId = data.npc_id;
//        npcName = data.npc_name;
//        basePrompt = data.base_prompt;

//        if (roleText != null)
//            roleText.text = npcName;

//        string type = data.npc_type.ToLower();
//        Debug.Log($"[NPC 설정] {npcName}, 타입: {type}");

//        if (type == "minigame")
//        {
//            npcType = NpcType.Minigame;
//            minigameSceneName = data.scene_name ?? "";
//        }
//        else if (type == "guide")
//        {
//            npcType = NpcType.Guide;
//            minigameSceneName = "";
//        }
//        else if (type == "vendor")
//        {
//            npcType = NpcType.Vendor;
//            minigameSceneName = "";
//        }
//        else
//        {
//            Debug.LogWarning($"⚠️ 알 수 없는 NPC 타입: {type}");
//        }
//    }
//}
