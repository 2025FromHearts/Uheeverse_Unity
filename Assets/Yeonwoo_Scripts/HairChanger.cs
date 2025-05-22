using UnityEngine;

public class HairChanger : MonoBehaviour
{
    public GameObject[] hairStyles;
    private int currentIndex = 0;

    public void SetHairStyle(int index)
    {
        for (int i = 0; i < hairStyles.Length; i++)
            hairStyles[i].SetActive(i == index);

        currentIndex = index;
    }
}