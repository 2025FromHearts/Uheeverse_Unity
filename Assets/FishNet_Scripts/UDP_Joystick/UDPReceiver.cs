using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using FishNet.Managing;
using FishNet.Object;

public class UDPReceiver : MonoBehaviour
{
    private UdpClient udpClient;
    private Thread receiveThread;
    public int listenPort = 8887;
    [SerializeField] private FishNet.Managing.NetworkManager networkManager;

    void Start()
    {
        udpClient = new UdpClient(listenPort);
        receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void ReceiveData()
    {
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, listenPort);
        while (true)
        {
            byte[] data = udpClient.Receive(ref endPoint);
            string command = Encoding.UTF8.GetString(data);

            UnityMainThreadDispatcher.Enqueue(() =>
            {
                foreach (var kvp in networkManager.ServerManager.Clients)
                {
                    var conn = kvp.Value;
                    if (conn.FirstObject != null)
                    {
                        var player = conn.FirstObject.GetComponent<RemotePlayerController>();
                        if (player != null)
                        {
                            player.SetMoveCommandServerRpc(command);
                        }
                    }
                }
            });
        }
    }

    void OnApplicationQuit()
    {
        udpClient?.Close();
        receiveThread?.Abort();
    }
}
