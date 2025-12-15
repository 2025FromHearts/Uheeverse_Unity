using UnityEngine;

public class KartJoypadInput : MonoBehaviour
{
    public UdpPadReceiver pad;
    public KartController kart;

    void Update()
    {
        if (pad == null || pad.latest == null) return;

        kart.useJoypad = true;
        kart.joypadMoveInput = new Vector2(
            pad.latest.lx,
            pad.latest.ly
        );
    }
}
