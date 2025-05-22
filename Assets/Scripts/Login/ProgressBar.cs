using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ProgressBar : MonoBehaviour {

    public Image progressImage;        // Fill 방식 Image
    public TextMeshProUGUI percentText; // 퍼센트 표시 텍스트

    public float duration = 1.5f;      // 로딩 시간

    public void StartProgress(float targetValue) // targetValue: 0.0 ~ 1.0
{
    // 1. 이미지 채우기
    progressImage.DOFillAmount(targetValue, duration).SetEase(Ease.OutCubic);

    // 2. 퍼센트 숫자 증가
    DOTween.To(
        () => progressImage.fillAmount,
        value =>
        {
            int percent = Mathf.RoundToInt(value * 100f);
            percentText.text = percent + "%";
        },
        targetValue,
        duration
    ).SetEase(Ease.OutCubic);
}

void Start()
{
    Invoke(nameof(Demo), 1f);
}

void Demo()
{
    StartProgress(1f);
}
}