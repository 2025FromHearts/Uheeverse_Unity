using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.TextCore.Text;

[System.Serializable]
public class NpcData
{
    public string npc_id;
    public string npc_name;
    public string npc_type;
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
    public GameObject guidePrefab;
    public GameObject minigamePrefab;
    public GameObject vendorPrefab;
    public Transform[] spawnPoints; // 사전 위치, 빈 오브젝트 활용

    void Start()
    {
        StartCoroutine(LoadNpcs());
    }

    // Update is called once per frame
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

        // JSON 배열 -> 수동으로 래핑 필요
        string wrappedJson = "{\"npcs\":" + rawJson + "}";
        NpcListWrapper npcList = JsonUtility.FromJson<NpcListWrapper>(wrappedJson);

        for (int i = 0; i < npcList.npcs.Count; i++)
        {
            NpcData npc = npcList.npcs[i];

            GameObject prefab = GetPrefabByType(npc.npc_type);
            if (prefab == null || i >= spawnPoints.Length)
                continue;

            GameObject npcObj = Instantiate(prefab, spawnPoints[i].position, spawnPoints[i].rotation);
            npcObj.name = npc.npc_name;

            var interact = npcObj.AddComponent<NpcInteract>();
            interact.npcId = npc.npc_id;
            interact.npcName = npc.npc_name;
        }
    }

    GameObject GetPrefabByType(string type)
    {
        switch (type)
        {
            case "guide": return guidePrefab;
            case "minigame": return minigamePrefab;
            case "vendor": return vendorPrefab;
            default: return null;
        }
    }

}
