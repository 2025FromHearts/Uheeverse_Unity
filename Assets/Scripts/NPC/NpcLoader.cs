using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class NpcData
{
    public string npc_id;
    public string npc_name;
    public string npc_type;
    public string scene_name;
    public string base_prompt;
}

[System.Serializable]
public class NpcListWrapper
{
    public List<NpcData> npcs;
}

public class NpcLoader : MonoBehaviour
{
    public string mapId;
    private string BASE_URL;

    [Header("NPC Prefabs")]
    public GameObject guidePrefab;
    public GameObject minigamePrefab;
    public GameObject vendorPrefab;
    public GameObject photoPrefab;

    [Header("배치 위치")]
    public Transform[] spawnPoints;

    private NpcGameManager gameManager;
    private NpcShopManager shopManager;
    private NpcPhotoManager photoManager;

    void Start()
    {
        gameManager = FindObjectOfType<NpcGameManager>();
        shopManager = FindObjectOfType<NpcShopManager>();
        photoManager = FindObjectOfType<NpcPhotoManager>();

        StartCoroutine(LoadNpcs());
    }


    IEnumerator LoadNpcs()
    {
        BASE_URL = ServerConfig.baseUrl;
        string url = BASE_URL + "/map/npc/list/" + mapId + "/";
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ NPC 불러오기 실패: " + www.error);
            yield break;
        }

        string rawJson = www.downloadHandler.text;
        string wrappedJson = "{\"npcs\":" + rawJson + "}";
        NpcListWrapper npcList = JsonUtility.FromJson<NpcListWrapper>(wrappedJson);

        for (int i = 0; i < npcList.npcs.Count; i++)
        {
            if (i >= spawnPoints.Length)
            {
                Debug.LogWarning("⚠️ NPC의 개수가 spawnPoints 개수보다 많음. 배치에 오류 있음.");
                break;
            }

            NpcData npc = npcList.npcs[i];
            GameObject prefab = GetPrefabByType(npc.npc_type);
            if (prefab == null)
            {
                Debug.LogWarning($"⚠️ Unknown NPC type: {npc.npc_type}");
                continue;
            }

            GameObject npcObj = Instantiate(prefab, spawnPoints[i].position, spawnPoints[i].rotation);
            npcObj.name = npc.npc_name;
            NpcTalkTracker.Instance?.RegisterNpc(npc.npc_id);

            var interact = npcObj.GetComponent<NpcInteract>();
            if (interact != null)
            {
                // JSON 데이터로 NPC 세팅
                interact.SetNpcData(npc);

                // 매니저 연결
                interact.npcGameManager = gameManager;
                interact.npcShopManager = shopManager;
                interact.npcPhotoManager = photoManager;
            }
            else
            {
                Debug.LogWarning("⚠️ NpcInteract 컴포넌트가 프리팹에 없음.");
            }
        }

        int trackableCount = npcList.npcs.Count;
        if (NpcTalkTracker.Instance != null)
            NpcTalkTracker.Instance.SetRequiredCount(trackableCount);
    }

    GameObject GetPrefabByType(string type)
    {
        switch (type.ToLower())
        {
            case "guide": return guidePrefab;
            case "minigame": return minigamePrefab;
            case "vendor": return vendorPrefab;
            case "photo": return photoPrefab;
            default: return null;
        }
    }
}
