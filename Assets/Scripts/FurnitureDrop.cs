using UnityEngine;

public class FurnitureDrop : MonoBehaviour
{
    [Header("References")]
    public PlayerInventory inventory;

    [Header("Drop Settings")]
    public float dropDistance  = 2f;
    public float snowBurialTime = 20f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip   dropSound;

    void Start()
    {
        if (inventory   == null) inventory   = GetComponent<PlayerInventory>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
            TryDrop();
    }

    void TryDrop()
    {
        if (inventory == null)
        {
            Debug.LogError("[FurnitureDrop] inventory 레퍼런스가 null입니다!");
            return;
        }

        // ── 슬롯 단위로 꺼내기 (ItemType + 원본 프리팹 함께)
        InventorySlot slot = inventory.DropLastSlot();
        if (slot == null)
        {
            Debug.LogWarning("[FurnitureDrop] 인벤토리가 비어있습니다.");
            return;
        }

        ItemType   itemType    = slot.itemType;
        GameObject prefab      = slot.sourcePrefab;
        Vector3    storedScale = slot.sourceScale; // ← 원본 크기 복원

        // ── 드롭 방향 결정
        //    카메라 forward의 수평 성분을 사용 → 하늘/땅을 보고 있어도 '앞'으로 버림
        Camera cam = Camera.main;
        Vector3 dropDir;
        if (cam != null)
        {
            // 카메라 forward에서 Y 제거 후 정규화 → 항상 수평 방향
            dropDir = cam.transform.forward;
            dropDir.y = 0f;
            if (dropDir.sqrMagnitude < 0.001f)          // 정수리/발밑을 볼 때
                dropDir = transform.forward;             // 플레이어 바디 방향 폴백
            dropDir.Normalize();
        }
        else
        {
            dropDir = transform.forward;
        }

        // ── 스폰 위치: 플레이어 허리 높이에서 앞 방향으로 Raycast → 바닥 충돌점
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f; // 허리 높이
        int groundMask = ~(LayerMask.GetMask("Player", "FurnitureItem", "Ignore Raycast"));

        Vector3 spawnPos;

        // 1차: 앞 방향 + 아래로 비스듬히 Raycast (dropDistance 앞 지점의 바닥 감지)
        Vector3 targetPoint = rayOrigin + dropDir * dropDistance;
        Ray downRay = new Ray(targetPoint + Vector3.up * 2f, Vector3.down);

        if (Physics.Raycast(downRay, out RaycastHit downHit, 10f, groundMask))
        {
            spawnPos = downHit.point;
            Debug.Log($"[FurnitureDrop] ✅ 바닥 감지: {downHit.collider.name} y={downHit.point.y:F2}");
        }
        else
        {
            // 2차 폴백: 플레이어 앞 + 발 높이 (Raycast 실패 시)
            spawnPos   = transform.position + dropDir * dropDistance;
            spawnPos.y = transform.position.y;
            Debug.LogWarning($"[FurnitureDrop] ⚠️ Raycast 실패 – 폴백 위치 사용 y={spawnPos.y:F2}");
        }

        // ── 프리팹 피벗↔바닥 오프셋 계산 (MeshFilter 로컬 bounds 기준)
        float yOffset = 0f;
        if (prefab != null)
        {
            float minLocalY = float.MaxValue;
            foreach (var mf in prefab.GetComponentsInChildren<MeshFilter>())
            {
                if (mf.sharedMesh == null) continue;
                float localBottom = mf.sharedMesh.bounds.center.y - mf.sharedMesh.bounds.extents.y;
                float worldBottom = mf.transform.localPosition.y + localBottom;
                if (worldBottom < minLocalY) minLocalY = worldBottom;
            }
            if (minLocalY < float.MaxValue)
                yOffset = -minLocalY;
        }
        spawnPos.y += yOffset;

        // ── 회전: Y축만 플레이어 방향 따라가기
        Quaternion spawnRot = Quaternion.LookRotation(dropDir, Vector3.up);

        GameObject droppedObj;

        if (prefab != null)
        {
            // ✅ 원본 프리팹 그대로 Instantiate
            droppedObj = Instantiate(prefab, spawnPos, spawnRot);
            droppedObj.name = "Dropped_" + itemType;
            droppedObj.transform.localScale = storedScale; // ← 원본 크기 복원
            Debug.Log($"[FurnitureDrop] ✅ Drop: {itemType} → prefab='{prefab.name}' scale={storedScale}");
        }
        else
        {
            // ⚠️ 안전 폴백: sourcePrefab을 끝내 찾지 못한 경우 임시 큐브 스폰
            Debug.LogWarning($"[FurnitureDrop] ⚠️ '{itemType}' sourcePrefab 없음 – 임시 큐브 사용");
            droppedObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            droppedObj.name = "Dropped_" + itemType;
            droppedObj.transform.position   = spawnPos;
            droppedObj.transform.rotation   = spawnRot;
            droppedObj.transform.localScale = Vector3.one * 0.6f;
        }

        // ── FurnitureItem 확인 / 보완 + 프리팹 정보 이어받기
        //    ⚠️ AddComponent는 즉시 Awake를 호출함 → sourcePrefab은 그 다음에 할당됨
        //    ∴ FurnitureItem.Start()에서 null임을 감지하고 재복구하는 것이 안전
        FurnitureItem fi = droppedObj.GetComponent<FurnitureItem>();
        if (fi == null) fi = droppedObj.AddComponent<FurnitureItem>();

        // ── prefab을 null이 아닌 값으로 강제 할당 (프리팹이 유효한 경우)
        //    prefab이 null이면 자동 복구는 FurnitureItem.Start에서 처리
        fi.itemType     = itemType;
        fi.sourcePrefab = prefab;   // null이어도 할당 (버퍼로 덮어씀)
        fi.sourceScale  = storedScale; // 크기도 보존

        // ── 할당 후 즉시 확인 (Debug만: Start 호출 전)
        bool fiPrefabOk = !ReferenceEquals(fi.sourcePrefab, null) && fi.sourcePrefab != null;
        Debug.Log($"[FurnitureDrop] 드롭 후 FurnitureItem 상태: itemType={fi.itemType} " +
                  $"sourcePrefab={(fiPrefabOk ? fi.sourcePrefab.name : "NULL")}");

        // ── 태그 보장
        droppedObj.tag = "FurnitureItem";

        // ── Collider 보장 (OverlapSphere 감지에 필수)
        var col = droppedObj.GetComponent<Collider>();
        if (col == null) col = droppedObj.AddComponent<BoxCollider>();
        col.enabled = true;

        // ── 눈 속 묻히기 타이머
        SnowBurial burial = droppedObj.AddComponent<SnowBurial>();
        burial.burialTime = snowBurialTime;

        if (audioSource != null && dropSound != null)
            audioSource.PlayOneShot(dropSound);
    }
}
