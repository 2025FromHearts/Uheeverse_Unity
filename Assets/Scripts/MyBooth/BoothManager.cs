using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class BoothManager : MonoBehaviour
{
    [Header("References")]
    public PlaceableCatalog catalog;
    public Transform boothParent;
    public PlacementController placementController;

    private int version = 0;
    private List<BoothItemData> placedItems = new();

    void Start()
    {
        StartCoroutine(LoadBooth());
        if (placementController != null)
            placementController.OnPlaced += OnItemPlaced;
    }

    // 부스 불러오기
    IEnumerator LoadBooth()
    {
        string token = PlayerPrefs.GetString("access_token", "");
        string characterId = PlayerPrefs.GetString("character_id", "");
        string url = $"{ServerConfig.baseUrl}/booth/{characterId}/";

        UnityWebRequest req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", "Bearer " + token);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ 부스 로드 실패: " + req.error);
            yield break;
        }

        BoothResponse booth = JsonConvert.DeserializeObject<BoothResponse>(req.downloadHandler.text);
        version = booth.version;

        foreach (var item in booth.items)
        {
            var entry = catalog.Find(item.item);
            if (entry == null || entry.prefab == null)
            {
                Debug.LogWarning($"⚠️ '{item.item}'을(를) Catalog에서 찾을 수 없습니다.");
                continue;
            }

            Vector3 pos = new(item.pos_x, item.pos_y, item.pos_z);
            Quaternion rot = Quaternion.Euler(item.rot_x, item.rot_y, item.rot_z);
            Vector3 savedScale = new(item.scale_x, item.scale_y, item.scale_z);

            var go = Instantiate(entry.prefab, pos, rot, boothParent);

            bool isBoothSpace = entry.key.ToLower().Contains("space") ||
                                entry.key.ToLower().Contains("boothbase") ||
                                entry.key.ToLower().Contains("floor") ||
                                entry.key.ToLower().Contains("zone");

            if (!isBoothSpace)
            {
                // 실제 월드 스케일 기반 보정 (1:1 유지 필요)
                Vector3 currentWorld = go.transform.lossyScale;
                float sx = currentWorld.x == 0 ? 1e-6f : currentWorld.x;
                float sy = currentWorld.y == 0 ? 1e-6f : currentWorld.y;
                float sz = currentWorld.z == 0 ? 1e-6f : currentWorld.z;
                Vector3 factor = new Vector3(
                    savedScale.x / sx,
                    savedScale.y / sy,
                    savedScale.z / sz
                );
                go.transform.localScale = Vector3.Scale(go.transform.localScale, factor);

                Debug.Log(
                    $"📐 '{item.item}' 월드 스케일 보정 | parent={go.transform.parent.lossyScale:F2} | " +
                    $"saved={savedScale} | before={currentWorld} | after={go.transform.lossyScale}"
                );
            }
            else
            {
                go.transform.localScale = Vector3.one;
            }

            Debug.Log($"🧱 아이템 로드됨: {item.item} @ {pos}");

            // 기존 서버 데이터도 placedItems에 포함시켜 저장 대상에 유지
            BoothItemData loaded = new BoothItemData
            {
                booth_item_id = item.booth_item_id,
                item = item.item,
                inventory = item.inventory,
                pos_x = item.pos_x,
                pos_y = item.pos_y,
                pos_z = item.pos_z,
                rot_x = item.rot_x,
                rot_y = item.rot_y,
                rot_z = item.rot_z,
                scale_x = item.scale_x,
                scale_y = item.scale_y,
                scale_z = item.scale_z,
                variant = item.variant,
                ordering = item.ordering,
                locked = item.locked,
                meta_json = item.meta_json
            };
            placedItems.Add(loaded);
        }

        Debug.Log($"✅ 부스 로드 완료, 기존 아이템 {placedItems.Count}개 포함");
    }

    // 새로 아이템 배치 시 리스트에 추가 저장
    void OnItemPlaced(PlacementController.PlacementResult result)
    {
        var placedObj = result.placedObject;
        Vector3 localScale = placedObj.transform.localScale; // 부모 스케일 영향을 무시하기 위함

        BoothItemData item = new()
        {
            booth_item_id = null,
            item = result.key,
            inventory = null,
            pos_x = result.position.x,
            pos_y = result.position.y,
            pos_z = result.position.z,
            rot_x = 0f,
            rot_y = result.rotY,
            rot_z = 0f,
            scale_x = localScale.x,
            scale_y = localScale.y,
            scale_z = localScale.z,
            variant = null,
            ordering = 0,
            locked = false,
            meta_json = "{}"
        };

        placedItems.Add(item);
    }

    // 나가기 버튼 누르면 서버에 저장
    public void SaveAndExit()
    {
        StartCoroutine(SaveBoothCoroutine());
    }

    IEnumerator SaveBoothCoroutine()
    {
        string token = PlayerPrefs.GetString("access_token", "");
        string characterId = PlayerPrefs.GetString("character_id", "");
        string url = $"{ServerConfig.baseUrl}/booth/{characterId}/save/";

        BoothSavePayload payload = new()
        {
            version = version,
            items = placedItems
        };

        string json = JsonConvert.SerializeObject(payload);
        Debug.Log($"저장 요청 JSON: {json}");

        UnityWebRequest req = new UnityWebRequest(url, "PUT");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + token);

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
            Debug.LogError($"❌ 부스 저장 실패: {req.error} (HTTP {req.responseCode})");
        else
            Debug.Log("✅ 부스 저장 완료");
    }

    // 데이터 구조체
    
    [System.Serializable]
    public class BoothResponse
    {
        public string booth_id;
        public int version;
        public List<BoothItemData> items;
    }

    [System.Serializable]
    public class BoothItemData
    {
        public string booth_item_id;
        public string item;
        public string inventory;

        public float pos_x;
        public float pos_y;
        public float pos_z;

        public float rot_x;
        public float rot_y;
        public float rot_z;

        public float scale_x;
        public float scale_y;
        public float scale_z;

        public string variant;
        public int ordering;
        public bool locked;
        public string meta_json;
    }

    [System.Serializable]
    public class BoothSavePayload
    {
        public int version;
        public List<BoothItemData> items;
    }
}
