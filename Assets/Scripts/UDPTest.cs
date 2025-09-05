using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPTest : MonoBehaviour
{
    public string targetIp = "172.20.1.122"; // PC 자체 테스트 → 이후 실제 PC IP로 변경
    public int targetPort = 8888;

    UdpClient sender;

    void Start()
    {
        sender = new UdpClient(AddressFamily.InterNetwork); // IPv4
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Send("hello");
        }
    }

    public async void Send(string text)
    {
        try
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            await sender.SendAsync(bytes, bytes.Length, targetIp, targetPort);
            Debug.Log($"[UDPSenderOnce] 송신 → {targetIp}:{targetPort} ({bytes.Length}B) \"{text}\"");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[UDPSenderOnce] 송신 에러: " + e);
        }
    }

    void OnApplicationQuit()
    {
        sender?.Close();
    }
}
