using UnityEngine;
// FishNet 쓰면 아래처럼 바꾸면 됨
// using FishNet.Object;

public class PlayerFromPad : MonoBehaviour // : NetworkBehaviour
{
    [Header("연결할 컴포넌트")]
    public PlayerInputController inputController; // CharacterRoot의 PlayerInputController
    public UdpPadReceiver pad;                    // JoypadReceiver 오브젝트

    [Header("입력 옵션")]
    public float deadZone = 0.1f;                 // 너무 작은 입력은 0으로 처리

    void Awake()
    {
        // 같은 오브젝트에 PlayerInputController가 붙어 있으면 자동으로 가져오기
        if (inputController == null)
            inputController = GetComponent<PlayerInputController>();
    }

    void Update()
    {
        // FishNet 쓰면 내 캐릭터만 입력 받도록:
        // if (!IsOwner) return;

        if (pad == null || pad.latest == null || inputController == null)
            return;

        PadState s = pad.latest;

        // 조이스틱 입력 (JoyPad에서 온 값, -1 ~ 1)
        Vector2 padInput = new Vector2(s.lx, s.ly);

        // 너무 작은 값은 노이즈로 보고 0 처리
        if (padInput.magnitude < deadZone)
            padInput = Vector2.zero;
        else
            padInput = Vector2.ClampMagnitude(padInput, 1f);

        // PlayerInputController에 외부 입력 사용하도록 설정
        inputController.useExternalInput = true;
        inputController.externalMoveInput = padInput;
    }
}
