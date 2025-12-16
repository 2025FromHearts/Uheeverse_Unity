using UnityEngine;

public class MyBoothPadController : MonoBehaviour
{
    public PadInputEventRouter padInput;
    public UdpPadReceiver pad;
    public MyBoothUI boothUI;

    public float inputThreshold = 0.6f;
    public float inputDelay = 0.25f;

    int currentIndex = 0;
    float lastInputTime;
    bool isActive = false;


    void OnEnable()
    {
        if (padInput != null)
            padInput.OnAPressed += OnA;
    }

    void OnDisable()
    {
        if (padInput != null)
            padInput.OnAPressed -= OnA;
    }
    public void Activate()
    {
        isActive = true;
        currentIndex = 0;
        UpdateSelection();
        Debug.Log("[MyBoothPad] Activated");
    }

    public void Deactivate()
    {
        isActive = false;
        Debug.Log("[MyBoothPad] Deactivated");
    }

    void Update()
    {
        if (!isActive) return;
        if (pad == null || pad.latest == null) return;
        if (Time.unscaledTime - lastInputTime < inputDelay) return;

        float x = pad.latest.lx;

        if (x > inputThreshold)
            Move(1);
        else if (x < -inputThreshold)
            Move(-1);
    }

    void Move(int dir)
    {
        int count = boothUI.SlotCount;
        if (count == 0) return;

        currentIndex = Mathf.Clamp(currentIndex + dir, 0, count - 1);
        lastInputTime = Time.unscaledTime;

        UpdateSelection();
    }

    void UpdateSelection()
    {
        for (int i = 0; i < boothUI.SlotCount; i++)
        {
            var slotBtn = boothUI.GetSlotButton(i);
            if (slotBtn == null) continue;

            var slotUI = slotBtn.GetComponentInParent<InventorySlotUI>();
            if (slotUI != null)
                slotUI.SetSelected(i == currentIndex);
        }

        boothUI.ShowDetailByIndex(currentIndex);
    }

    void OnA()
    {
        if (!isActive) return;

        Debug.Log("[MyBoothPad] A pressed ¡æ BeginPlacement");
        boothUI.BeginPlacementFromPad();
    }
}
