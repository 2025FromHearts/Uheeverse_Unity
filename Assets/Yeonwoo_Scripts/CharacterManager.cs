using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public CharacterStatus characterStatus;
    public string character_id;
    public static CharacterManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CharacterManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("CharacterManager");
                    _instance = obj.AddComponent<CharacterManager>();
                }
            }
            return _instance;
        }
    }
    private static CharacterManager _instance;

}