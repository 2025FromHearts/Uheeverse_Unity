using UnityEngine;

public class JoypadToPlayerMove : MonoBehaviour
{
    public UdpPadReceiver pad;
    public PlayerMove player;

    void Update()
    {
        if (pad == null || pad.latest == null || player == null) return;

        player.useJoypad = true;
        player.joypadInput = new Vector2(
            pad.latest.lx,
            pad.latest.ly
        );
    }
}
