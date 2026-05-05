using System.Collections.Generic;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════
//  FurniturePlacement
//  ─ 기존 버그 수정 사항 완전 보존:
//      • Unity fake-null 이중 검사 (DropLastSlot 포함)
//      • MeshFilter.sharedMesh.bounds 기반 Y축 피벗 오프셋 자동 보정
//      • sourceScale 저장 / 복원
//      • BoxCollider 활성화 보장
//  ─ 신규: Rust 스타일 Ghost(반투명) 배치 프리뷰 시스템
//      • Q키 → Ghost 모드 진입
//      • Ghost는 단 1회 생성, Update에서 Transform만 갱신 (재생성 없음)
//      • 좌클릭 → 실제 배치 (기존 안전 스폰 로직 그대로 사용)
//      • Q키 재입력 / 우클릭 → Ghost 취소 (인벤토리에 아이템 반환)
// ═══════════════════════════════════════════════════════════════════════════
public class FurniturePlacement : MonoBehaviour
{
    // ── References ────────────────────────────────────────────────────────
    [Header("References")]
    public PlayerInventory inventory;
    public SurvivalTimer   survivalTimer;
    public CabinComfort    cabinComfort;

    // ── Placement Settings ────────────────────────────────────────────────
    [Header("Placement Settings")]
    public float   placeDistance = 2f;
    public KeyCode placeKey      = KeyCode.Q;

    // ── Ghost Settings ────────────────────────────────────────────────────
    [Header("Ghost Preview")]
    [Tooltip("반투명 고스트 머티리얼. 미할당 시 코드에서 자동 생성.")]
    public Material ghostMaterial;

    // ── Audio ─────────────────────────────────────────────────────────────
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip   placeSound;

    // ── 내부 상태 ──────────────────────────────────────────────────────────
    private static readonly Dictionary<ItemType, Material> _cachedMaterials
        = new Dictionary<ItemType, Material>();

    // Ghost 모드 상태
    private bool          _ghostMode;         // Ghost 모드 활성 여부
    private GameObject    _ghostObj;          // 현재 Ghost 인스턴스
    private InventorySlot _pendingSlot;       // Ghost 확정 대기 중인 슬롯
    private float         _ghostPivotOffset;  // Y축 피벗 오프셋 (캐시)
    private Material      _ghostMat;          // 런타임 생성 고스트 머티리얼 인스턴스
    private float         _rotationOffset;    // R키 90도 회전 오프셋 (0/90/180/270)

    // 레이어 마스크는 Start에서 1회 계산
    private int _groundMask;

    // ── 라이프사이클 ───────────────────────────────────────────────────────
    void Start()
    {
        if (inventory    == null) inventory    = GetComponent<PlayerInventory>();
        if (survivalTimer== null) survivalTimer = GetComponent<SurvivalTimer>();
        if (audioSource  == null) audioSource  = GetComponent<AudioSource>();

        // 지형·바닥만 감지, 플레이어·가구·Ghost는 제외
        _groundMask = ~(LayerMask.GetMask("Player", "FurnitureItem", "Ignore Raycast"));
    }

    void OnDestroy()
    {
        // Ghost 잔여물 정리
        DestroyGhost();

        // 캐시 머티리얼 해제
        foreach (var mat in _cachedMaterials.Values)
            if (mat != null) Object.Destroy(mat);
        _cachedMaterials.Clear();

        // 런타임 Ghost 머티리얼 해제
        if (_ghostMat != null) Object.Destroy(_ghostMat);
    }

    // ── Update ─────────────────────────────────────────────────────────────
    void Update()
    {
        if (survivalTimer == null || inventory == null) return;
        if (!survivalTimer.inSafeZone)
        {
            // 안전지대 이탈 시 Ghost 강제 취소 (인벤토리 반환)
            if (_ghostMode) CancelGhost();
            return;
        }

        // ── Q키: Ghost 모드 토글 ────────────────────────────────────────
        if (Input.GetKeyDown(placeKey))
        {
            if (_ghostMode)
            {
                // 이미 Ghost 모드 → 취소
                CancelGhost();
            }
            else if (inventory.GetItemCount() > 0)
            {
                // Ghost 모드 진입
                EnterGhostMode();
            }
            return;
        }

        // ── Ghost 모드 중 매 프레임 처리 ───────────────────────────────
        if (_ghostMode && _ghostObj != null)
        {
            // R키: 90도 스냅 회전 (UpdateGhostPosition 전에 처리)
            if (Input.GetKeyDown(KeyCode.R))
            {
                _rotationOffset = (_rotationOffset + 90f) % 360f;
                Debug.Log($"[FurniturePlacement] 🔄 회전: {_rotationOffset}도");
            }

            UpdateGhostPosition();

            // 우클릭: 취소
            if (Input.GetMouseButtonDown(1))
            {
                CancelGhost();
                return;
            }

            // 좌클릭: 실제 배치 확정
            if (Input.GetMouseButtonDown(0))
            {
                PlaceFromGhost();
            }
        }

    }

    // ═══════════════════════════════════════════════════════════════════════
    //  GHOST 시스템
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Ghost 모드 진입. 인벤토리 마지막 슬롯을 '대기 상태'로 꺼내고 Ghost를 생성.
    /// DropLastSlot을 사용하므로 fake-null 복구 로직이 자동 적용됨.
    /// </summary>
    void EnterGhostMode()
    {
        _pendingSlot = inventory.DropLastSlot();
        if (_pendingSlot == null) return;

        _rotationOffset = 0f;         // 진입마다 회전 리셋
        CreateGhost(_pendingSlot);
        _ghostMode = true;
    }

    /// <summary>
    /// Ghost 오브젝트를 1회 생성.
    /// - Collider 전부 비활성화 (Raycast 방해 방지)
    /// - FurnitureItem 스크립트 제거 (상호작용 방지)
    /// - 모든 MeshRenderer를 ghostMaterial로 교체
    /// - sourceScale 적용 (프리뷰 크기 = 실제 배치 크기)
    /// - Y축 피벗 오프셋 캐시
    /// </summary>
    void CreateGhost(InventorySlot slot)
    {
        GameObject prefab = slot.sourcePrefab;
        if (prefab == null) return;

        // ── 1. Instantiate (위치는 UpdateGhostPosition에서 즉시 보정)
        _ghostObj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        _ghostObj.name = "Ghost_Preview";

        // ── 2. sourceScale 적용 (크기 오류 방지 - 핵심 버그 수정 보존)
        _ghostObj.transform.localScale = slot.sourceScale;

        // ── 3. Collider 전부 비활성화 (Raycast 방해 방지)
        foreach (var col in _ghostObj.GetComponentsInChildren<Collider>())
            col.enabled = false;

        // ── 4. FurnitureItem 등 상호작용 스크립트 제거
        var fi = _ghostObj.GetComponent<FurnitureItem>();
        if (fi != null) Object.Destroy(fi);

        // ── 5. Ghost 머티리얼 교체
        EnsureGhostMaterial();
        foreach (var mr in _ghostObj.GetComponentsInChildren<MeshRenderer>())
        {
            // 슬롯 수만큼 모두 교체 (멀티 서브메시 포함)
            var mats = new Material[mr.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
                mats[i] = _ghostMat;
            mr.materials = mats;
        }

        // ── 6. Y축 피벗 오프셋 캐시 (MeshFilter.sharedMesh.bounds 기반 - 기존 버그 수정 보존)
        _ghostPivotOffset = CalcPivotYOffset(prefab);

        Debug.Log($"[FurniturePlacement] 🔷 Ghost 생성: {slot.itemType} scale={slot.sourceScale}");
    }

    /// <summary>
    /// 매 프레임 Ghost의 Transform(위치·회전)만 갱신. Ghost 재생성 없음.
    /// 기존 MeshFilter.sharedMesh.bounds 기반 Y 오프셋 로직 그대로 적용.
    /// </summary>
    void UpdateGhostPosition()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        // 최대 4f 제한 (너무 멀리서 설치 방지)
        if (Physics.Raycast(ray, out RaycastHit hit, 4f, _groundMask))
        {
            Vector3 pos = hit.point;
            pos.y += _ghostPivotOffset; // 피벗 오프셋 보정 (땅에 파묻히지 않음)
            _ghostObj.transform.position = pos;
        }
        else
        {
            // Raycast 실패 시 플레이어 앞 + 발 높이 폴백
            Vector3 fallback = transform.position + transform.forward * placeDistance;
            fallback.y = transform.position.y - 0.5f + _ghostPivotOffset;
            _ghostObj.transform.position = fallback;
        }

        // 회전: 월드 90도 스냅 + R키 오프셋
        //   1) 플레이어 Y 각도를 90도 단위로 스냅 (월드 기준 정렬)
        //   2) R키 오프셋 추가
        float baseAngle   = Mathf.Round(transform.eulerAngles.y / 90f) * 90f;
        float finalAngle  = baseAngle + _rotationOffset;
        _ghostObj.transform.rotation = Quaternion.Euler(0f, finalAngle, 0f);
    }

    /// <summary>
    /// Ghost의 현재 position/rotation을 기존 안전한 스폰 로직에 넘겨 실제 가구 배치.
    /// fake-null 검사, sourceScale 복원, FurnitureItem 연동 등 기존 로직 완전 보존.
    /// </summary>
    void PlaceFromGhost()
    {
        if (_pendingSlot == null || _ghostObj == null) return;

        // Ghost에서 위치·회전 추출
        Vector3    spawnPos = _ghostObj.transform.position;
        Quaternion spawnRot = _ghostObj.transform.rotation;

        // Ghost 먼저 파괴 (실제 오브젝트와 겹치지 않도록)
        DestroyGhost();
        _ghostMode = false;

        // ── 기존 안전 스폰 로직 (단 한 줄도 수정하지 않음) ─────────────
        SpawnFurniture(_pendingSlot, spawnPos, spawnRot);
        _pendingSlot = null;
    }

    /// <summary>
    /// Ghost 취소. 대기 중이던 슬롯을 인벤토리에 반환하고 Ghost 파괴.
    /// </summary>
    void CancelGhost()
    {
        if (_pendingSlot != null)
        {
            // 인벤토리에 아이템 반환 (scale 포함)
            inventory.AddItem(_pendingSlot.itemType, _pendingSlot.sourcePrefab, _pendingSlot.sourceScale);
            Debug.Log($"[FurniturePlacement] 🚫 Ghost 취소 – 인벤토리 반환: {_pendingSlot.itemType}");
            _pendingSlot = null;
        }

        DestroyGhost();
        _ghostMode = false;
    }

    /// <summary>Ghost 오브젝트 즉시 파괴 (null 안전).</summary>
    void DestroyGhost()
    {
        if (_ghostObj != null)
        {
            Object.Destroy(_ghostObj);
            _ghostObj = null;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  실제 스폰 로직 (기존 PlaceFurniture에서 분리 – 로직 100% 보존)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 실제 가구 오브젝트를 씬에 스폰.
    /// 기존 PlaceFurniture()의 모든 안전 로직이 이 메서드에 보존되어 있음:
    ///   - fake-null 방어, sourceScale 복원, FurnitureItem 연동, Collider 보장 등
    /// Ghost 시스템과 독립적으로 작동 (Ghost에서 position/rotation만 받음).
    /// </summary>
    void SpawnFurniture(InventorySlot slot, Vector3 spawnPos, Quaternion spawnRot)
    {
        ItemType   itemType   = slot.itemType;
        GameObject prefab     = slot.sourcePrefab;
        Vector3    storedScale= slot.sourceScale; // ← 원본 크기 복원 (버그 수정 보존)

        GameObject placed;

        if (prefab != null)
        {
            // ✅ 원본 프리팹 그대로 Instantiate
            placed = Instantiate(prefab, spawnPos, spawnRot);
            placed.name = "Placed_" + itemType;
            placed.transform.localScale = storedScale; // ← 원본 크기 복원
            Debug.Log($"[FurniturePlacement] ✅ Place: {itemType} → prefab='{prefab.name}' scale={storedScale}");
        }
        else
        {
            // ⚠️ 안전 폴백: sourcePrefab을 끝내 찾지 못한 경우 임시 큐브 사용
            Debug.LogWarning($"[FurniturePlacement] ⚠️ '{itemType}' sourcePrefab 없음 – 임시 큐브 사용");
            placed = GameObject.CreatePrimitive(PrimitiveType.Cube);
            placed.name = "Placed_" + itemType;
            placed.transform.position   = spawnPos;
            placed.transform.rotation   = spawnRot;
            placed.transform.localScale = GetFurnitureScale(itemType);

            var rend = placed.GetComponent<Renderer>();
            if (rend != null) rend.sharedMaterial = GetCachedMaterial(itemType);
        }

        // ── 태그 보장
        placed.tag = "FurnitureItem";

        // ── Collider 보장 (OverlapSphere 감지에 필수)
        var col = placed.GetComponent<Collider>();
        if (col == null) col = placed.AddComponent<BoxCollider>();
        col.enabled = true;

        // ── FurnitureItem 컴포넌트 보장 + 프리팹 정보 이어받기
        FurnitureItem fi = placed.GetComponent<FurnitureItem>();
        if (fi == null) fi = placed.AddComponent<FurnitureItem>();
        fi.itemType     = itemType;
        fi.sourcePrefab = prefab;      // null이어도 할당 (Start에서 복구 시도)
        fi.sourceScale  = storedScale; // 크기도 보존

        if (cabinComfort != null)
            cabinComfort.OnFurniturePlaced(itemType);

        if (audioSource != null && placeSound != null)
            audioSource.PlayOneShot(placeSound);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  헬퍼 유틸리티
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 프리팹의 MeshFilter.sharedMesh.bounds 기반 Y 피벗 오프셋 계산.
    /// 기존 버그 수정 로직 그대로 보존 (Renderer.bounds 사용 안 함 – 부정확).
    /// </summary>
    static float CalcPivotYOffset(GameObject prefab)
    {
        float minLocalY = float.MaxValue;
        foreach (var mf in prefab.GetComponentsInChildren<MeshFilter>())
        {
            if (mf.sharedMesh == null) continue;
            float localBottom = mf.sharedMesh.bounds.center.y - mf.sharedMesh.bounds.extents.y;
            float worldBottom = mf.transform.localPosition.y + localBottom;
            if (worldBottom < minLocalY) minLocalY = worldBottom;
        }
        return minLocalY < float.MaxValue ? -minLocalY : 0f;
    }

    /// <summary>
    /// Ghost용 반투명 머티리얼을 1회 생성. 인스펙터에서 할당하면 그것을 사용.
    /// </summary>
    void EnsureGhostMaterial()
    {
        if (_ghostMat != null) return;

        if (ghostMaterial != null)
        {
            // 인스펙터 할당 머티리얼의 인스턴스 복사 (원본 수정 방지)
            _ghostMat = new Material(ghostMaterial);
        }
        else
        {
            // 자동 생성: URP Lit 또는 Standard + 반투명 설정
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            _ghostMat = new Material(shader);

            // 반투명 설정
            _ghostMat.SetFloat("_Surface", 1f);           // URP: Transparent
            _ghostMat.SetFloat("_Mode",    3f);           // Standard: Transparent
            _ghostMat.SetInt("_SrcBlend",  (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _ghostMat.SetInt("_DstBlend",  (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _ghostMat.SetInt("_ZWrite",    0);
            _ghostMat.EnableKeyword("_ALPHABLEND_ON");
            _ghostMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            _ghostMat.color = new Color(0.3f, 0.6f, 1f, 0.45f); // 반투명 청색
        }
    }

    // ── 폴백 큐브용 기존 헬퍼 (기존 코드 100% 보존) ──────────────────────
    Material GetCachedMaterial(ItemType type)
    {
        if (_cachedMaterials.TryGetValue(type, out Material mat) && mat != null)
            return mat;

        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        mat = new Material(shader);
        mat.color = GetFurnitureColor(type);
        _cachedMaterials[type] = mat;
        return mat;
    }

    Vector3 GetFurnitureScale(ItemType type)
    {
        switch (type)
        {
            case ItemType.OldChair:    return new Vector3(0.5f, 0.8f, 0.5f);
            case ItemType.WoodenTable: return new Vector3(1.2f, 0.6f, 0.8f);
            case ItemType.Bookshelf:   return new Vector3(0.8f, 1.5f, 0.3f);
            case ItemType.Lantern:     return new Vector3(0.2f, 0.4f, 0.2f);
            case ItemType.HeavyCrate:  return new Vector3(0.8f, 0.8f, 0.8f);
            case ItemType.Rug:         return new Vector3(1.5f, 0.05f, 1.0f);
            case ItemType.WallClock:   return new Vector3(0.3f, 0.3f, 0.05f);
            case ItemType.SmallDrawer: return new Vector3(0.6f, 0.7f, 0.4f);
            default:                   return Vector3.one * 0.5f;
        }
    }

    Color GetFurnitureColor(ItemType type)
    {
        switch (type)
        {
            case ItemType.OldChair:    return new Color(0.55f, 0.35f, 0.18f);
            case ItemType.WoodenTable: return new Color(0.6f,  0.4f,  0.2f);
            case ItemType.Bookshelf:   return new Color(0.45f, 0.3f,  0.15f);
            case ItemType.Lantern:     return new Color(1f,    0.85f, 0.4f);
            case ItemType.HeavyCrate:  return new Color(0.5f,  0.45f, 0.35f);
            case ItemType.Rug:         return new Color(0.7f,  0.2f,  0.15f);
            case ItemType.WallClock:   return new Color(0.3f,  0.25f, 0.2f);
            case ItemType.SmallDrawer: return new Color(0.5f,  0.35f, 0.2f);
            default:                   return Color.gray;
        }
    }
}
