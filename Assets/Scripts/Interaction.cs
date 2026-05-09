using UnityEngine;

public class Interaction : MonoBehaviour
{
    public float interactRange = 3f;
    public SurvivalTimer timer;
    public PlayerInventory inventory;
    public CabinComfort cabinComfort;   // 줍기 시 배치 카운트 동기화

    void Start()
    {
        if (timer == null) timer = GetComponent<SurvivalTimer>();
        if (inventory == null) inventory = GetComponent<PlayerInventory>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        Vector3 radarCenter = transform.position + Vector3.up;
        Collider[] hitColliders = Physics.OverlapSphere(radarCenter, interactRange);

        Collider closestCollider = null;
        float minDistance = Mathf.Infinity;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("FurnitureItem") ||
                hitCollider.CompareTag("TimeItem") ||
                hitCollider.CompareTag("MaterialItem") ||
                hitCollider.CompareTag("CraftingTable"))
            {
                float distance = Vector3.Distance(radarCenter, hitCollider.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCollider = hitCollider;
                }
            }
        }

        if (closestCollider == null) return;

        if (closestCollider.CompareTag("TimeItem"))
        {
            if (timer != null) timer.AddTime(5f);
            Destroy(closestCollider.gameObject);
        }
        else if (closestCollider.CompareTag("MaterialItem"))
        {
            if (timer != null) timer.AddMaterial(1);
            Destroy(closestCollider.gameObject);
        }
        else if (closestCollider.CompareTag("CraftingTable"))
        {
            if (timer != null) timer.UpgradeCoat();
        }
        else if (closestCollider.CompareTag("FurnitureItem"))
        {
            FurnitureItem furniture = closestCollider.GetComponent<FurnitureItem>();
            if (furniture != null && inventory != null)
            {
                // ── 핵심: Destroy 전에 로컬 변수로 캐시
                //    Destroy 후 furniture.sourcePrefab에 접근하면 Unity fake-null 가능
                ItemType   cachedType   = furniture.itemType;
                GameObject cachedPrefab = furniture.sourcePrefab;
                Vector3    cachedScale  = furniture.transform.localScale; // ← 원본 크기 저장

                // ── 진단: 줍기 시점 상태 출력
                if (cachedPrefab == null)
                    Debug.LogWarning(
                        $"[Interaction] ⚠️ '{closestCollider.gameObject.name}'의 " +
                        $"FurnitureItem.sourcePrefab이 null!\n" +
                        "→ Tools > Furniture Pack Fixer > Postprocessor – Re-apply All Prefabs 실행");
                else
                    Debug.Log($"[Interaction] ✅ 줍기: {cachedType} sourcePrefab='{cachedPrefab.name}' scale={cachedScale}");

                // ── 캐시된 값으로 AddItem 호출 (scale 포함)
                if (inventory.AddItem(cachedType, cachedPrefab, cachedScale))
                {
                    bool wasPlaced = closestCollider.gameObject.name.StartsWith("Placed_");
                    if (wasPlaced && cabinComfort != null)
                        cabinComfort.OnFurniturePickedUp(cachedType);

                    Destroy(closestCollider.gameObject);
                }
            }
        }
    }
}