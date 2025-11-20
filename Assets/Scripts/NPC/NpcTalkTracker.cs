using System.Collections.Generic;
using UnityEngine;

public class NpcTalkTracker : MonoBehaviour
{
    public static NpcTalkTracker Instance;

    // NPC별 상태 저장
    private Dictionary<string, bool> talkedDict = new Dictionary<string, bool>();

    public int requiredCount = 0;


    // talkedCount는 talkedDict.Values.Where(x=true).Count 로 계산 가능
    public int talkedCount => CountTalked();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 1) NPC 로딩할 때 호출
    public void RegisterNpc(string npcId)
    {
        if (!talkedDict.ContainsKey(npcId))
        {
            talkedDict.Add(npcId, false);
            requiredCount = talkedDict.Count;
        }
    }

    // 2) 실제 대화를 끝냈을 때 호출

    public void MarkNpcAsTalked(string npcId)
    {
        if (!talkedDict.ContainsKey(npcId))
            return;

        // 대화 완료
        talkedDict[npcId] = true;

        Debug.Log($"NPC {npcId} 대화 완료 [{talkedCount}/{requiredCount}]");

        if (talkedCount >= requiredCount)
        {
            Debug.Log("모든 NPC와 대화 완료");

            var ticket = FindAnyObjectByType<TicketReveal>(FindObjectsInactive.Include);
            if (ticket != null)
            {
                string festivalId = PlayerPrefs.GetString("current_map_id", "default_map");
                ticket.OnDialogueCompleted(festivalId);
            }
            else
            {
                Debug.LogWarning("⚠️ TicketReveal을 찾지 못했습니다.");
            }
        }
    }
    private int CountTalked()
    {
        int count = 0;
        foreach (var pair in talkedDict)
            if (pair.Value) count++;
        return count;
    }
    public void SetRequiredCount(int count)
    {
        requiredCount = count;
        Debug.Log($"NPC 대화 목표 수: {requiredCount}");
    }

    public bool IsAllTalked()
    {
        return talkedCount >= requiredCount;
    }
    public List<string> GetAllNpcIds()
    {
        return new List<string>(talkedDict.Keys);
    }
}
