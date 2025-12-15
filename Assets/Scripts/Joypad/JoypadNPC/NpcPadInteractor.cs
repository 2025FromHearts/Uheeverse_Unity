using UnityEngine;

public class NpcPadInteractor : MonoBehaviour
{
    public PadInputEventRouter padInput;
    public float interactRadius = 50f;

    void OnEnable()
    {
        if (padInput != null)
            padInput.OnAPressed += TryInteract;
    }

    void OnDisable()
    {
        if (padInput != null)
            padInput.OnAPressed -= TryInteract;
    }

    void TryInteract()
    {

        Debug.Log(
    $"[TryInteract] UIBlock={UIManager.IsUIBlocking}, mode={padInput.currentMode}"
);
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRadius);

        foreach (var hit in hits)
        {
            NpcInteract npc = hit.GetComponent<NpcInteract>();
            if (npc != null)
            {
                npc.InteractWithNpcByPad();
                return;
            }
        }
    }
}
