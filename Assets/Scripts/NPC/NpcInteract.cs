using UnityEngine;

public class NpcInteract : MonoBehaviour
{
    public string npcId;
    public string npcName;
    public float interactionRadius = 3f;
    private Transform player;

    void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(player.position, transform.position);
        if (dist <= interactionRadius && Input.GetKeyDown(KeyCode.Space))
        {
            var talkManager = FindAnyObjectByType<NpcTalkManager>();
            if (talkManager != null)
                talkManager.TalkToNpc(npcId, npcName);  // 이름도 같이 전달
        }
    }
}
