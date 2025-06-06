using System.Collections;
using UnityEngine;

public class SpinItem: MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Spin!");
            KartController kart = collision.gameObject.GetComponent<KartController>();
            if (kart != null)
                kart.StartCoroutine(Spin(kart));

            gameObject.SetActive(false);
        }
    }

    IEnumerator Spin(KartController kart)
    {
        kart.SetSpinning(true);
        float duration = 1.5f;
        float spinSpeed = 720f;
        float t = 0f;

        while (t < duration)
        {
            kart.transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
            t += Time.deltaTime;
            yield return null;
        }

        kart.SetSpinning(false);
    }
}
