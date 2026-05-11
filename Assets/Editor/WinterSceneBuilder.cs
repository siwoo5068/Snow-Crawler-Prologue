using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Low Poly Woods 에셋에서 겨울 테마 프리팹만 골라 씬에 배치하고
/// SnowMaterial을 적용하는 에디터 도구.
/// Halloween 관련 항목은 모두 제외.
/// </summary>
public class WinterSceneBuilder : EditorWindow
{
    [MenuItem("Tools/Snow Crawler/Build Winter Environment")]
    static void ShowWindow()
    {
        GetWindow<WinterSceneBuilder>("Winter Scene Builder");
    }

    private bool placeTrees = true;
    private bool placeRocks = true;
    private bool placeTerrain = true;
    private bool placeBushes = true;
    private bool placeSnowVfx = true;
    private bool placeFog = true;

    private int treeCount = 30;
    private int rockCount = 15;
    private int bushCount = 10;
    private float spawnRadius = 80f;
    private float minDistFromCenter = 15f; // SafeZone 근처에 안 놓기

    void OnGUI()
    {
        GUILayout.Label("❄ 겨울 환경 빌더", EditorStyles.boldLabel);
        GUILayout.Space(5);
        GUILayout.Label("Low Poly Woods 에셋에서 겨울 테마만 골라 배치합니다.");
        GUILayout.Label("(Halloween, Autumn 항목은 자동 제외)");
        GUILayout.Space(10);

        GUILayout.Label("배치할 항목", EditorStyles.boldLabel);
        placeTrees = EditorGUILayout.Toggle("🌲 나무 (Fir, Pine, Leafless)", placeTrees);
        placeRocks = EditorGUILayout.Toggle("🪨 바위", placeRocks);
        placeTerrain = EditorGUILayout.Toggle("⛰ 지형 (Hill, Mountain)", placeTerrain);
        placeBushes = EditorGUILayout.Toggle("🌿 덤불 (겨울 색상)", placeBushes);
        placeSnowVfx = EditorGUILayout.Toggle("🌨 눈 내리는 효과", placeSnowVfx);
        placeFog = EditorGUILayout.Toggle("🌫 안개 효과", placeFog);

        GUILayout.Space(10);
        GUILayout.Label("배치 설정", EditorStyles.boldLabel);
        treeCount = EditorGUILayout.IntSlider("나무 개수", treeCount, 5, 80);
        rockCount = EditorGUILayout.IntSlider("바위 개수", rockCount, 5, 40);
        bushCount = EditorGUILayout.IntSlider("덤불 개수", bushCount, 3, 30);
        spawnRadius = EditorGUILayout.Slider("배치 반경", spawnRadius, 30f, 150f);
        minDistFromCenter = EditorGUILayout.Slider("중앙 보호 반경 (SafeZone)", minDistFromCenter, 5f, 30f);

        GUILayout.Space(15);

        // 기존 환경 오브젝트 삭제 버튼
        if (GUILayout.Button("🗑 기존 겨울 환경 오브젝트 삭제"))
        {
            if (EditorUtility.DisplayDialog("확인", "WinterEnvironment 오브젝트를 삭제하시겠습니까?", "삭제", "취소"))
            {
                var existing = GameObject.Find("WinterEnvironment");
                if (existing != null) DestroyImmediate(existing);
                Debug.Log("[WinterSceneBuilder] 기존 환경 삭제 완료");
            }
        }

        GUILayout.Space(5);

        GUI.backgroundColor = new Color(0.3f, 0.8f, 1f);
        if (GUILayout.Button("❄ 겨울 환경 생성!", GUILayout.Height(40)))
        {
            BuildWinterScene();
        }
        GUI.backgroundColor = Color.white;
    }

    void BuildWinterScene()
    {
        // 기존 환경 삭제
        var existing = GameObject.Find("WinterEnvironment");
        if (existing != null) DestroyImmediate(existing);

        // 루트 오브젝트 생성
        var root = new GameObject("WinterEnvironment");
        Undo.RegisterCreatedObjectUndo(root, "Build Winter Environment");

        // SnowMaterial 로드
        Material snowMat = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Low Poly Woods/Materials/SnowMaterial.mat");

        // === 나무 배치 ===
        if (placeTrees)
        {
            var treeParent = new GameObject("Trees");
            treeParent.transform.SetParent(root.transform);

            // 겨울에 어울리는 나무: 침엽수(Fir, Pine) + 앙상한 나무(Leafless) + 통나무(Log) + 나무 그루터기(Stump) + 가지(Branch)
            string[] winterTreeNames = {
                "Fir", "Fir 2",
                "Pine 1", "Pine 2",
                "Leafless 1", "Leafless 2", "Leafless 3", "Leafless 4", "Leafless 5",
                "Log 1", "Log 2", "Logs 1",
                "Stump 1", "Stump 2",
                "Branch 1", "Branch 2", "Branch 3"
            };

            var treePrefabs = LoadPrefabs("Assets/Low Poly Woods/Prefabs/Trees", winterTreeNames);
            PlaceObjects(treePrefabs, treeParent.transform, treeCount, snowMat);
            Debug.Log($"[WinterSceneBuilder] 나무 {treeCount}개 배치 완료");
        }

        // === 바위 배치 ===
        if (placeRocks)
        {
            var rockParent = new GameObject("Rocks");
            rockParent.transform.SetParent(root.transform);

            // 모든 바위 사용 (이끼 바위 포함 — 눈으로 덮일 예정)
            string[] rockNames = {
                "Rock 1", "Rock 2", "Rock 3", "Rock 4", "Rock 5",
                "Rock 6", "Rock 7", "Rock 8", "Rock 9", "Rock 10",
                "Rock moss 1", "Rock moss 2", "Rock moss 3"
            };

            var rockPrefabs = LoadPrefabs("Assets/Low Poly Woods/Prefabs/Rocks", rockNames);
            PlaceObjects(rockPrefabs, rockParent.transform, rockCount, snowMat);
            Debug.Log($"[WinterSceneBuilder] 바위 {rockCount}개 배치 완료");
        }

        // === 지형 배치 ===
        if (placeTerrain)
        {
            var terrainParent = new GameObject("Terrain");
            terrainParent.transform.SetParent(root.transform);

            string[] terrainNames = {
                "Hill 1", "Hill 2", "Hill 3", "Hill 4", "Hill 5",
                "Mountain 1", "Mountain 2", "Mountain 3"
            };

            var terrainPrefabs = LoadPrefabs("Assets/Low Poly Woods/Prefabs/Terrain", terrainNames);

            // 지형은 맵 가장자리에 배치 (배경 역할)
            for (int i = 0; i < terrainPrefabs.Count && i < 6; i++)
            {
                float angle = (360f / 6) * i;
                float dist = spawnRadius * 0.8f;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * dist,
                    -1f, // 살짝 아래로
                    Mathf.Sin(angle * Mathf.Deg2Rad) * dist
                );

                var obj = (GameObject)PrefabUtility.InstantiatePrefab(
                    terrainPrefabs[i % terrainPrefabs.Count]);
                obj.transform.SetParent(terrainParent.transform);
                obj.transform.position = pos;
                obj.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                obj.transform.localScale = Vector3.one * Random.Range(1.5f, 3f);

                if (snowMat != null) ApplyMaterial(obj, snowMat);
            }
            Debug.Log("[WinterSceneBuilder] 지형 배치 완료");
        }

        // === 덤불 (겨울 느낌) ===
        if (placeBushes)
        {
            var bushParent = new GameObject("Bushes");
            bushParent.transform.SetParent(root.transform);

            // 겨울에도 있을 수 있는 덤불만 (꽃, 버섯 제외)
            string[] bushNames = { "Bush 1", "Bush 2", "Bush 3", "Bush 4" };

            var bushPrefabs = LoadPrefabs("Assets/Low Poly Woods/Prefabs/Small plants", bushNames);
            PlaceObjects(bushPrefabs, bushParent.transform, bushCount, snowMat);
            Debug.Log($"[WinterSceneBuilder] 덤불 {bushCount}개 배치 완료");
        }

        // === 눈 내리는 VFX ===
        if (placeSnowVfx)
        {
            var snowVfx = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Low Poly Woods/Vfx/FallingSnowflakes.prefab");
            if (snowVfx != null)
            {
                var vfxObj = (GameObject)PrefabUtility.InstantiatePrefab(snowVfx);
                vfxObj.transform.SetParent(root.transform);
                vfxObj.transform.position = new Vector3(0, 15f, 0);
                vfxObj.name = "SnowflakesVFX";
                Debug.Log("[WinterSceneBuilder] 눈 VFX 배치 완료");
            }
        }

        // === 안개 VFX ===
        if (placeFog)
        {
            var fogVfx = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Low Poly Woods/Vfx/Fog.prefab");
            if (fogVfx != null)
            {
                var fogObj = (GameObject)PrefabUtility.InstantiatePrefab(fogVfx);
                fogObj.transform.SetParent(root.transform);
                fogObj.transform.position = new Vector3(0, 1f, 0);
                fogObj.name = "FogVFX";
                Debug.Log("[WinterSceneBuilder] 안개 VFX 배치 완료");
            }
        }

        // 씬 더티 표시
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("완료!",
            $"❄ 겨울 환경 생성 완료!\n\n" +
            $"나무: {(placeTrees ? treeCount + "개" : "미배치")}\n" +
            $"바위: {(placeRocks ? rockCount + "개" : "미배치")}\n" +
            $"지형: {(placeTerrain ? "배치됨" : "미배치")}\n" +
            $"덤불: {(placeBushes ? bushCount + "개" : "미배치")}\n" +
            $"눈 VFX: {(placeSnowVfx ? "활성" : "미배치")}\n" +
            $"안개 VFX: {(placeFog ? "활성" : "미배치")}\n\n" +
            "모든 오브젝트에 SnowMaterial이 적용되었습니다.",
            "확인");

        Debug.Log("[WinterSceneBuilder] ===== 겨울 환경 생성 완료 =====");
    }

    List<GameObject> LoadPrefabs(string folderPath, string[] names)
    {
        var prefabs = new List<GameObject>();
        foreach (var name in names)
        {
            string path = $"{folderPath}/{name}.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
                prefabs.Add(prefab);
            else
                Debug.LogWarning($"[WinterSceneBuilder] 프리팹을 찾을 수 없음: {path}");
        }
        return prefabs;
    }

    void PlaceObjects(List<GameObject> prefabs, Transform parent, int count, Material mat)
    {
        if (prefabs.Count == 0) return;

        for (int i = 0; i < count; i++)
        {
            var prefab = prefabs[Random.Range(0, prefabs.Count)];
            Vector3 pos = GetRandomPosition();

            var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            obj.transform.SetParent(parent);
            obj.transform.position = pos;
            obj.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            // 나무/바위 크기 랜덤 변화
            float scale = Random.Range(0.7f, 1.5f);
            obj.transform.localScale = Vector3.one * scale;

            // SnowMaterial 적용
            if (mat != null) ApplyMaterial(obj, mat);

            // Static 플래그 (라이트맵/배칭 최적화)
            obj.isStatic = true;
        }
    }

    Vector3 GetRandomPosition()
    {
        Vector3 pos;
        int attempts = 0;
        do
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(minDistFromCenter, spawnRadius);
            pos = new Vector3(Mathf.Cos(angle) * dist, 0f, Mathf.Sin(angle) * dist);
            attempts++;
        } while (pos.magnitude < minDistFromCenter && attempts < 50);

        return pos;
    }

    void ApplyMaterial(GameObject obj, Material mat)
    {
        var renderers = obj.GetComponentsInChildren<MeshRenderer>();
        foreach (var r in renderers)
        {
            var mats = new Material[r.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
                mats[i] = mat;
            r.sharedMaterials = mats;
        }
    }
}
