using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class PadCommandSender : MonoBehaviour
{
    [Header("UDP Target (Joypad App)")]
    public string targetIp = "127.0.0.1";   
    public int targetPort = 9001; 
    UdpClient client;

    void Awake()
    {
        client = new UdpClient();
    }

    public void SendOpenTextInput(string target)
    {
        string json = $"{{\"type\":\"open_text_input\",\"target\":\"{target}\"}}";
        Send(json);
        Debug.Log("[PadCommandSender] open_text_input sent");
    }

    public void SendCloseTextInput()
    {
        string json = "{\"type\":\"close_text_input\"}";
        Send(json);
        Debug.Log("[PadCommandSender] close_text_input sent");
    }

    void Send(string json)
    {
        byte[] data = Encoding.UTF8.GetBytes(json);
        client.Send(data, data.Length, targetIp, targetPort);
    }

    void OnDestroy()
    {
        client?.Close();
    }
}
