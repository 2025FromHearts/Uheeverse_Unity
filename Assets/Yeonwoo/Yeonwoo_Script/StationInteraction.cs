using UnityEngine;
using UnityEngine.UI;

public class StationInteraction : MonoBehaviour
{
    public GameObject uiPanel; // ���� ���� UI �г�
    public LayerMask stationLayer; // �������� ���Ե� ���̾�

    private GameObject currentHovered;

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, stationLayer))
        {
            GameObject target = hit.collider.gameObject;

            // ȣ�� ���̸� ���̶���Ʈ
            if (currentHovered != target)
            {
                if (currentHovered != null)
                {
                    // ȣ�� ����
                    Unhighlight(currentHovered);
                }

                currentHovered = target;
                Highlight(currentHovered);
            }

            // Ŭ�� �� UI Ȱ��ȭ
            if (Input.GetMouseButtonDown(0))
            {
                ShowUIPanel();
            }
            Debug.Log("Raycast hit: " + hit.collider.gameObject.name);
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
        // ���̶���Ʈ ó�� (��: Outline ȿ�� �߰�, ��Ƽ���� ���� ��)
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
