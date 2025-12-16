using UnityEngine;
using UnityEngine.SceneManagement;

public class MyboothOpener : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Range")]
    public float activeRadius = 10.0f;

    [Header("Scene")]
    public string targetSceneName = "MyBooth";

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
        if (padInput.currentMode == PadInputEventRouter.InputMode.UPhone)
            return; //U폰 열려있으면 반응 차단

        if (!isPlayerInRange) return;

        // 씬 이동
        SceneManager.LoadScene(targetSceneName);
    }
}
