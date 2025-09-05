using TMPro;
using UnityEngine;

public enum NpcType
{
    Guide,
    Minigame,
    Vendor,
    Photo
}

public class NpcInteract : MonoBehaviour
{
    [Header("고정 키 (티켓/대화 체크용)")]
    [Tooltip("Inspector에서 직접 입력하는 고정 키 (예: apple_guide, apple_vendor1)")]
    public string talkKey;

    [Header("서버 데이터 (자동 세팅됨)")]
    public string npcId;
    public string npcName;
    public NpcType npcType;

    [Header("설정")]
    public float interactionRadius = 50f;
    public float nameShowDistance = 8f;
    public bool isTalking = false;
    [Tooltip("이 NPC가 실행할 미니게임 씬 이름 (Minigame 타입일 경우만 사용)")]
    public string minigameSceneName;
    public NpcGameManager npcGameManager;
    public NpcShopManager npcShopManager;
    public NpcPhotoManager npcPhotoManager;
    [Tooltip("상점 NPC가 표시할 안내 메시지")]
    public string responseMessage;

    [Header("NPC UI")]
    public GameObject roleUI;
    public TMP_Text roleText;

    [Header("참조")]
    public Transform player;
    private string basePrompt;
    private bool hasTalkedThisFrame = false;

    void Start()
    {
        if (player == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go != null) player = go.transform;
        }
        if (roleText != null && !string.IsNullOrEmpty(npcName))
            roleText.text = npcName;
    }

    void Update()
    {
        if (player == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go != null) player = go.transform;
            else return;
        }

        float dist = Vector3.Distance(player.position, transform.position);

        if (roleUI != null)
        {
            bool show = dist <= nameShowDistance;
            if (roleUI.activeSelf != show) roleUI.SetActive(show);
        }

        // 키보드 Space 입력
        if (dist <= interactionRadius && Input.GetKeyDown(KeyCode.Space))
        {
            if (hasTalkedThisFrame) return;
            hasTalkedThisFrame = true;
            InteractWithNpc();
        }
        else
        {
            hasTalkedThisFrame = false;
        }
    }

    // 조이패드 A 입력시 호출할 함수
    public void InteractWithNpcByPad()
    {
        if (player == null) return;
        float dist = Vector3.Distance(player.position, transform.position);
        if (dist <= interactionRadius)
        {
            InteractWithNpc();
        }
    }

    // 실제 NPC 대화/상호작용 처리
    private void InteractWithNpc()
    {
        Debug.Log($"[{npcName}] Interact, npcType={npcType}");
        switch (npcType)
        {
            case NpcType.Guide:
                var talkManager = FindAnyObjectByType<NpcTalkManager>();
                if (talkManager != null)
                {
                    talkManager.TalkToNpc(talkKey, npcName);
                }
                break;
            case NpcType.Minigame:
                npcGameManager?.ShowMinigameDialogue(npcName, minigameSceneName);
                break;
            case NpcType.Vendor:
                npcShopManager?.ShowShopDialogue(npcName);
                break;
            case NpcType.Photo:
                npcPhotoManager?.ShowPhotoDialogue(npcName, this);
                break;
        }
    }

    public void ResetTalkState()
    {
        isTalking = false;
    }

    public void SetNpcData(NpcData data)
    {
        npcId = data.npc_id;
        npcName = data.npc_name;
        basePrompt = data.base_prompt;
        if (roleText != null) roleText.text = npcName;
        string type = data.npc_type.ToLower();
        if (type == "minigame")
        {
            npcType = NpcType.Minigame;
            minigameSceneName = data.scene_name;
        }
        else if (type == "guide") npcType = NpcType.Guide;
        else if (type == "vendor") npcType = NpcType.Vendor;
        else if (type == "photo") npcType = NpcType.Photo;
        else Debug.LogWarning($"⚠️ 알 수 없는 NPC 타입: {type}");
    }
}
