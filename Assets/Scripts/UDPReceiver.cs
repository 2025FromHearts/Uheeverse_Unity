using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Collections.Generic;

public class UDPReceiver : MonoBehaviour
{
    public int listenPort = 7770;
    public PlayerInputController playerInputController;
    private UdpClient udpClient;
    private Queue<string> messageQueue = new Queue<string>();

    void Start()
    {
        udpClient = new UdpClient(listenPort);
        udpClient.BeginReceive(ReceiveCallback, null);
    }

    private void ReceiveCallback(System.IAsyncResult ar)
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, listenPort);
        byte[] data = udpClient.EndReceive(ar, ref remoteEP);
        string message = Encoding.UTF8.GetString(data);
        Debug.Log($"[UDP ¼ö½Å] {remoteEP.Address}:{remoteEP.Port} ¡æ {message}");
        lock (messageQueue)
        {
            messageQueue.Enqueue(message);
        }
        udpClient.BeginReceive(ReceiveCallback, null);
    }

    void Update()
    {
        while (messageQueue.Count > 0)
        {
            string msg;
            lock (messageQueue)
            {
                msg = messageQueue.Dequeue();
            }
            if (playerInputController != null)
                playerInputController.SetJoypadInput(msg);
        }
    }

    private void OnApplicationQuit()
    {
        udpClient?.Close();
    }
}
