using System.Collections;
using TMPro;
using UnityEngine;

public class CountdownUI : MonoBehaviour
{
    public TextMeshProUGUI countdownText;
    public KartController kartController; // ���� ���� �ʿ�

    public KartGameManager kmg;

    private void Start()
    {
        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        kmg = KartGameManager.Instance;

        KartController kartController = FindAnyObjectByType<KartController>();
        kmg.rpcKartDisable();
        //kartController.enabled = false;

        string[] countdowns = { "3", "2", "1", "Go!" };

        foreach (string count in countdowns)
        {
            countdownText.text = count;
            yield return new WaitForSeconds(1f);
        }

        

        countdownText.gameObject.SetActive(false);
        kmg.rpcKartEnable();
        //kartController.enabled = true;
    }
}
