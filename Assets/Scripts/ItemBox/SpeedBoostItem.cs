using System.Collections;
using UnityEngine;

public class SpeedBoostItem : MonoBehaviour
{
    // 충돌 감지 함수 (물리적 충돌이 발생했을 때 호출됨)
    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 대상이 "Player" 태그를 가진 경우에만 실행
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Speed Boost!"); // 디버그 메시지 출력

            // 충돌한 오브젝트에서 KartController 컴포넌트를 가져옴
            KartController kart = collision.gameObject.GetComponent<KartController>();
            if (kart != null)
            {
                // 스피드 부스트 효과를 적용하는 코루틴 실행
                kart.StartCoroutine(ApplySpeedBoost(kart));

                // 이 아이템 박스를 비활성화 (즉시 사라지게 함)
                gameObject.SetActive(false);

                // 또는 완전히 삭제하고 싶다면 아래 코드 사용 가능
                // Destroy(gameObject);
            }
        }
    }

    // 카트 속도를 일시적으로 증가시키는 코루틴
    private IEnumerator ApplySpeedBoost(KartController kart)
    {
        Debug.Log("스피드 부스트 시작!");

        // 속도 배율을 2배로 설정
        kart.SetSpeedMultiplier(5f);

        // 3초 동안 유지
        yield return new WaitForSeconds(5f);

        Debug.Log("스피드 부스트 끝!");

        // 속도 배율을 원래대로 복구
        kart.SetSpeedMultiplier(1f);
    }
}
