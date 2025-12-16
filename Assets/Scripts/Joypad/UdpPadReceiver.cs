using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UdpPadReceiver : MonoBehaviour
{
    [Header("UDP 설정")]
    public int listenPort = 9000;      // JoyPad 쪽 Target Port와 동일

    [Header("현재 패드 상태 (디버그용)")]
    public PadState latest = new PadState();
    public bool isReceiving = false;

    UdpClient client;
    Thread thread;
    bool running;

    void Start()
    {
        try
        {
            client = new UdpClient(listenPort);
            running = true;
            thread = new Thread(ReceiveLoop);
            thread.IsBackground = true;
            thread.Start();

            Debug.Log($"[UdpPadReceiver] Listening on {listenPort}");
        }
        catch (Exception e)
        {
            Debug.LogError("[UdpPadReceiver] Start failed: " + e.Message);
        }
    }

    void ReceiveLoop()
    {
        IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);

        while (running)
        {
            try
            {
                byte[] data = client.Receive(ref remote);
                string json = Encoding.UTF8.GetString(data);

                PadState s = JsonUtility.FromJson<PadState>(json);
                if (s != null)
                {
                    latest = s;
                    isReceiving = true;
                }
            }
            catch (Exception e)
            {
                if (running)
                    Debug.LogWarning("[UdpPadReceiver] " + e.Message);
            }
        }
    }

    void OnDestroy()
    {
        running = false;

        try { client?.Close(); } catch { }

        if (thread != null && thread.IsAlive)
        {
            try { thread.Abort(); } catch { }
        }
    }
}
