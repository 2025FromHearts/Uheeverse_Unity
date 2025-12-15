using UnityEngine;
using UnityEngine.SceneManagement;

public class TrainOpener : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Range")]
    public float activeRadius = 10.0f;

    [Header("Scene")]
    public string targetSceneName = "Train";

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
            padInput.OnAPressed += TryMoveScene;
    }

    void OnDisable()
    {
        if (padInput != null)
            padInput.OnAPressed -= TryMoveScene;
    }

    void TryMoveScene()
    {
        if (!isPlayerInRange) return;

        // ¾À ÀÌµ¿
        SceneManager.LoadScene(targetSceneName);
    }
}
