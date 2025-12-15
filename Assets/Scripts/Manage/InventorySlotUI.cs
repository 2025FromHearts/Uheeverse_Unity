using System.Collections;
using UnityEngine;

public class InventorySlotUI : MonoBehaviour
{
    public float selectedScale = 1.1f;
    public float scaleSpeed = 10f;

    Vector3 normalScale;
    Coroutine scaleCo;

    void Awake()
    {
        normalScale = transform.localScale;
    }

    public void SetSelected(bool selected)
    {
        Vector3 target = selected ? normalScale * selectedScale : normalScale;

        if (scaleCo != null)
            StopCoroutine(scaleCo);

        scaleCo = StartCoroutine(ScaleAnim(target));
    }

    IEnumerator ScaleAnim(Vector3 target)
    {
        Vector3 start = transform.localScale;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * scaleSpeed;
            transform.localScale = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.localScale = target;
    }
}
