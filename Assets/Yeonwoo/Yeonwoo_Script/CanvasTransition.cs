using UnityEngine;
using UnityEngine.UI;
public class CanvasTransition : MonoBehaviour
{
    public Toggle allAgree;
    public Toggle serviceTerms;
    public Toggle privacyUse;
    public Toggle thirdParty;
    public Toggle marketing;

    public Button nextButton;
    public GameObject infoCanvas;   // 가입 정보 입력 캔버스
    public GameObject termsCanvas;  // 약관 동의 캔버스

    private void Start()
    {
        // 전체 동의 토글 → 나머지 토글 컨트롤
        allAgree.onValueChanged.AddListener(SetAllToggles);

        // 개별 토글 바뀌면 전체 동의 체크 여부 확인 + 다음 버튼 활성화
        serviceTerms.onValueChanged.AddListener(_ => OnIndividualToggleChanged());
        privacyUse.onValueChanged.AddListener(_ => OnIndividualToggleChanged());
        thirdParty.onValueChanged.AddListener(_ => OnIndividualToggleChanged());
        marketing.onValueChanged.AddListener(_ => OnIndividualToggleChanged());

        nextButton.interactable = false;
    }

    void SetAllToggles(bool isOn)
    {
        // 나머지 토글들 상태 바꾸기 (전체 동의 누르면 반응)
        serviceTerms.SetIsOnWithoutNotify(isOn);
        privacyUse.SetIsOnWithoutNotify(isOn);
        thirdParty.SetIsOnWithoutNotify(isOn);
        marketing.SetIsOnWithoutNotify(isOn);

        CheckRequiredTerms(); // 다음 버튼 활성화도 함께 체크
    }

    void OnIndividualToggleChanged()
    {
        // 개별 토글 중 하나라도 해제되면 전체 동의도 해제
        bool allChecked = serviceTerms.isOn && privacyUse.isOn && thirdParty.isOn && marketing.isOn;
        allAgree.SetIsOnWithoutNotify(allChecked);

        CheckRequiredTerms();
    }

    private void CheckRequiredTerms()
    {
        // 필수 항목만 확인 (마케팅 제외)
        bool requiredChecked = serviceTerms.isOn && privacyUse.isOn && thirdParty.isOn;
        nextButton.interactable = requiredChecked;
    }
    public static class AgreeState
    {
        public static bool marketingConsent = false;
    }

    public void OnClickNext()
    {
        AgreeState.marketingConsent = marketing.isOn;

        termsCanvas.SetActive(false);
        infoCanvas.SetActive(true);
    }

    public void PrevCanvas()
    {
        termsCanvas.SetActive(true);
        infoCanvas.SetActive(false); 
    }
}
