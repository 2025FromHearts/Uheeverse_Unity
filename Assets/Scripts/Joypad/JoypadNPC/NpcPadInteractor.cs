using UnityEngine;

public class NpcPadInteractor : MonoBehaviour
{
    public PadInputEventRouter padInput;
    public float interactRadius = 50f;

    Transform player;

    void Awake()
    {
        var go = GameObject.FindWithTag("Player");
        if (go != null)
            player = go.transform;
    }

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

        Debug.Log("NpcPadInteractor:TryInteract()");

        Debug.Log(
            $"[TryInteract] UIBlock={UIManager.IsUIBlocking}, mode={padInput.currentMode}"
        );

        if (padInput.currentMode != PadInputEventRouter.InputMode.Player)
            return;

        if (UIManager.IsUIBlocking)
            return;

        if (player == null) return;

        Collider[] hits = Physics.OverlapSphere(player.position, interactRadius);

        foreach (var hit in hits)
        {
            NpcInteract npc = hit.GetComponentInParent<NpcInteract>();
            if (npc != null)
            {
                npc.InteractWithNpcByPad();
                return;
            }
        }
    }

}
