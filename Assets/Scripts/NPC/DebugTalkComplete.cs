using UnityEngine;
using System.Collections.Generic;
public class DebugTalkComplete : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            var tracker = NpcTalkTracker.Instance;
            if (tracker == null) return;

            foreach (var npcId in tracker.GetAllNpcIds())
            {
                tracker.MarkNpcAsTalked(npcId);
            }

            Debug.Log("F1 → 모든 NPC 대화 완료 처리됨");
        }
    }
}