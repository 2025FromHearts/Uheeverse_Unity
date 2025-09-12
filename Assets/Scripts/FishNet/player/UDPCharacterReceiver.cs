using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEditor.Animations;

public class UDPCharacterReceiver : MonoBehaviour
{
    [Header("UDP 설정")]
    public int listenPort = 8889;
    
    [Header("캐릭터 설정")]
    public float speed = 5f;
    
    [Header("UDP 입력값 (디버그용)")]
    public Vector2 udpInput = Vector2.zero;
    public bool isReceiving = false;
    
    // 컴포넌트들
    private CharacterController control;

    private Rigidbody rigid;
    private Animator anim;
    private Vector3 moveVec;

    Vector3 movedirection;
    
    // UDP 관련
    private UdpClient udpClient;
    private Thread receiveThread;
    private bool shouldReceive = true;
    
    void Awake()
    {
        // 캐릭터 컴포넌트 가져오기
        rigid = GetComponent<Rigidbody>();
        control = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        
        if (control == null)
        {
            Debug.LogError("Rigidbody 컴포넌트가 필요합니다!");
        }
    }
    
    void Start()
    {
        StartUDPServer();
    }
    
    void StartUDPServer()
    {
        try
        {
            udpClient = new UdpClient(listenPort);
            receiveThread = new Thread(ReceiveData);
            receiveThread.IsBackground = true;
            shouldReceive = true;
            receiveThread.Start();
            
            Debug.Log($"UDP 서버 시작: 포트 {listenPort}에서 대기 중...");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UDP 서버 시작 실패: {e.Message}");
        }
    }
    
    private void ReceiveData()
    {
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, listenPort);
        
        while (shouldReceive)
        {
            try
            {
                // UDP 패킷 받기
                byte[] data = udpClient.Receive(ref endPoint);
                string command = Encoding.UTF8.GetString(data);
                
                // 메인 스레드에서 처리하도록 예약
                UnityMainThreadDispatcher.Enqueue(() => {
                    ProcessUDPCommand(command);
                });
            }
            catch (System.Exception e)
            {
                if (shouldReceive) // 정상 종료가 아닌 경우만 에러 출력
                {
                    Debug.LogError($"UDP 수신 에러: {e.Message}");
                }
                break;
            }
        }
    }

    void ProcessUDPCommand(string command)
    {
        isReceiving = true;

        if (command == "STOP")
        {
            udpInput = Vector2.zero;
            Debug.Log("정지 명령 받음");
            return;
        }

        if (command.StartsWith("MOVE,"))
        {
            string[] parts = command.Split(',');
            if (parts.Length >= 3)
            {
                if (float.TryParse(parts[1], out float x) && float.TryParse(parts[2], out float z))
                {
                    udpInput = new Vector2(x, z);
                    Debug.Log($"움직임 명령 받음: H={x:F2}, V={z:F2}");
                }
            }
        }
    }

    void Update()
    {
        if (control == null) return;

        float x = udpInput.x;
        float z = udpInput.y;

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        movedirection = (forward * z) + (right * x);

        // movedirection = new Vector3(x, 0, z);

        control.Move(movedirection * speed * Time.deltaTime);

        // moveVec = new Vector3(x, 0, z) * speed * Time.fixedDeltaTime;
        // rigid.MovePosition(rigid.position + moveVec);

        if (movedirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movedirection); // 이 줄 추가
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1.5f * Time.deltaTime);
        }
    
        moveVec = movedirection;

        // Quaternion dirQuat = Quaternion.LookRotation(moveVec);
        // Quaternion moveQuat = Quaternion.Slerp(rigid.rotation, dirQuat, 0.3f);
        // rigid.MoveRotation(moveQuat);
    }


    
    void LateUpdate()
    {
        // 애니메이터가 있으면 움직임 애니메이션 설정
        if (anim != null)
        {
            anim.SetFloat("Move", moveVec.sqrMagnitude);
        }
    }
    
    // UDP 서버 종료
    void StopUDPServer()
    {
        shouldReceive = false;
        
        if (udpClient != null)
        {
            udpClient.Close();
            udpClient.Dispose();
        }
        
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Join(1000); // 1초 대기
            if (receiveThread.IsAlive)
            {
                receiveThread.Abort();
            }
        }
        
        Debug.Log("UDP 서버 종료");
    }
    
    void OnDestroy()
    {
        StopUDPServer();
    }
    
    void OnApplicationQuit()
    {
        StopUDPServer();
    }
    
    // 디버그용 UI
    void OnGUI()
    {
        GUI.Label(new Rect(10, 100, 200, 20), $"UDP Input: {udpInput}");
        GUI.Label(new Rect(10, 120, 200, 20), $"Receiving: {isReceiving}");
        GUI.Label(new Rect(10, 140, 200, 20), $"Listen Port: {listenPort}");
        GUI.Label(new Rect(10, 160, 200, 20), $"Move Speed: {udpInput.magnitude:F2}");
        GUI.Label(new Rect(10, 180, 200, 20), $"Direction: H={udpInput.x:F2}, V={udpInput.y:F2}");
    }
}