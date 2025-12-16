using System.Collections;
using UnityEngine;

public class BoothSlotHighlight : MonoBehaviour
{
    public Transform visualRoot;   
    public float selectedScale = 1.15f;
    public float scaleSpeed = 12f;

    Vector3 normalScale;

    void Awake()
    {
        normalScale = visualRoot.localScale;
    }

    public void SetSelected(bool selected)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(
            selected ? normalScale * selectedScale : normalScale
        ));
    }

    IEnumerator ScaleTo(Vector3 target)
    {
        Vector3 start = visualRoot.localScale;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * scaleSpeed;
            visualRoot.localScale = Vector3.Lerp(start, target, t);
            yield return null;
        }

        visualRoot.localScale = target;
    }
}
