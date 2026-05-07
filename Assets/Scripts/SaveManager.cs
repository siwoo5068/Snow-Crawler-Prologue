using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    private string SavePath => System.IO.Path.Combine(Application.persistentDataPath, "save.json");

    [Header("Scene References (비워두면 자동 탐색)")]
    public SurvivalTimer survivalTimer;
    public SortieManager sortieManager;

    [Header("Furniture Tag")]
    public string furnitureTag = "FurnitureItem";

    [Serializable]
    public class FurnitureSaveData
    {
        public string itemTypeName;
        public float posX, posY, posZ;
        public float rotX, rotY, rotZ, rotW;
        public float scaleX, scaleY, scaleZ;
    }

    [Serializable]
    public class GameSaveData
    {
        public List<FurnitureSaveData> placedFurniture = new List<FurnitureSaveData>();
        public int   upgradeLevel   = 0;
        public int   materialCount  = 0;
        public float maxTime        = 30f;
        public float coldMultiplier = 1f;
        public int   sortieCount    = 0;
        public string savedAt       = "";
        public int   saveVersion    = 1;
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (survivalTimer == null) survivalTimer = FindFirstObjectByType<SurvivalTimer>();
        if (sortieManager == null) sortieManager = FindFirstObjectByType<SortieManager>();
        Debug.Log($"[SaveManager] 초기화 완료. 저장 경로: {SavePath}");
    }

    public void SaveGame()
    {
        var data = new GameSaveData();
        data.savedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        data.placedFurniture = BuildFurnitureList();
        if (survivalTimer != null)
        {
            data.upgradeLevel   = survivalTimer.upgradeLevel;
            data.materialCount  = survivalTimer.materialCount;
            data.maxTime        = survivalTimer.maxTime;
            data.coldMultiplier = survivalTimer.coldMultiplier;
        }
        if (sortieManager != null) data.sortieCount = sortieManager.SortieCount;
        try
        {
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            System.IO.File.WriteAllText(SavePath, json);
            Debug.Log($"[SaveManager] 저장 완료: 가구 {data.placedFurniture.Count}개 | Lv.{data.upgradeLevel} | 출격 {data.sortieCount}회");
        }
        catch (Exception e) { Debug.LogError($"[SaveManager] 저장 실패: {e.Message}"); }
    }

    public void LoadGame()
    {
        if (!System.IO.File.Exists(SavePath)) { Debug.Log("[SaveManager] 저장 파일 없음"); return; }
        try
        {
            string json = System.IO.File.ReadAllText(SavePath);
            var data = JsonUtility.FromJson<GameSaveData>(json);
            if (data == null) { Debug.LogError("[SaveManager] 파싱 실패"); return; }
            if (survivalTimer != null)
            {
                survivalTimer.upgradeLevel   = data.upgradeLevel;
                survivalTimer.materialCount  = data.materialCount;
                survivalTimer.maxTime        = data.maxTime;
                survivalTimer.coldMultiplier = data.coldMultiplier;
            }
            if (sortieManager != null) sortieManager.SortieCount = data.sortieCount;
            RespawnFurniture(data.placedFurniture);
            Debug.Log($"[SaveManager] 불러오기 완료: 가구 {data.placedFurniture.Count}개 | Lv.{data.upgradeLevel}");
        }
        catch (Exception e) { Debug.LogError($"[SaveManager] 불러오기 실패: {e.Message}"); }
    }

    public void DeleteSave() { if (System.IO.File.Exists(SavePath)) { System.IO.File.Delete(SavePath); Debug.Log("[SaveManager] 저장 파일 삭제"); } }
    public bool HasSaveFile() => System.IO.File.Exists(SavePath);

    private List<FurnitureSaveData> BuildFurnitureList()
    {
        var result = new List<FurnitureSaveData>();
        GameObject[] objs = GameObject.FindGameObjectsWithTag(furnitureTag);
        foreach (var go in objs)
        {
            var item = go.GetComponent<FurnitureItem>();
            if (item == null) continue;
            result.Add(new FurnitureSaveData
            {
                itemTypeName = item.itemType.ToString(),
                posX = go.transform.position.x, posY = go.transform.position.y, posZ = go.transform.position.z,
                rotX = go.transform.rotation.x, rotY = go.transform.rotation.y, rotZ = go.transform.rotation.z, rotW = go.transform.rotation.w,
                scaleX = go.transform.localScale.x, scaleY = go.transform.localScale.y, scaleZ = go.transform.localScale.z,
            });
        }
        return result;
    }

    private void RespawnFurniture(List<FurnitureSaveData> list)
    {
        if (list == null || list.Count == 0) return;
        foreach (var entry in list)
        {
            if (!Enum.TryParse<ItemType>(entry.itemTypeName, out ItemType itemType)) { Debug.LogWarning($"[SaveManager] 알 수 없는 ItemType: {entry.itemTypeName}"); continue; }
            GameObject prefab = LoadPrefabByName(entry.itemTypeName);
            if (prefab == null) { Debug.LogWarning($"[SaveManager] 프리팹 없음: {entry.itemTypeName}"); continue; }
            Vector3 pos = new Vector3(entry.posX, entry.posY, entry.posZ);
            Quaternion rot = new Quaternion(entry.rotX, entry.rotY, entry.rotZ, entry.rotW);
            Vector3 scale = new Vector3(entry.scaleX, entry.scaleY, entry.scaleZ);
            GameObject spawned = Instantiate(prefab, pos, rot);
            spawned.transform.localScale = scale;
            spawned.tag = furnitureTag;
            var fi = spawned.GetComponent<FurnitureItem>() ?? spawned.AddComponent<FurnitureItem>();
            fi.itemType = itemType; fi.sourcePrefab = prefab; fi.sourceScale = scale;
            Debug.Log($"[SaveManager] 가구 복원: {entry.itemTypeName} @ {pos}");
        }
    }

    private GameObject LoadPrefabByName(string typeName)
    {
        GameObject prefab = Resources.Load<GameObject>($"Furniture/{typeName}");
#if UNITY_EDITOR
        if (prefab == null)
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:Prefab {typeName}", new[] { "Assets/Furniture Mega Pack/Prefabs" });
            foreach (string guid in guids)
            {
                string p = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                string n = System.IO.Path.GetFileNameWithoutExtension(p);
                if (n.Equals(typeName, StringComparison.OrdinalIgnoreCase) || n.StartsWith(typeName, StringComparison.OrdinalIgnoreCase))
                { prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(p); if (prefab != null) break; }
            }
        }
#endif
        return prefab;
    }
}
