using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneLoader : MonoBehaviour
{
    void Start()
    {
        LoginLoader();
    }

    void LoginLoader()
    {
        SceneManager.LoadScene("StartScene", LoadSceneMode.Additive);
    }
}
