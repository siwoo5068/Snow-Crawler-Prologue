using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

/// <summary>
/// 가구 아이템 컴포넌트.
/// Awake 시 sourcePrefab이 비어있으면 오브젝트 이름으로 에셋을 자동 탐색하여 복구합니다.
/// </summary>
public class FurnitureItem : MonoBehaviour
{
    public ItemType itemType;

    [Tooltip("원본 프리팹 레퍼런스 – Drop·Place 시 이 프리팹을 그대로 다시 스폰합니다.")]
    public GameObject sourcePrefab;

    [Tooltip("원본 오브젝트의 localScale – 줍기 시 저장, 배치 시 복원합니다.")]
    public Vector3 sourceScale = Vector3.one;

    void Awake()
    {
        // ── Awake에서는 진짜 null인 경우만 복구 시도
        //    (이시점에 FurnitureDrop이 아직 sourcePrefab을 주입하지 않았을 수 있음)
        //    Start()에서 한 번 더 확인함
        bool prefabAlreadySet = !ReferenceEquals(sourcePrefab, null) && sourcePrefab != null;
        if (!prefabAlreadySet)
        {
            // Awake 시점에 아직 null이면 일단 복구 시도
            // (FurnitureDrop이 Instantiate 후 할당하는 경우 Start에서 덮어쓸)
            RecoverSourcePrefab();
        }
        else
        {
            Debug.Log($"[FurnitureItem.Awake] ✅ sourcePrefab 이미 할당됨: '{sourcePrefab.name}' (복구 실행 방지)");
        }
    }

    void Start()
    {
        // ── Start에서 다시 확인: FurnitureDrop이 Instantiate 후 sourcePrefab을
        //    Awake 이후에 할당했을 경우 이 시점에 유효함
        bool prefabValid = !ReferenceEquals(sourcePrefab, null) && sourcePrefab != null;
        if (!prefabValid)
        {
            Debug.LogWarning($"[FurnitureItem.Start] ⚠️ '{gameObject.name}' sourcePrefab 여전히 null – 마지막 복구 시도");
            RecoverSourcePrefab();
        }
        else
        {
            Debug.Log($"[FurnitureItem.Start] ✅ '{gameObject.name}' sourcePrefab={sourcePrefab.name}");
        }
    }

    /// <summary>
    /// 오브젝트 이름 → 에셋 DB에서 원본 프리팹 자동 탐색 후 sourcePrefab에 할당.
    /// 에디터: AssetDatabase / 빌드: Resources.Load 폴백
    /// </summary>
    void RecoverSourcePrefab()
    {
        // Instantiate / 배치 접두사 제거 → 원본 프리팹 이름 추출
        string rawName = gameObject.name
            .Replace("(Clone)", "")
            .Replace("Dropped_", "")
            .Replace("Placed_",  "")
            .Trim();

        // ItemType 이름 접두사로 찾기 (Placed_Bed → "Bed" → Bed02, Bed11 중 첫 번째)
        // 정확한 이름이 있으면 우선 사용
        string exactName = rawName;

#if UNITY_EDITOR
        // ── 1차: 정확한 이름 일치 탐색
        sourcePrefab = FindPrefabInEditor(exactName, exact: true);

        // ── 2차: 타입 이름 접두사 탐색 (Placed_Bed 처럼 정확한 프리팹명이 아닐 때)
        if (sourcePrefab == null)
            sourcePrefab = FindPrefabInEditor(exactName, exact: false);

        if (sourcePrefab != null)
            Debug.Log($"[FurnitureItem.Awake] ✅ 자동 복구 성공: '{gameObject.name}' → '{sourcePrefab.name}'");
        else
            Debug.LogWarning($"[FurnitureItem.Awake] ⚠️ 자동 복구 실패: '{gameObject.name}' – Inspector Source Prefab 직접 할당 필요");
#else
        // 빌드: Resources/Furniture/ 폴더 탐색 (해당 폴더에 프리팹 복사 필요)
        sourcePrefab = Resources.Load<GameObject>($"Furniture/{exactName}");
        if (sourcePrefab != null)
            Debug.Log($"[FurnitureItem.Awake] ✅ Resources 복구 성공: '{exactName}'");
        else
            Debug.LogWarning($"[FurnitureItem.Awake] ⚠️ Resources 복구 실패: 'Furniture/{exactName}'");
#endif
    }

#if UNITY_EDITOR
    static GameObject FindPrefabInEditor(string name, bool exact)
    {
        string[] guids = AssetDatabase.FindAssets(
            $"t:Prefab {name}",
            new[] { "Assets/Furniture Mega Pack/Prefabs" }
        );

        foreach (string guid in guids)
        {
            string path   = AssetDatabase.GUIDToAssetPath(guid);
            string pName  = Path.GetFileNameWithoutExtension(path);

            bool match = exact
                ? pName.Equals(name, System.StringComparison.OrdinalIgnoreCase)
                : pName.StartsWith(name, System.StringComparison.OrdinalIgnoreCase);

            if (match)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null) return prefab;
            }
        }
        return null;
    }
#endif
}
