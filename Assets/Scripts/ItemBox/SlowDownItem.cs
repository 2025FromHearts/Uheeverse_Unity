using System.Collections;
using UnityEngine;

public class SlowDownItem : MonoBehaviour
{
    // 카트가 아이템과 충돌했을 때 호출됨
    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 대상이 "Player" 태그를 가진 경우에만 실행
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Slowed Down!"); // 디버그 로그

            // 카트 컨트롤러 가져오기
            KartController kart = collision.gameObject.GetComponent<KartController>();
            if (kart != null)
            {
                // 느려지는 효과 적용
                kart.StartCoroutine(SlowSpeed(kart));
            }

            // 아이템 비활성화 (또는 Destroy(gameObject))
            gameObject.SetActive(false);
        }
    }

    // 느려지는 효과를 처리하는 코루틴
    private IEnumerator SlowSpeed(KartController kart)
    {
        // 속도 배율을 절반으로 설정
        kart.SetSpeedMultiplier(0.1f);

        // 5초간 유지
        yield return new WaitForSeconds(5f);

        // 속도 복구
        kart.SetSpeedMultiplier(1f);
    }
}
