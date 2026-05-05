using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Furniture Mega Pack 자동 에셋 포스트프로세서
/// ──────────────────────────────────────────────────────────────
/// 새 FBX 추가     → Scale Factor 0.01 자동 적용
/// 새 프리팹 추가  → Tag / BoxCollider / FurnitureItem / sourcePrefab 자동 설정
/// ──────────────────────────────────────────────────────────────
/// 이 파일은 Assets/Editor 폴더에 있어야 동작합니다.
/// </summary>
public class FurniturePackPostprocessor : AssetPostprocessor
{
    // ── 감시 경로 ─────────────────────────────────────────────────
    private const string FBX_ROOT    = "Assets/Furniture Mega Pack/FBX";
    private const string PREFAB_ROOT = "Assets/Furniture Mega Pack/Prefabs";

    // 재진입 방지 (EditPrefabContentsScope → SaveAssets → OnPostprocessAllAssets 무한루프 차단)
    private static bool _isProcessing = false;

    // ══════════════════════════════════════════════════════════════
    //  STEP 1 – FBX 임포트 전처리  (Scale Factor → 0.01)
    // ══════════════════════════════════════════════════════════════
    void OnPreprocessModel()
    {
        // 감시 폴더 밖이면 무시
        if (!assetPath.StartsWith(FBX_ROOT, System.StringComparison.OrdinalIgnoreCase)) return;
        if (!assetPath.EndsWith(".fbx",     System.StringComparison.OrdinalIgnoreCase)) return;

        ModelImporter mi = assetImporter as ModelImporter;
        if (mi == null) return;

        bool dirty = false;

        if (!Mathf.Approximately(mi.globalScale, 0.01f))
        {
            mi.globalScale = 0.01f;
            dirty = true;
        }

        if (mi.useFileScale)
        {
            mi.useFileScale = false;
            dirty = true;
        }

        if (dirty)
            Debug.Log($"[FurniturePP] ◈ FBX Scale → 0.01 자동 적용: {Path.GetFileName(assetPath)}");
    }

    // ══════════════════════════════════════════════════════════════
    //  STEP 2 – 프리팹 임포트/이동 후처리  (전체 자동 설정)
    // ══════════════════════════════════════════════════════════════
    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        // 재진입 차단
        if (_isProcessing) return;

        // PREFAB_ROOT 하위 .prefab 파일만 필터링
        string[] targets = importedAssets
            .Concat(movedAssets)
            .Where(p =>
                p.StartsWith(PREFAB_ROOT, System.StringComparison.OrdinalIgnoreCase) &&
                p.EndsWith(".prefab",     System.StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .ToArray();

        if (targets.Length == 0) return;

        _isProcessing = true;
        try
        {
            int changed = 0;
            foreach (string path in targets)
            {
                if (ConfigurePrefab(path))
                    changed++;
            }

            if (changed > 0)
            {
                AssetDatabase.SaveAssets();
                Debug.Log($"[FurniturePP] ✅ {changed}/{targets.Length}개 프리팹 자동 설정 완료");
            }
        }
        finally
        {
            _isProcessing = false;
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  프리팹 단일 자동 설정 로직
    // ══════════════════════════════════════════════════════════════
    static bool ConfigurePrefab(string path)
    {
        // 에셋 참조는 scope 바깥에서 로드 (sourcePrefab 자기 참조에 사용)
        GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefabAsset == null)
        {
            Debug.LogWarning($"[FurniturePP] 프리팹 로드 실패: {path}");
            return false;
        }

        string prefabName = Path.GetFileNameWithoutExtension(path);
        bool   changed    = false;

        // EditPrefabContentsScope → 편집 후 자동 저장 (using 블록 종료 시)
        using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
        {
            GameObject root = scope.prefabContentsRoot;

            // ── 1. Tag ─────────────────────────────────────────────
            if (!root.CompareTag("FurnitureItem"))
            {
                root.tag = "FurnitureItem";
                changed  = true;
                Debug.Log($"[FurniturePP] {prefabName}: Tag → FurnitureItem");
            }

            // ── 2. BoxCollider (메쉬 크기에 맞게 자동 생성) ─────────
            Collider existingCol = root.GetComponentInChildren<Collider>(true);
            if (existingCol == null)
            {
                BoxCollider bc = root.AddComponent<BoxCollider>();

                // 루트 또는 자식 메쉬에서 bounds 추출해 Collider 크기 설정
                MeshFilter mf = root.GetComponentInChildren<MeshFilter>(true);
                if (mf != null && mf.sharedMesh != null)
                {
                    bc.center = mf.sharedMesh.bounds.center;
                    bc.size   = mf.sharedMesh.bounds.size;
                }

                existingCol = bc;
                changed     = true;
                Debug.Log($"[FurniturePP] {prefabName}: BoxCollider 자동 추가 (메쉬 크기에 맞춤)");
            }

            // Collider 비활성화 방지
            if (!existingCol.enabled)
            {
                existingCol.enabled = true;
                changed = true;
            }

            // ── 3. FurnitureItem 컴포넌트 ─────────────────────────
            FurnitureItem fi = root.GetComponent<FurnitureItem>();
            if (fi == null)
            {
                fi      = root.AddComponent<FurnitureItem>();
                changed = true;
                Debug.Log($"[FurniturePP] {prefabName}: FurnitureItem 컴포넌트 추가");
            }

            // ── 4. sourcePrefab 자기 참조 ─────────────────────────
            //    prefabAsset은 scope 바깥에서 로드된 에셋 레퍼런스
            //    → fi.sourcePrefab에 할당하면 프리팹에 자기 참조로 저장됨
            if (fi.sourcePrefab != prefabAsset)
            {
                fi.sourcePrefab = prefabAsset;
                changed         = true;
                Debug.Log($"[FurniturePP] {prefabName}: sourcePrefab 자기 참조 설정");
            }
        }
        // using 블록 종료 → EditPrefabContentsScope 가 자동으로 SavePrefab 수행

        return changed;
    }

    // ══════════════════════════════════════════════════════════════
    //  수동 실행 메뉴 (이미 있는 프리팹 일괄 재처리)
    // ══════════════════════════════════════════════════════════════
    [MenuItem("Tools/Furniture Pack Fixer/Postprocessor – Re-apply All Prefabs")]
    public static void ReapplyAllPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { PREFAB_ROOT });
        int changed = 0;

        _isProcessing = true;
        try
        {
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (ConfigurePrefab(path)) changed++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string msg = $"전체 재처리 완료!\n{changed}/{guids.Length}개 프리팹 업데이트됨";
            Debug.Log($"[FurniturePP] {msg}");
            EditorUtility.DisplayDialog("Furniture Postprocessor", msg, "확인");
        }
        finally
        {
            _isProcessing = false;
        }
    }
}
