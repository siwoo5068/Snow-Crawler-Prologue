using System.Collections.Generic;
using UnityEngine;

// =============================================================================
//  ItemSpawner — 완전 랜덤 스폰 + 맵 크기 자동 분석
//
//  ★ 동작 방식 ★
//   - 게임 시작 시 Ground 오브젝트의 Collider/MeshRenderer 바운드를 자동 읽음
//   - SafeZone(별장)으로부터 minSpawnDist ~ maxSpawnDist 범위 안에 랜덤 스폰
//   - 맵 Ground 크기를 바꿔도 자동 적용 (별도 설정 불필요)
//   - RespawnAll() 호출 시 위치가 새로 랜덤화 → 매 출격마다 새로운 탐험
//
//  ★ 크기 조정 ★
//   Inspector > ③ Furniture Scale Rules 에서 수정하세요.
//   Name Prefix = 프리팹 이름 앞부분 (Bed, Chair, Sofa …)
//   새 가구 추가 시 [+] 버튼으로 항목만 추가하면 됩니다.
// =============================================================================

[System.Serializable]
public class FurnitureScaleRule
{
    [Tooltip("프리팹 이름 앞부분 (대소문자 무관). 예: Bed, Chair, Sofa, Table, Cushion, Lamp")]
    public string namePrefix = "";

    [Tooltip("이 카테고리에 적용할 localScale")]
    public Vector3 scale = Vector3.one;
}

public class ItemSpawner : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════════
    //  [1] MAP AUTO-DETECT — 맵 자동 분석
    // ══════════════════════════════════════════════════════════════════════
    [Header("① Map Auto-Detect")]
    [Tooltip("바닥 Ground 오브젝트. 비워두면 'Ground' 이름으로 자동 탐색합니다.")]
    public GameObject groundObject;

    [Tooltip("별장(안전지대) 오브젝트. 비워두면 'SafeZone' 태그로 자동 탐색합니다.")]
    public GameObject safeZoneObject;

    [Tooltip("맵 경계 안쪽 여백 (미터). 맵 끝에 너무 붙어 스폰되는 것을 방지합니다.")]
    public float mapEdgeMargin = 2f;

    // ══════════════════════════════════════════════════════════════════════
    //  [2] SPAWN RANGE — 스폰 거리 범위
    // ══════════════════════════════════════════════════════════════════════
    [Header("② Spawn Range (SafeZone 기준 거리)")]
    [Tooltip("별장에서 최소 이 거리 이상 떨어진 곳에 스폰 (너무 가까이 나오면 의미가 없음)")]
    public float minSpawnDist = 5f;

    [Tooltip("별장에서 최대 이 거리 이내에 스폰 (0이면 맵 전체 사용)")]
    public float maxSpawnDist = 0f;

    // ══════════════════════════════════════════════════════════════════════
    //  [3] FURNITURE SPAWN — 가구 스폰
    // ══════════════════════════════════════════════════════════════════════
    [Header("③ Furniture Spawn")]
    [Tooltip("스폰할 가구 프리팹 목록. 새 가구 추가 시 여기에 드래그하세요.")]
    public GameObject[] furniturePrefabs;

    [Tooltip("한 번에 스폰할 가구 수")]
    public int furnitureSpawnCount = 8;

    [Tooltip("위치 결정 최대 시도 횟수 (거리 조건 충족 안 되면 재시도)")]
    public int maxPlacementTries = 30;

    // ══════════════════════════════════════════════════════════════════════
    //  [4] SUPPLY BOX SPAWN — 재료 상자 스폰
    // ══════════════════════════════════════════════════════════════════════
    [Header("④ Supply Box Spawn")]
    [Tooltip("재료 상자 프리팹")]
    public GameObject supplyBoxPrefab;

    [Tooltip("한 번에 스폰할 재료 상자 수")]
    public int supplySpawnCount = 5;

    // ══════════════════════════════════════════════════════════════════════
    //  [5] FURNITURE SCALE RULES — ★ 크기 조정 목록 ★
    // ══════════════════════════════════════════════════════════════════════
    [Header("⑤ Furniture Scale Rules  ★ 크기 조정은 여기서 ★")]
    [Tooltip("Name Prefix가 프리팹 이름 앞부분과 일치하면 해당 Scale 적용.\n" +
             "위에서부터 순서대로 매칭, 처음 일치하는 규칙 사용.\n" +
             "새 카테고리 추가 시 [+] 버튼으로 항목 추가.")]
    public List<FurnitureScaleRule> scaleRules = new List<FurnitureScaleRule>
    {
        new FurnitureScaleRule { namePrefix = "Bed",     scale = new Vector3(1.2f, 0.6f, 2.0f) },
        new FurnitureScaleRule { namePrefix = "Sofa",    scale = Vector3.one },
        new FurnitureScaleRule { namePrefix = "Table",   scale = Vector3.one },
        new FurnitureScaleRule { namePrefix = "Chair",   scale = Vector3.one },
        new FurnitureScaleRule { namePrefix = "Cushion", scale = Vector3.one },
    };

    [Tooltip("어떤 규칙에도 해당하지 않는 가구의 기본 크기")]
    public Vector3 defaultScale = Vector3.one;

    // ══════════════════════════════════════════════════════════════════════
    //  [6] GROUND SNAP — Y축 바닥 보정
    // ══════════════════════════════════════════════════════════════════════
    [Header("⑥ Ground Snap")]
    [Tooltip("바닥 Raycast로 Y 높이 자동 보정")]
    public bool groundSnap = true;

    [Tooltip("Raycast 시작 높이 (위에서 아래로 쏨)")]
    public float groundSnapHeight = 10f;

    // ── 런타임 캐시 ───────────────────────────────────────────────────────
    private readonly List<GameObject> _spawnedObjects = new List<GameObject>();
    private int     _groundMask;
    private Bounds  _mapBounds;        // 맵 XZ 범위 (Y는 무시)
    private Vector3 _safeZonePos;      // 별장 위치
    private bool    _mapReady = false;

    // ─────────────────────────────────────────────────────────────────────
    void Awake()
    {
        _groundMask = ~(LayerMask.GetMask("Player", "UI", "Ignore Raycast"));
        _mapReady   = AnalyseMap();
    }

    void Start()
    {
        if (_mapReady) SpawnAll();
        else Debug.LogError("[ItemSpawner] 맵 분석 실패 — Ground 오브젝트를 확인하세요.");
    }

    // ── 공개 API ──────────────────────────────────────────────────────────
    /// <summary>기존 스폰 오브젝트 전체 제거 후 새로 스폰. SortieManager에서 출격 시 호출.</summary>
    public void RespawnAll()
    {
        ClearAll();
        if (_mapReady) SpawnAll();
    }

    // ── 맵 자동 분석 ──────────────────────────────────────────────────────
    bool AnalyseMap()
    {
        // Ground 오브젝트 탐색
        if (groundObject == null)
            groundObject = GameObject.Find("Ground");

        if (groundObject == null)
        {
            Debug.LogError("[ItemSpawner] Ground 오브젝트를 찾을 수 없습니다. Inspector에서 직접 할당하세요.");
            return false;
        }

        // Ground Bounds 계산 (Collider 우선, 없으면 MeshRenderer)
        Bounds raw;
        var col = groundObject.GetComponent<Collider>();
        var mr  = groundObject.GetComponent<MeshRenderer>();
        if      (col != null) raw = col.bounds;
        else if (mr  != null) raw = mr.bounds;
        else
        {
            // Collider/MeshRenderer 모두 없으면 스케일로 추정
            Vector3 s = groundObject.transform.lossyScale;
            raw = new Bounds(groundObject.transform.position,
                             new Vector3(s.x * 10f, 1f, s.z * 10f));
            Debug.LogWarning("[ItemSpawner] Ground에 Collider/MeshRenderer가 없어 스케일로 맵 범위를 추정합니다.");
        }

        // Y는 관리하지 않고 XZ만 사용 (마진 적용)
        _mapBounds = new Bounds(
            new Vector3(raw.center.x, 0f, raw.center.z),
            new Vector3(raw.size.x - mapEdgeMargin * 2f, 100f, raw.size.z - mapEdgeMargin * 2f)
        );

        // SafeZone 탐색
        if (safeZoneObject == null)
            safeZoneObject = GameObject.FindGameObjectWithTag("SafeZone");

        _safeZonePos = safeZoneObject != null
            ? safeZoneObject.transform.position
            : groundObject.transform.position; // fallback: 맵 중심

        // maxSpawnDist 미설정(0)이면 맵 대각선 절반으로 자동 계산
        if (maxSpawnDist <= 0f)
            maxSpawnDist = Mathf.Sqrt(_mapBounds.size.x * _mapBounds.size.x +
                                      _mapBounds.size.z * _mapBounds.size.z) * 0.5f;

        Debug.Log($"[ItemSpawner] 맵 분석 완료 — " +
                  $"XZ범위 {_mapBounds.size.x:F1}×{_mapBounds.size.z:F1}m | " +
                  $"SafeZone {_safeZonePos} | " +
                  $"스폰거리 {minSpawnDist:F1}~{maxSpawnDist:F1}m");
        return true;
    }

    // ── 전체 스폰 ─────────────────────────────────────────────────────────
    void SpawnAll()
    {
        SpawnFurnitures();
        SpawnSupplyBoxes();

        int f = 0, s = 0;
        foreach (var go in _spawnedObjects)
        {
            if (go == null) continue;
            if (go.CompareTag("FurnitureItem")) f++;
            else if (go.CompareTag("MaterialItem")) s++;
        }
        Debug.Log($"[ItemSpawner] 스폰 완료 — 가구:{f}개, 재료:{s}개");
    }

    // ── 가구 스폰 ─────────────────────────────────────────────────────────
    void SpawnFurnitures()
    {
        if (furniturePrefabs == null || furniturePrefabs.Length == 0)
        {
            Debug.LogWarning("[ItemSpawner] furniturePrefabs 비어 있음.");
            return;
        }

        // 프리팹 풀 셔플
        var pool = new List<GameObject>(furniturePrefabs);
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = pool[i]; pool[i] = pool[j]; pool[j] = tmp;
        }

        int spawned = 0;
        for (int i = 0; i < furnitureSpawnCount; i++)
        {
            Vector3 pos;
            if (!TryGetRandomPos(out pos)) continue;

            GameObject prefab = pool[i % pool.Count];
            float      yaw    = Random.Range(0f, 360f);
            GameObject go     = Instantiate(prefab, pos, Quaternion.Euler(0f, yaw, 0f));
            go.name           = "Spawned_" + prefab.name;

            Vector3 scale = ResolveScale(prefab.name);
            go.transform.localScale = scale;

            FurnitureItem fi = go.GetComponent<FurnitureItem>();
            if (fi == null) fi = go.AddComponent<FurnitureItem>();
            fi.sourcePrefab = prefab;
            fi.sourceScale  = scale;

            go.tag = "FurnitureItem";
            EnsureCollider(go);
            _spawnedObjects.Add(go);
            spawned++;
        }

        if (spawned < furnitureSpawnCount)
            Debug.LogWarning($"[ItemSpawner] 요청 {furnitureSpawnCount}개 중 {spawned}개만 스폰됨 — minSpawnDist를 줄이거나 맵을 넓히세요.");
    }

    // ── 재료 상자 스폰 ───────────────────────────────────────────────────
    void SpawnSupplyBoxes()
    {
        if (supplyBoxPrefab == null) return;

        for (int i = 0; i < supplySpawnCount; i++)
        {
            Vector3 pos;
            if (!TryGetRandomPos(out pos)) continue;

            GameObject go = Instantiate(supplyBoxPrefab, pos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            go.name = "Spawned_SupplyBox";
            go.tag  = "MaterialItem";
            EnsureCollider(go);
            _spawnedObjects.Add(go);
        }
    }

    // ── 랜덤 위치 결정 (거리 조건 + 맵 범위 체크) ──────────────────────
    bool TryGetRandomPos(out Vector3 result)
    {
        for (int attempt = 0; attempt < maxPlacementTries; attempt++)
        {
            // 맵 XZ 범위 내 랜덤 위치
            float x = Random.Range(_mapBounds.min.x, _mapBounds.max.x);
            float z = Random.Range(_mapBounds.min.z, _mapBounds.max.z);
            Vector3 candidate = new Vector3(x, groundSnapHeight, z);

            // SafeZone 거리 조건 확인 (XZ 평면 거리)
            float dist = Vector2.Distance(
                new Vector2(candidate.x, candidate.z),
                new Vector2(_safeZonePos.x, _safeZonePos.z));

            if (dist < minSpawnDist || dist > maxSpawnDist) continue;

            // Ground Snap: 위에서 Raycast로 Y 보정
            if (groundSnap)
            {
                Ray ray = new Ray(candidate, Vector3.down);
                if (Physics.Raycast(ray, out RaycastHit hit, groundSnapHeight * 2f, _groundMask))
                    candidate.y = hit.point.y;
                else
                    candidate.y = 0f;
            }

            result = candidate;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    // ── Scale Rule 해석 ───────────────────────────────────────────────────
    Vector3 ResolveScale(string prefabName)
    {
        if (scaleRules != null)
            foreach (var rule in scaleRules)
                if (!string.IsNullOrEmpty(rule.namePrefix) &&
                    prefabName.StartsWith(rule.namePrefix, System.StringComparison.OrdinalIgnoreCase))
                    return rule.scale;
        return defaultScale;
    }

    // ── 제거 ─────────────────────────────────────────────────────────────
    void ClearAll()
    {
        foreach (var go in _spawnedObjects)
            if (go != null) Destroy(go);
        _spawnedObjects.Clear();
    }

    // ── Collider 보장 ────────────────────────────────────────────────────
    static void EnsureCollider(GameObject go)
    {
        Collider c = go.GetComponent<Collider>();
        if (c == null) c = go.AddComponent<BoxCollider>();
        c.enabled = true;
    }

    // ── 에디터 Gizmo — 스폰 가능 구역 시각화 ────────────────────────────
    void OnDrawGizmosSelected()
    {
        // 맵 범위 (파란 박스)
        if (groundObject != null)
        {
            Gizmos.color = new Color(0.2f, 0.5f, 1f, 0.2f);
            Gizmos.DrawCube(
                new Vector3(_mapBounds.center.x, 0.1f, _mapBounds.center.z),
                new Vector3(_mapBounds.size.x, 0.1f, _mapBounds.size.z));
        }

        // SafeZone 최소/최대 거리 원 (노란/초록)
        Vector3 origin = safeZoneObject != null
            ? safeZoneObject.transform.position
            : (groundObject != null ? groundObject.transform.position : transform.position);

        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.6f);
        DrawCircleGizmo(origin, minSpawnDist);

        Gizmos.color = new Color(0.3f, 1f, 0.3f, 0.4f);
        float maxR = maxSpawnDist > 0f ? maxSpawnDist : 30f;
        DrawCircleGizmo(origin, maxR);
    }

    static void DrawCircleGizmo(Vector3 center, float radius, int segments = 40)
    {
        float step = 360f / segments;
        for (int i = 0; i < segments; i++)
        {
            float a0 = Mathf.Deg2Rad * (i * step);
            float a1 = Mathf.Deg2Rad * ((i + 1) * step);
            Vector3 p0 = center + new Vector3(Mathf.Cos(a0), 0f, Mathf.Sin(a0)) * radius;
            Vector3 p1 = center + new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1)) * radius;
            Gizmos.DrawLine(p0, p1);
        }
    }
}
