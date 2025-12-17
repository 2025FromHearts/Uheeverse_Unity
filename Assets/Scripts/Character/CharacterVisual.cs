using UnityEngine;

public class CharacterVisual : MonoBehaviour
{
    Renderer[] renderers;

    void Awake()
    {
        Init();
    }

    void Init()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>(true);
    }

    public void SetVisible(bool visible)
    {
        Init();

        foreach (var r in renderers)
            r.enabled = visible;
    }
}