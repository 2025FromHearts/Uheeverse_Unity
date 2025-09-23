using UnityEngine;

public class CloseCanvas : MonoBehaviour
{
    public void CloseSelf()
    {
        gameObject.SetActive(false);
    }
}
