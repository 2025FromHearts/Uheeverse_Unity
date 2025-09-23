using System.Collections;
using UnityEngine;

public class U_PanelManager : MonoBehaviour
{
    [Header("팝업 Canvas/Panel")]
    public GameObject popupPanel; // 팝업 Panel (처음엔 비활성화!)

    void Start()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false); // 실행 시 무조건 꺼진 상태로 시작
    }

    public void OpenPopup()
    {
        popupPanel.SetActive(true);
    }

    //IEnumerator OpenPopupDelayed()
    //{
    //    popupPanel.SetActive(true);

        // 1프레임 대기 → UI 이벤트 시스템 초기화 보장
    //    yield return null;

    //    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
   // }

    // 취소 버튼 → 패널 끄기
    public void Cancel()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }

    // 예 버튼 → 원하는 기능 실행 후 패널 끄기
    public void Confirm()
    {
        // 예: 씬 이동 같은 기능은 여기 넣어도 되고
        // 아니면 Inspector에서 Yes 버튼에 직접 연결해도 됨
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }
}
