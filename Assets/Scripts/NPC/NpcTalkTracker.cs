using UnityEngine;
using TMPro;
using System.Collections;

public class NpcTalkTracker : MonoBehaviour
{
    public static NpcTalkTracker Instance;

    private int talkedCount = 0;
    public int requiredCount = 0;

    [Header("UI 연결")]
    public GameObject notificationObject;      // 알림 텍스트 오브젝트
    public TextMeshProUGUI notificationText;   // 실제 텍스트 컴포넌트
    public float messageDuration = 3f;         // 몇 초 동안 보일지

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

        if (notificationObject != null)
            notificationObject.SetActive(false);
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
            Debug.Log("🎟️ 모든 NPC와 대화를 완료했습니다! 티켓이 발급되었으니 U폰에서 확인해보세요.");
            ShowNotification("축제 체험을 완료했습니다! 티켓이 발급되었으니 U폰에서 확인해보세요.");
        }
    }

    public bool IsAllTalked()
    {
        return requiredCount > 0 && talkedCount >= requiredCount;
    }

    private void ShowNotification(string message)
    {
        if (notificationText == null || notificationObject == null)
        {
            Debug.LogWarning("⚠️ 알림 UI가 연결되지 않았습니다.");
            return;
        }

        notificationText.text = message;
        notificationObject.SetActive(true);
        StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(messageDuration);
        notificationObject.SetActive(false);
    }
}
