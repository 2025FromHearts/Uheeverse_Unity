using System.Collections;
using FishNet.Object;
using TMPro;
using UnityEngine;
using FishNet.Managing;
using FishNet.Connection;
using FishNet.Transporting;

public class CountdownUI : MonoBehaviour
{
    public TextMeshProUGUI countdownText;
    public KartController kartController; // ���� ���� �ʿ�
    public KartStart ks;

    private NetworkConnection conn; 

    public KartGameManager kmg;

    private void Start()
    {
        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        
        kmg = KartGameManager.Instance;

        KartStart mykart = KartStart.LocalInstance;

        Debug.Log($"kmg isSpawned 확인{kmg.IsSpawned}");

        mykart.server_rpc_kart_dis();
        Debug.Log("카트 비활성화");

        string[] countdowns = { "3", "2", "1", "Go!" };

        foreach (string count in countdowns)
        {
            countdownText.text = count;
            yield return new WaitForSeconds(1f);
        }

        countdownText.gameObject.SetActive(false);
        mykart.server_rpc_kart_en();
        Debug.Log($"{conn.ClientId} 카트 활성화");
        //kartController.enabled = true;
    }
}
