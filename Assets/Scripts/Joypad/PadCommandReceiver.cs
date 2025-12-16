using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class PadCommandReceiver : MonoBehaviour
{
    public int listenPort = 9001;

    // ===== 외부(UI)에서 구독할 이벤트 =====
    public static event Action OnOpenTextInput;
    public static event Action OnCloseTextInput;
    public static event Action<string> OnSubmitText;

    UdpClient client;
    Thread thread;
    bool running;

    // 스레드 → 메인 스레드 전달용
    bool requestOpen;
    bool requestClose;
    string submittedText;
    bool requestSubmit;

    void Start()
    {
        Debug.Log("[PadCommandReceiver] START, listenPort = " + listenPort);

        client = new UdpClient(listenPort);
        running = true;

        thread = new Thread(ReceiveLoop);
        thread.IsBackground = true;
        thread.Start();
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

                Debug.Log("[PadCommandReceiver] UDP RECEIVED: " + json);

                if (json.Contains("\"type\":\"open_text_input\""))
                {
                    requestOpen = true;
                }
                else if (json.Contains("\"type\":\"close_text_input\""))
                {
                    requestClose = true;
                }
                else if (json.Contains("\"type\":\"submit_text\""))
                {
                    submittedText = ExtractValue(json);
                    requestSubmit = true;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[PadCommandReceiver] Receive error: " + e.Message);
            }
        }
    }

    void Update()
    {
        if (requestOpen)
        {
            requestOpen = false;
            Debug.Log("[PadCommandReceiver] EVENT: OpenTextInput");
            OnOpenTextInput?.Invoke();
        }

        if (requestClose)
        {
            requestClose = false;
            Debug.Log("[PadCommandReceiver] EVENT: CloseTextInput");
            OnCloseTextInput?.Invoke();
        }

        if (requestSubmit)
        {
            requestSubmit = false;
            Debug.Log("[PadCommandReceiver] EVENT: SubmitText = " + submittedText);
            OnSubmitText?.Invoke(submittedText);
        }
    }

    string ExtractValue(string json)
    {
        // {"type":"submit_text","value":"캐릭터"}
        const string key = "\"value\":\"";
        int start = json.IndexOf(key);
        if (start < 0) return "";

        start += key.Length;
        int end = json.IndexOf("\"", start);
        if (end < 0) return "";

        return json.Substring(start, end - start);
    }

    void OnDestroy()
    {
        running = false;
        try { client?.Close(); } catch { }
    }
}
