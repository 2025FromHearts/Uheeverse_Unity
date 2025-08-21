using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StationInteraction : MonoBehaviour
{
    public GameObject uiPanel;
    public LayerMask stationLayer;
    private GameObject currentHovered;

    void Update()
    {
        // 카메라 확인
        if (Camera.main == null)
        {
            Debug.LogError("Camera.main이 null입니다!");
            return;
        }

        // 씬과 Physics Scene 확인
        var currentScene = SceneManager.GetActiveScene();
        var physicsScene = currentScene.GetPhysicsScene();
        Debug.Log($"현재 씬: {currentScene.name}, Physics Scene 유효: {physicsScene.IsValid()}");

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 전체 레이어에서 테스트
        if (Physics.Raycast(ray, out hit, 100f))
        {
            Debug.Log($"전체 Raycast Hit: {hit.collider.name}, Layer: {hit.collider.gameObject.layer}");
        }
        else
        {
            Debug.Log("전체 Raycast에서 아무것도 감지 안됨");
        }

        // stationLayer에서 테스트
        if (Physics.Raycast(ray, out hit, 100f, stationLayer))
        {
            Debug.Log($"Station Raycast Hit: {hit.collider.name}");
            
            GameObject target = hit.collider.gameObject;

            if (currentHovered != target)
            {
                if (currentHovered != null)
                {
                    Unhighlight(currentHovered);
                }

                currentHovered = target;
                Highlight(currentHovered);
            }

            if (Input.GetMouseButtonDown(0))
            {
                ShowUIPanel();
            }
        }
        else
        {
            Debug.Log("Station Layer Raycast에서 아무것도 감지 안됨");
            
            if (currentHovered != null)
            {
                Unhighlight(currentHovered);
                currentHovered = null;
            }
        }
    }

    void Highlight(GameObject obj)
    {
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend != null)
            rend.material.color = Color.cyan;
    }

    void Unhighlight(GameObject obj)
    {
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend != null)
            rend.material.color = Color.white;
    }

    void ShowUIPanel()
    {
        uiPanel.SetActive(true);
    }
}