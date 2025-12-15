using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using TMPro;

public class PadTextReceiver : MonoBehaviour
{
    public int listenPort = 9001;          // 조이패드가 보내는 포트
    public TMP_InputField nicknameInput;   // 실제 반영할 InputField

    UdpClient client;
    Thread thread;
    bool running;

    string receivedText;
    bool hasNewText;

    void Start()
    {
        client = new UdpClient(listenPort);
        running = true;

        thread = new Thread(ReceiveLoop);
        thread.IsBackground = true;
        thread.Start();

        Debug.Log("[PadTextReceiver] START");
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

                if (json.Contains("submit_text"))
                {
                    int idx = json.IndexOf("\"value\":\"") + 9;
                    int end = json.LastIndexOf("\"");
                    receivedText = json.Substring(idx, end - idx);
                    hasNewText = true;

                    Debug.Log("[PadTextReceiver] RECEIVED TEXT: " + receivedText);
                }
            }
            catch { }
        }
    }

    void Update()
    {
        if (!hasNewText) return;

        hasNewText = false;

        if (nicknameInput != null)
        {
            nicknameInput.text = receivedText;
            nicknameInput.caretPosition = receivedText.Length;
        }
    }

    void OnDestroy()
    {
        running = false;
        client?.Close();
    }
}
