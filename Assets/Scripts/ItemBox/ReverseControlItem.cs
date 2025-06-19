using System.Collections;
using UnityEngine;

public class ReverseControlItem : MonoBehaviour
{

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Reverse Controls!");
            KartController kart = collision.gameObject.GetComponent<KartController>();
            if (kart != null)
            {
                kart.SetReverseControl(true);
                StartCoroutine(Reverse(kart));
            }

            gameObject.SetActive(false);
        }
    }

    IEnumerator Reverse(KartController kart)
    {
        yield return new WaitForSeconds(5f);
        kart.SetReverseControl(false);
    }
}
