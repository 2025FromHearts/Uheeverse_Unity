using UnityEngine;

public class UIStoreOpener : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    public GameObject targetPanel;
    public ShopUI shopUI;

    [Header("Range")]
    public float activeRadius = 2.0f;

    [Header("Input")]
    public PadInputEventRouter padInput;

    bool isPlayerInRange;

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(player.position, transform.position);
        isPlayerInRange = dist <= activeRadius;
    }

    void OnEnable()
    {
        if (padInput != null)
            padInput.OnAPressed += TryOpenPanel;
    }

    void OnDisable()
    {
        if (padInput != null)
            padInput.OnAPressed -= TryOpenPanel;
    }

    void TryOpenPanel()
    {
        if (!isPlayerInRange) return;
        if (targetPanel == null) return;

        targetPanel.SetActive(true);

        if (shopUI != null)
            shopUI.OpenShop();
    }
}
