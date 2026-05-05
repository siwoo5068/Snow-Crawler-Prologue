using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif
// PlayerInventory – struct 복사 문제 및 Unity fake-null 방지 버전

// ─────────────────────────────────────────────────────────────────
//  인벤토리 슬롯 – ItemType + 원본 프리팹을 함께 보관
//  class(참조 타입) 사용 – struct 값 복사 시 UnityEngine.Object 참조 유실 방지
// ─────────────────────────────────────────────────────────────────
public class InventorySlot
{
    public ItemType   itemType;
    public GameObject sourcePrefab;   // null이면 프리팹 정보 없음
    public Vector3    sourceScale;    // 원본 오브젝트의 localScale (크기 보존용)

    public InventorySlot(ItemType type, GameObject prefab)
    {
        itemType    = type;
        sourcePrefab = prefab;
        sourceScale  = Vector3.one; // 기본값
    }

    public InventorySlot(ItemType type, GameObject prefab, Vector3 scale)
    {
        itemType     = type;
        sourcePrefab = prefab;
        sourceScale  = scale;
    }
}

public class PlayerInventory : MonoBehaviour
{
    [Header("Weight Settings")]
    public float maxWeight = 15f;

    public float TotalWeight { get; private set; }

    // 내부 저장소 – InventorySlot 목록 (ItemType + 원본 프리팹)
    private List<InventorySlot> slots = new List<InventorySlot>();

    // ── 무게 확인 ────────────────────────────────────────────────
    public bool CanCarry(ItemType type)
    {
        if (!ItemDatabase.Weight.TryGetValue(type, out float w)) return true;
        return TotalWeight + w <= maxWeight;
    }

    // ── 아이템 추가 (프리팸 없는 구형 호환용) ─────────────────────────────
    public bool AddItem(ItemType type)
        => AddItem(type, null, Vector3.one);

    // ── 아이템 추가 (원본 프리팸 포함, scale 없는 구형) ───────────────────
    public bool AddItem(ItemType type, GameObject sourcePrefab)
        => AddItem(type, sourcePrefab, Vector3.one);

    // ── 아이템 추가 (원본 프리팸 + scale 포함) ────────────────────────
    public bool AddItem(ItemType type, GameObject sourcePrefab, Vector3 sourceScale)
    {
        if (!CanCarry(type)) return false;

        // ── Unity fake-null 방지: C# 레퍼런스는 살아있어도
        //    Unity 네이티브 오브젝트가 Destroy됐으면 null로 처리
        bool prefabIsValid = !ReferenceEquals(sourcePrefab, null) && sourcePrefab != null;

        if (!prefabIsValid)
        {
            // ── 방어적 복구: sourcePrefab이 null이거나 fake-null이면 에셋 DB에서 자동 탐색
            sourcePrefab = FindPrefabByType(type);
            if (sourcePrefab != null)
                Debug.Log($"[Inventory] ✅ 자동 복구 성공: {type} → '{sourcePrefab.name}'");
            else
                Debug.LogWarning($"[Inventory] ⚠️ 자동 복구 실패: {type} – Drop 시 임시 큐브 사용");
        }
        else
        {
            Debug.Log($"[Inventory.AddItem] ✅ type={type} prefab='{sourcePrefab.name}' scale={sourceScale}");
        }

        // ── 슬롯 생성 및 리스트에 추가 (class이므로 참조 타입 – 값 복사 없음)
        var slot = new InventorySlot(type, sourcePrefab, sourceScale);
        slots.Add(slot);

        // ── 저장 직후 상태 확인 (저장된 참조가 여전히 유효한지 이중 검증)
        var stored = slots[slots.Count - 1];
        bool storedValid = !ReferenceEquals(stored.sourcePrefab, null) && stored.sourcePrefab != null;
        Debug.Log($"[Inventory.AddItem] 저장 확인: type={stored.itemType} " +
                  $"prefab={(storedValid ? stored.sourcePrefab.name : "NULL")} " +
                  $"scale={stored.sourceScale} (총 {slots.Count}개)");

        if (ItemDatabase.Weight.TryGetValue(type, out float w))
            TotalWeight += w;

        return true;
    }


    // ── 타입 이름으로 프리팹 자동 탐색 (방어적 복구용) ───────────
    static GameObject FindPrefabByType(ItemType type)
    {
        string typeName = type.ToString().ToLower(); // e.g. "bed", "chair"

#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets(
            "t:Prefab", new[] { "Assets/Furniture Mega Pack/Prefabs" }
        );
        foreach (string guid in guids)
        {
            string path  = AssetDatabase.GUIDToAssetPath(guid);
            string pName = Path.GetFileNameWithoutExtension(path).ToLower();

            // 타입명으로 시작하는 첫 번째 프리팹 반환
            if (pName.StartsWith(typeName))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null) return prefab;
            }
        }
#endif
        // 빌드 폴백: Resources/Furniture/{TypeName}
        return Resources.Load<GameObject>($"Furniture/{type}");
    }

    // ── 특정 타입 제거 (무게만 감소) ─────────────────────────────
    public void RemoveItem(ItemType type)
    {
        for (int i = slots.Count - 1; i >= 0; i--)
        {
            if (slots[i].itemType == type)
            {
                slots.RemoveAt(i);
                if (ItemDatabase.Weight.TryGetValue(type, out float w))
                    TotalWeight = Mathf.Max(0f, TotalWeight - w);
                return;
            }
        }
    }

    // ── 마지막 슬롯 꺼내기 (ItemType만 – 하위 호환) ─────────────
    public ItemType? DropLastItem()
    {
        InventorySlot slot = DropLastSlot();
        return slot != null ? slot.itemType : (ItemType?)null;
    }

    // ── 마지막 슬롯 꺼내기 (프리팹 포함, class이므로 null 반환) ──
    public InventorySlot DropLastSlot()
    {
        if (slots.Count == 0) return null;

        InventorySlot last = slots[slots.Count - 1];

        // ── Unity fake-null 이중 검사: 저장 후 네이티브 오브젝트가 파괴됐을 경우 대비
        bool prefabValid = !ReferenceEquals(last.sourcePrefab, null) && last.sourcePrefab != null;

        if (!prefabValid && last.sourcePrefab != null)
        {
            // fake-null 감지됨 – 저장된 C# 레퍼런스는 있지만 Unity 오브젝트는 파괴된 상태
            Debug.LogWarning($"[Inventory.DropLastSlot] ⚠️ fake-null 감지: type={last.itemType} " +
                             $"– sourcePrefab C# 레퍼런스 존재하나 Unity 오브젝트 파괴됨. 자동 복구 시도.");
            last.sourcePrefab = null; // 명시적으로 null 처리
        }

        // ── prefab이 null이면 자동 복구 재시도
        if (last.sourcePrefab == null)
        {
            last.sourcePrefab = FindPrefabByType(last.itemType);
            if (last.sourcePrefab != null)
                Debug.Log($"[Inventory.DropLastSlot] ✅ 자동 복구 성공: {last.itemType} → '{last.sourcePrefab.name}'");
            else
                Debug.LogWarning($"[Inventory.DropLastSlot] ⚠️ 자동 복구 실패: {last.itemType}");
        }

        // ── 최종 상태 출력
        Debug.Log($"[Inventory.DropLastSlot] ▶ type={last.itemType} " +
                  $"prefab={(last.sourcePrefab != null ? last.sourcePrefab.name : "NULL")}");

        slots.RemoveAt(slots.Count - 1);

        if (ItemDatabase.Weight.TryGetValue(last.itemType, out float w))
            TotalWeight = Mathf.Max(0f, TotalWeight - w);

        return last;
    }

    public int GetItemCount() => slots.Count;

    // IReadOnlyList<ItemType> – 기존 UI/HUD 스크립트 호환
    public IReadOnlyList<ItemType> GetItems()
    {
        var types = new List<ItemType>(slots.Count);
        foreach (var s in slots) types.Add(s.itemType);
        return types;
    }

    public void ResetState()
    {
        slots.Clear();
        TotalWeight = 0f;
    }
}
