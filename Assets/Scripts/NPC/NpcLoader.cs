//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Networking;

//[System.Serializable]
//public class NpcData
//{
//    public string npc_id;
//    public string npc_name;
//    public string npc_type;
//    public string scene_name;
//    public string base_prompt;
//}

//[System.Serializable]
//public class NpcListWrapper
//{
//    public List<NpcData> npcs;
//}

//public class NpcLoader : MonoBehaviour
//{
//    public string mapId;
//    private string BASE_URL;

//    public GameObject guidePrefab;
//    public GameObject minigamePrefab;
//    public GameObject vendorPrefab;

//    public Transform[] spawnPoints;
//    private NpcGameManager gameManager;
//    private NpcShopManager shopManager;

//    void Start()
//    {
//        gameManager = FindObjectOfType<NpcGameManager>();
//        shopManager = FindObjectOfType<NpcShopManager>();

//        StartCoroutine(LoadNpcs());
//    }
//    IEnumerator LoadNpcs()
//    {
//        BASE_URL = ServerConfig.baseUrl;
//        string url = BASE_URL + "/map/npc/list/" + mapId + "/";
//        UnityWebRequest www = UnityWebRequest.Get(url);
//        yield return www.SendWebRequest();

//        if (www.result != UnityWebRequest.Result.Success)
//        {
//            Debug.LogError("❌ NPC 불러오기 실패: " + www.error);
//            yield break;
//        }

//        string rawJson = www.downloadHandler.text;
//        string wrappedJson = "{\"npcs\":" + rawJson + "}";
//        NpcListWrapper npcList = JsonUtility.FromJson<NpcListWrapper>(wrappedJson);

//        for (int i = 0; i < npcList.npcs.Count; i++)
//        {
//            if (i >= spawnPoints.Length)
//            {
//                Debug.LogWarning("⚠️ NPC의 개수가 spawnPoints 개수보다 더 많음. 배치에 오류 있음.");
//                break;
//            }

//            NpcData npc = npcList.npcs[i];
//            GameObject prefab = GetPrefabByType(npc.npc_type);
//            if (prefab == null)
//            {
//                Debug.LogWarning($"⚠️ Unknown NPC type: {npc.npc_type}");
//                continue;
//            }

//            GameObject npcObj = Instantiate(prefab, spawnPoints[i].position, spawnPoints[i].rotation);
//            npcObj.name = npc.npc_name;

//            var interact = npcObj.GetComponent<NpcInteract>();
//            if (interact != null)
//            {
//                interact.SetNpcData(npc);

//                // 🔽 매니저 직접 연결
//                interact.npcGameManager = gameManager;
//                interact.npcShopManager = shopManager;
//            }
//            else
//            {
//                Debug.LogWarning("⚠️ NpcInteract 컴포넌트가 프리팹에 없으니 확인.");
//            }
//        }
//    }

//    GameObject GetPrefabByType(string type)
//    {
//        switch (type.ToLower())
//        {
//            case "guide": return guidePrefab;
//            case "minigame": return minigamePrefab;
//            case "vendor": return vendorPrefab;
//            default: return null;
//        }
//    }
//}
