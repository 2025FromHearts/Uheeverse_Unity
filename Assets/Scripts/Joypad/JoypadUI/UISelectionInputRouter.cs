using System;
using UnityEngine;

public class UISelectionInputRouter : MonoBehaviour
{
    public UdpPadReceiver pad;

    [Header("¼³Á¤")]
    public float moveThreshold = 0.5f;
    public float repeatDelay = 0.25f;

    float lastMoveTime;

    public event Action OnMoveUp;
    public event Action OnMoveDown;
    public event Action OnMoveLeft;
    public event Action OnMoveRight;

    void Update()
    {
        if (pad == null || pad.latest == null) return;

        Vector2 dir = new Vector2(pad.latest.lx, pad.latest.ly);

        if (Time.time - lastMoveTime < repeatDelay)
            return;

        if (dir.y > moveThreshold)
        {
            OnMoveUp?.Invoke();
            lastMoveTime = Time.time;
        }
        else if (dir.y < -moveThreshold)
        {
            OnMoveDown?.Invoke();
            lastMoveTime = Time.time;
        }
        else if (dir.x < -moveThreshold)
        {
            OnMoveLeft?.Invoke();
            lastMoveTime = Time.time;
        }
        else if (dir.x > moveThreshold)
        {
            OnMoveRight?.Invoke();
            lastMoveTime = Time.time;
        }
    }
}
