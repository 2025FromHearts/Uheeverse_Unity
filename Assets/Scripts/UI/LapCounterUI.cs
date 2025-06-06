using UnityEngine;

public class LapCounter : MonoBehaviour
{
    public int totalLaps = 3;
    private int currentLap = 0;
    private float raceTime = 0f;
    private int appleCount = 0;
    private bool raceFinished = false;
    private bool hasStartedLap = false;

    public PlayerStatusUI playerStatusUI;
    public Sprite playerSprite;
    public FinishUI finishUI;

    void Start()
    {
        playerStatusUI.SetPlayerSprite(playerSprite);
        playerStatusUI.UpdateLapText(currentLap, totalLaps);
    }

    void Update()
    {
        if (!raceFinished)
        {
            raceTime += Time.deltaTime;
        }
    }

    public void AddApple()
    {
        appleCount++;
    }

    void OnTriggerEnter(Collider other)
    {
        if (raceFinished) return;

        if (other.CompareTag("Kart"))
        {
            if (!hasStartedLap)
            {
                hasStartedLap = true;
                Debug.Log("[LapCounter] 첫 출발선 통과 - 카운트 시작 준비됨");
                return; // 첫 바퀴는 무시
            }

            currentLap++;
            Debug.Log($"[LapCounter] 현재 랩: {currentLap}");

            playerStatusUI.UpdateLapText(currentLap, totalLaps);

            if (currentLap >= totalLaps)
            {
                raceFinished = true;
                Debug.Log("[LapCounter] 레이스 종료!");

                if (finishUI != null)
                {
                    finishUI.ShowFinishPanel(raceTime, appleCount);
                }
                else
                {
                    Debug.LogWarning("[LapCounter] FinishUI가 할당되지 않았습니다.");
                }
            }
        }
    }

}