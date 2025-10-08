using UnityEngine;

public class NpcTalkTracker : MonoBehaviour
{
    public static NpcTalkTracker Instance;

    private int talkedCount = 0;
    public int requiredCount = 0;

    [Header("티켓 관련")]
    public GameObject ticketCanvas;       // 티켓 담는 캔버스 (Inspector 연결)
    public TicketReveal ticketReveal;     // 애니메이션 스크립트

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

        if (ticketCanvas != null) ticketCanvas.SetActive(false); // 처음엔 꺼둠
    }
    public void SetRequiredCount(int count)
    {
        requiredCount = count;
        Debug.Log($"🎯 NPC 대화 목표 수: {requiredCount}");
    }

    public void MarkNpcAsTalked()
    {
        talkedCount++;
        Debug.Log($"✅ 대화 카운트: {talkedCount}/{requiredCount}");

        if (IsAllTalked())
        {
            Debug.Log("🎟️ 모든 NPC와 대화를 완료했습니다! 티켓 발급 가능합니다.");
            ShowTicket();
        }
    }

    public bool IsAllTalked()
    {
        return requiredCount > 0 && talkedCount >= requiredCount;
    }

    void ShowTicket()
    {
        if (ticketCanvas != null)
        {
            ticketCanvas.SetActive(true); // 캔버스 켜기
            if (ticketReveal != null)
            {
                ticketReveal.Reveal();    // 서서히 나오는 애니메이션 실행
            }
        }
    }
}
