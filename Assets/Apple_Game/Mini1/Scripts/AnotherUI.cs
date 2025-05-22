using UnityEngine;
using TMPro;

public class AnotherUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI anotherScoreText;

    void Start()
    {
        // PlayerScoreUI 인스턴스가 없을 경우 에러 방지
        if (PlayerScoreUI.Instance != null)
        {
            // 초기 값 설정
            anotherScoreText.text = PlayerScoreUI.Instance.GetScore().ToString();
        }
    }

    void Update()
    {
        // 실시간 업데이트 (필요시)
        if (PlayerScoreUI.Instance != null)
        {
            anotherScoreText.text = PlayerScoreUI.Instance.GetScore().ToString();
        }
    }
}