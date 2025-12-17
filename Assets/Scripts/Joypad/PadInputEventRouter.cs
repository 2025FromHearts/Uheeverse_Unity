using System;
using UnityEngine;

public class PadInputEventRouter : MonoBehaviour
{
    public UdpPadReceiver pad;

    bool prevA, prevB, prevX, prevY, prevL, prevR;
    bool prevPlus, prevMinus;

    public event Action OnAPressed;
    public event Action OnBPressed;
    public event Action OnXPressed;
    public event Action OnYPressed;
    public event Action OnPlusPressed;
    public event Action OnMinusPressed;
    public event Action OnLPressed;
    public event Action OnRPressed;


    public enum InputMode
    {
        Player,
        Placement,
        UPhone,
        Ticket,
        Gallery,
        Popup,
        Dialogue
    }

    public InputMode currentMode = InputMode.Player;

    void Update()
    {

        if (pad == null || pad.latest == null) return;

        PadState s = pad.latest;

        // A
        if (!prevA && s.a)
        {
            Debug.Log($"[Router] A pressed, mode={currentMode}");
            if (AllowA())
                OnAPressed?.Invoke();
            else
                Debug.Log("[Router] A blocked by AllowA()");
        }

        // B
        if (!prevB && s.b && AllowB())
            OnBPressed?.Invoke();

        // X

        if (!prevX && s.x && AllowX())
            OnXPressed?.Invoke();

        // Y
        if (!prevY && s.y && AllowY())
            OnYPressed?.Invoke();

        // L / R
        if (!prevL && s.l && AllowL())
            OnLPressed?.Invoke();

        if (!prevR && s.r && AllowR())
            OnRPressed?.Invoke();

        // Plus / Minus
        if (!prevPlus && s.plus)
            OnPlusPressed?.Invoke();

        if (!prevMinus && s.minus)
            OnMinusPressed?.Invoke();

        Cache(s);
    }


    bool AllowA()
    {
        // 팝업 떠 있으면 A는 팝업 전용으로만 처리
        if (currentMode == InputMode.Popup)
            return true;

        // 일반 UI / 플레이어 상태
        return currentMode == InputMode.Player
            || currentMode == InputMode.UPhone
            || currentMode == InputMode.Dialogue
            || currentMode == InputMode.Placement;
    }

    bool AllowB()
    {
        return currentMode == InputMode.UPhone
            || currentMode == InputMode.Gallery
            || currentMode == InputMode.Popup
            || currentMode == InputMode.Placement;

    }

    bool AllowX()
    {
        return currentMode == InputMode.UPhone
            || currentMode == InputMode.Player
            || currentMode == InputMode.Gallery
            || currentMode == InputMode.Popup
            || currentMode == InputMode.Dialogue
            || currentMode == InputMode.Placement;
    }

    bool AllowY()
    {
        return currentMode == InputMode.Player
            || currentMode == InputMode.Popup
            || currentMode == InputMode.Dialogue
            || currentMode == InputMode.Placement;
    }

    bool AllowL()
    {
        return true;
    }

    bool AllowR()
    {
        return true;
    }

    void Cache(PadState s)
    {
        prevA = s.a;
        prevB = s.b;
        prevX = s.x;
        prevY = s.y;
        prevL = s.l;
        prevR = s.r;
        prevPlus = s.plus;
        prevMinus = s.minus;
    }
}
