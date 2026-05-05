using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Furniture Mega Pack 자동 수정 도구
/// 1. FBX Scale Factor 0.01 적용
/// 2. URP 머티리얼 자동 생성/연결
/// 3. 프리팹 MeshFilter/MeshRenderer FBX 재연결
/// Menu: Tools > Furniture Pack Fixer
/// </summary>
public class FurniturePackFixer : EditorWindow
{
    private const string FURNITURE_ROOT = "Assets/Furniture Mega Pack";
    private const string FBX_ROOT       = "Assets/Furniture Mega Pack/FBX";
    private const string PREFAB_ROOT    = "Assets/Furniture Mega Pack/Prefabs";
    private const string MAT_OUTPUT     = "Assets/Furniture Mega Pack/Materials";
    private const string URP_SHADER     = "Universal Render Pipeline/Lit";

    // ═══════════════════════════════════════════════════
    //  메인 진입점 – 모든 수정 순차 실행
    // ═══════════════════════════════════════════════════
    [MenuItem("Tools/Furniture Pack Fixer/▶ Run All Fixes")]
    public static void RunAllFixes()
    {
        Debug.Log("=== [FurniturePackFixer] 전체 수정 시작 ===");

        int r1 = DoFixFBXScale();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        int r2 = DoFixMaterials();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        int r3 = DoRewirePrefabs();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string msg = $"모든 수정이 완료되었습니다!\n\n" +
                     $"• FBX Scale Factor 수정: {r1}개\n" +
                     $"• 머티리얼 생성/연결: {r2}개\n" +
                     $"• 프리팹 메쉬 재연결: {r3}개";

        Debug.Log($"=== [FurniturePackFixer] 완료 ===\n{msg}");
        EditorUtility.DisplayDialog("Furniture Pack Fixer 완료", msg, "확인");
    }

    // ═══════════════════════════════════════════════════
    //  STEP 1 – FBX Scale Factor → 0.01
    // ═══════════════════════════════════════════════════
    [MenuItem("Tools/Furniture Pack Fixer/Step 1 – Fix FBX Scale (0.01)")]
    public static void Step1() { DoFixFBXScale(); AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); }

    private static int DoFixFBXScale()
    {
        Debug.Log("[Step 1] FBX Scale Factor 수정...");
        int count = 0;

        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { FBX_ROOT });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)) continue;

            ModelImporter mi = AssetImporter.GetAtPath(path) as ModelImporter;
            if (mi == null) continue;

            bool dirty = false;
            if (!Mathf.Approximately(mi.globalScale, 0.01f)) { mi.globalScale = 0.01f; dirty = true; }
            if (mi.useFileScale)                              { mi.useFileScale = false;  dirty = true; }

            if (dirty)
            {
                mi.SaveAndReimport();
                Debug.Log($"  [Scale] {Path.GetFileName(path)} → 0.01 적용");
                count++;
            }
        }

        Debug.Log($"[Step 1] 완료: {count}개");
        return count;
    }

    // ═══════════════════════════════════════════════════
    //  STEP 2 – URP 머티리얼 생성 / 핑크 셰이더 교체
    // ═══════════════════════════════════════════════════
    [MenuItem("Tools/Furniture Pack Fixer/Step 2 – Fix Materials (URP)")]
    public static void Step2() { DoFixMaterials(); AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); }

    private static int DoFixMaterials()
    {
        Debug.Log("[Step 2] URP 머티리얼 수정...");
        int count = 0;

        // Materials 폴더 생성
        if (!AssetDatabase.IsValidFolder(MAT_OUTPUT))
            AssetDatabase.CreateFolder(FURNITURE_ROOT, "Materials");

        Shader urpLit = Shader.Find(URP_SHADER) ?? Shader.Find("Standard");
        if (urpLit == null) { Debug.LogError("[Step 2] 셰이더를 찾을 수 없습니다."); return 0; }
        Debug.Log($"  사용 셰이더: {urpLit.name}");

        // 프리팹마다 머티리얼 생성
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { PREFAB_ROOT }))
        {
            string pp   = AssetDatabase.GUIDToAssetPath(guid);
            string name = Path.GetFileNameWithoutExtension(pp);
            string mp   = $"{MAT_OUTPUT}/{name}_Mat.mat";

            if (!File.Exists(AssetDatabase.GetAssetPath(AssetDatabase.LoadAssetAtPath<Material>(mp))))
            {
                Material m = new Material(urpLit) { name = $"{name}_Mat" };
                SetBaseColor(m, urpLit, GetCategoryColor(name));
                AssetDatabase.CreateAsset(m, mp);
                Debug.Log($"  [Mat] 생성: {mp}");
                count++;
            }
        }

        // FBX 내장 + Furniture 전체 머티리얼 중 오류 셰이더 교체
        string[] allMatGuids = AssetDatabase.FindAssets("t:Material", new[] { FURNITURE_ROOT });
        foreach (string guid in allMatGuids)
        {
            string mp  = AssetDatabase.GUIDToAssetPath(guid);
            Material m = AssetDatabase.LoadAssetAtPath<Material>(mp);
            if (m == null || m.shader == null) continue;

            string sn = m.shader.name;
            if (sn == "Standard" || sn.StartsWith("Hidden") || sn.Contains("Error"))
            {
                m.shader = urpLit;
                EditorUtility.SetDirty(m);
                Debug.Log($"  [Mat] 셰이더 교체: {Path.GetFileName(mp)}  {sn} → {urpLit.name}");
                count++;
            }
        }

        Debug.Log($"[Step 2] 완료: {count}개");
        return count;
    }

    // ═══════════════════════════════════════════════════
    //  STEP 3 – 프리팹 Mesh 재연결
    // ═══════════════════════════════════════════════════
    [MenuItem("Tools/Furniture Pack Fixer/Step 3 – Rewire Prefab Meshes")]
    public static void Step3() { DoRewirePrefabs(); AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); }

    private static int DoRewirePrefabs()
    {
        Debug.Log("[Step 3] 프리팹 메쉬 재연결...");
        int count = 0;

        // FBX 이름 → 경로 딕셔너리
        var fbxMap = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
        foreach (string g in AssetDatabase.FindAssets("t:Model", new[] { FBX_ROOT }))
        {
            string p = AssetDatabase.GUIDToAssetPath(g);
            if (p.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                fbxMap[Path.GetFileNameWithoutExtension(p)] = p;
        }
        Debug.Log($"  FBX 발견: {string.Join(", ", fbxMap.Keys)}");

        // 머티리얼 이름 → 에셋 딕셔너리
        var matMap = new Dictionary<string, Material>(System.StringComparer.OrdinalIgnoreCase);
        foreach (string g in AssetDatabase.FindAssets("t:Material", new[] { MAT_OUTPUT }))
        {
            string p = AssetDatabase.GUIDToAssetPath(g);
            Material m = AssetDatabase.LoadAssetAtPath<Material>(p);
            if (m != null) matMap[m.name.Replace("_Mat", "")] = m;
        }

        // 프리팹 순회
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { PREFAB_ROOT }))
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            string prefabName = Path.GetFileNameWithoutExtension(prefabPath);

            string fbxPath = BestFbxMatch(prefabName, fbxMap);
            if (fbxPath == null) { Debug.LogWarning($"  [Skip] '{prefabName}' FBX 없음"); continue; }

            Mesh[]     fbxMeshes = LoadAllSubAssets<Mesh>(fbxPath);
            Material[] fbxMats   = LoadAllSubAssets<Material>(fbxPath);

            if (fbxMeshes.Length == 0) { Debug.LogWarning($"  [Skip] '{prefabName}' 메쉬 없음"); continue; }

            Material assignMat = matMap.ContainsKey(prefabName) ? matMap[prefabName]
                               : (fbxMats.Length > 0 ? fbxMats[0] : null);

            using (var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
            {
                GameObject root   = scope.prefabContentsRoot;
                bool       dirty  = false;

                var mfList = root.GetComponentsInChildren<MeshFilter>(true);
                var mrList = root.GetComponentsInChildren<MeshRenderer>(true);

                // MeshFilter – 메쉬 채우기
                for (int i = 0; i < mfList.Length; i++)
                {
                    Mesh target = i < fbxMeshes.Length ? fbxMeshes[i] : fbxMeshes[0];
                    if (mfList[i].sharedMesh == null || IsNullOrMissing(mfList[i].sharedMesh))
                    {
                        mfList[i].sharedMesh = target;
                        EditorUtility.SetDirty(mfList[i]);
                        dirty = true;
                        Debug.Log($"  [Mesh] {prefabName}/{mfList[i].name} → {target.name}");
                    }
                }

                // MeshRenderer – 핑크/null 머티리얼 교체
                foreach (MeshRenderer mr in mrList)
                {
                    if (assignMat == null) continue;
                    Material[] shared = mr.sharedMaterials;
                    bool needFix = shared.Any(m => m == null || IsNullOrMissing(m) ||
                                   (m.shader != null && (m.shader.name.StartsWith("Hidden") || m.shader.name.Contains("Error"))));
                    if (needFix)
                    {
                        mr.sharedMaterials = Enumerable.Repeat(assignMat, shared.Length).ToArray();
                        EditorUtility.SetDirty(mr);
                        dirty = true;
                        Debug.Log($"  [Mat] {prefabName}/{mr.name} → {assignMat.name}");
                    }
                }

                // 컴포넌트 자체가 없는 경우 추가
                if (mfList.Length == 0)
                {
                    MeshFilter   mf = root.AddComponent<MeshFilter>();
                    MeshRenderer mr = root.AddComponent<MeshRenderer>();
                    mf.sharedMesh      = fbxMeshes[0];
                    mr.sharedMaterial  = assignMat ?? CreateFallbackMat(prefabName);
                    EditorUtility.SetDirty(root);
                    dirty = true;
                    Debug.Log($"  [New] {prefabName} → 컴포넌트 신규 추가");
                }

                if (dirty) count++;
            }
        }

        RevertScenePinkMaterials();

        Debug.Log($"[Step 3] 완료: {count}개 프리팹");
        return count;
    }

    // ─────────────────────────────────────────────────────────────────
    //  씬 인스턴스 핑크 머티리얼 되돌리기
    // ─────────────────────────────────────────────────────────────────
    [MenuItem("Tools/Furniture Pack Fixer/Fix Pink Objects In Current Scene")]
    public static void RevertScenePinkMaterials()
    {
        int n = 0;
        foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.hideFlags != HideFlags.None) continue;
            if (!PrefabUtility.IsPartOfPrefabInstance(go)) continue;
            if (!PrefabUtility.IsOutermostPrefabInstanceRoot(go)) continue;

            foreach (MeshRenderer mr in go.GetComponentsInChildren<MeshRenderer>(true))
            {
                bool bad = mr.sharedMaterials.Any(m => m == null ||
                           (m != null && m.shader != null && (m.shader.name.StartsWith("Hidden") || m.shader.name.Contains("Error"))));
                if (bad)
                {
                    PrefabUtility.RevertObjectOverride(mr, InteractionMode.AutomatedAction);
                    n++;
                }
            }
        }
        if (n > 0) Debug.Log($"  [Scene] {n}개 씬 인스턴스 되돌림");
    }

    // ─────────────────────────────────────────────────────────────────
    //  헬퍼
    // ─────────────────────────────────────────────────────────────────
    private static string BestFbxMatch(string prefabName, Dictionary<string, string> map)
    {
        if (map.TryGetValue(prefabName, out string exact)) return exact;
        string best = null;
        foreach (var kv in map)
            if (kv.Key.StartsWith(prefabName, System.StringComparison.OrdinalIgnoreCase))
                if (best == null || kv.Key.Length < best.Length) best = kv.Key;
        if (best != null) { Debug.Log($"  [Match] '{prefabName}' → '{best}'"); return map[best]; }
        return null;
    }

    private static T[] LoadAllSubAssets<T>(string path) where T : Object =>
        AssetDatabase.LoadAllAssetsAtPath(path).OfType<T>().ToArray();

    private static bool IsNullOrMissing(Object obj) =>
        obj == null || string.IsNullOrEmpty(obj.name);

    private static Material CreateFallbackMat(string name)
    {
        Shader s = Shader.Find(URP_SHADER) ?? Shader.Find("Standard");
        return new Material(s) { name = name + "_Auto" };
    }

    private static void SetBaseColor(Material m, Shader s, Color c)
    {
        if (s.name.Contains("Universal") || s.name.Contains("Lit"))
            m.SetColor("_BaseColor", c);
        else
            m.SetColor("_Color", c);
    }

    private static Color GetCategoryColor(string name)
    {
        string l = name.ToLower();
        if (l.StartsWith("bed"))     return new Color(0.72f, 0.60f, 0.50f);
        if (l.StartsWith("chair"))   return new Color(0.55f, 0.45f, 0.38f);
        if (l.StartsWith("sofa"))    return new Color(0.48f, 0.52f, 0.58f);
        if (l.StartsWith("cushion")) return new Color(0.80f, 0.70f, 0.60f);
        if (l.StartsWith("table"))   return new Color(0.60f, 0.45f, 0.30f);
        return new Color(0.7f, 0.7f, 0.7f);
    }
}
