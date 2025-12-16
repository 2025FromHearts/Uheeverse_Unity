using System;

[Serializable]
public class PadState
{
    public float lx;   // 조이스틱 X (-1~1)
    public float ly;   // 조이스틱 Y (-1~1)

    public bool a, b, x, y;
    public bool l, r;
    public bool plus, minus;

    public int page;   // 0: 기본 조작, 1: UI, 2: 미니게임 ... 이런 식으로
}
