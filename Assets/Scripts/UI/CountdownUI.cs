using System.Collections;
using TMPro;
using UnityEngine;

public class CountdownUI : MonoBehaviour
{
    public TextMeshProUGUI countdownText;
    public KartController kartController; // 참조 연결 필요

    private void Start()
    {
        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        kartController.enabled = false;

        string[] countdowns = { "3", "2", "1", "Go!" };

        foreach (string count in countdowns)
        {
            countdownText.text = count;
            yield return new WaitForSeconds(1f);
        }

        countdownText.gameObject.SetActive(false);
        kartController.enabled = true;
    }
}
