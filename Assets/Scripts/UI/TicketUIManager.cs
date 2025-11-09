using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;

public class TicketUIManager : MonoBehaviour
{
    [Header("UI 연결")]
    public Transform ticketSlotParent;
    public GameObject ticketPrefab;

    [Header("축제별 티켓 이미지(UI) 매핑")]
    public List<FestivalImage> festivalImages; // Inspector에서 등록
    private Dictionary<string, Sprite> imageDict;

    [System.Serializable]
    public class FestivalImage
    {
        public string mapId;       // 서버에서 내려오는 map(UUID) 값
        public Sprite ticketImage; // UI에 표시할 이미지
    }

    [System.Serializable]
    public class TicketData
    {
        public string ticket_id;
        public string character;
        public string map;
        public string serial_number;
        public string issued_at;
    }

    void Awake()
    {
        imageDict = new Dictionary<string, Sprite>();
        foreach (var fi in festivalImages)
        {
            if (!imageDict.ContainsKey(fi.mapId))
                imageDict.Add(fi.mapId, fi.ticketImage);
        }
    }

    // 티켓북 열기
    public void OpenTicketBook()
    {
        Debug.Log("티켓북 열림");
        StartCoroutine(CoLoadTickets());
    }

    // 티켓 목록 불러오기
    IEnumerator CoLoadTickets()
    {
        string token = PlayerPrefs.GetString("access_token", "");
        string url = $"{ServerConfig.baseUrl}/tickets/list/";
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + token);


        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ 티켓 불러오기 실패: " + www.error);
            yield break;
        }

        string json = www.downloadHandler.text;
        Debug.Log($"✅ 티켓 목록 응답: {json}");

        List<TicketData> tickets = JsonConvert.DeserializeObject<List<TicketData>>(json);

        // 기존 슬롯 제거
        foreach (Transform child in ticketSlotParent)
            Destroy(child.gameObject);

        // 불러온 티켓마다 추가
        foreach (var ticket in tickets)
            AddTicket(ticket);
    }

    // 티켓 슬롯 하나 추가
    public void AddTicket(TicketData ticket)
    {
        GameObject obj = Instantiate(ticketPrefab, ticketSlotParent);

        Image ticketImg = obj.transform.Find("Image").GetComponent<Image>();
        TMP_Text serialText = obj.transform.Find("SerialText").GetComponent<TMP_Text>();
        TMP_Text dateText = obj.transform.Find("DateText").GetComponent<TMP_Text>();

        if (ticketImg && imageDict.ContainsKey(ticket.map))
            ticketImg.sprite = imageDict[ticket.map];

        if (serialText)
            serialText.text = ticket.serial_number;

        if (dateText)
        {
            if (System.DateTime.TryParse(ticket.issued_at, out var parsedDate))
            {
                parsedDate = parsedDate.ToLocalTime();
                dateText.text = parsedDate.ToString("yyyy.MM.dd");
            }
            else
            {
                dateText.text = ticket.issued_at;
            }
        }

        obj.name = $"Ticket_{ticket.serial_number}";
    }
}
