using UnityEngine;

public class UISoundManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip shutterClip;

    public void PlayShutter()
    {
        audioSource.PlayOneShot(shutterClip);
    }
}
