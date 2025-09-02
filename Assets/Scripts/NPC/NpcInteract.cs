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
    public string npcId;    // 서버에서 받은 원래 npc_id
    public string npcName;  // 서버에서 받은 npc_name
    public NpcType npcType;

    [Header("설정")]
    public float interactionRadius = 3f;
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

        if (dist <= interactionRadius && Input.GetKeyDown(KeyCode.Space))
        {
            if (hasTalkedThisFrame) return;
            hasTalkedThisFrame = true;

            Debug.Log($"[{npcName}] Space pressed, npcType={npcType}");

            switch (npcType)
            {
                case NpcType.Guide:
                    var talkManager = FindAnyObjectByType<NpcTalkManager>();
                    if (talkManager != null)
                    {
                        // 👉 이제 티켓 체크는 talkKey로 함
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
        else
        {
            hasTalkedThisFrame = false;
        }
    }

    public void ResetTalkState()
    {
        isTalking = false;
    }

    public void SetNpcData(NpcData data)
    {
        // 서버에서 온 데이터는 그대로 세팅
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
