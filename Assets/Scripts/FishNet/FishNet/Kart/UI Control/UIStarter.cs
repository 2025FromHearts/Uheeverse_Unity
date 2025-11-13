using Unity.VisualScripting;
using UnityEngine;

public class UIStarter : MonoBehaviour
{
    public KartGameManager kmg;

    [SerializeField]
    public GameObject uiroot;

    void start()
    {
        
    }

    void Awake()
    {
        Debug.Log("UiStarter 활성화");
    }

    void Update()
    {

    }
    
    public void CountdownStart()
    {
        //GameObject uiroot = GameObject.Find("UIRoot");
        uiroot.SetActive(true);
    }
}
