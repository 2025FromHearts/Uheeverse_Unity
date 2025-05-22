using UnityEngine;
using UnityEngine.UI;

public class StationInteraction : MonoBehaviour
{
    public GameObject uiPanel; // 축제 즐기기 UI 패널
    public LayerMask stationLayer; // 기차역이 포함된 레이어

    private GameObject currentHovered;

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, stationLayer))
        {
            GameObject target = hit.collider.gameObject;

            // 호버 중이면 하이라이트
            if (currentHovered != target)
            {
                if (currentHovered != null)
                {
                    // 호버 해제
                    Unhighlight(currentHovered);
                }

                currentHovered = target;
                Highlight(currentHovered);
            }

            // 클릭 시 UI 활성화
            if (Input.GetMouseButtonDown(0))
            {
                ShowUIPanel();
            }
        }
        else
        {
            if (currentHovered != null)
            {
                Unhighlight(currentHovered);
                currentHovered = null;
            }
        }
    }

    void Highlight(GameObject obj)
    {
        // 하이라이트 처리 (예: Outline 효과 추가, 머티리얼 변경 등)
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
