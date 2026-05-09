using UnityEngine;

public class CabinComfort : MonoBehaviour
{
    [Header("Comfort Settings")]
    [Tooltip("안락도 최대치 (이 값 대비 비율로 엔딩 조건 판정)")]
    public float maxComfortValue = 8f;

    /// <summary>현재 누적 안락도 점수</summary>
    public float CurrentComfort { get; private set; }

    /// <summary>배치된 가구 개수</summary>
    public int PlacedCount { get; private set; }

    /// <summary>안락도 비율 (0~1). EndingManager가 이 값을 체크.</summary>
    public float ComfortRatio
    {
        get { return maxComfortValue > 0f ? Mathf.Clamp01(CurrentComfort / maxComfortValue) : 0f; }
    }

    /// <summary>
    /// 안락도 변화 시 호출되는 이벤트. EndingManager가 구독하여 2막 트리거 판정에 사용.
    /// </summary>
    public event System.Action<float> OnComfortChanged;

    /// <summary>가구 배치 시 호출 — 가구 종류에 따라 안락도 기여량이 다름</summary>
    public void OnFurniturePlaced(ItemType type)
    {
        float value = GetComfortValue(type);
        CurrentComfort += value;
        PlacedCount++;

        Debug.Log($"[CabinComfort] {type} 배치 (+{value:F1}) → 안락도: {CurrentComfort:F1}/{maxComfortValue} ({ComfortRatio * 100f:F0}%)");
        OnComfortChanged?.Invoke(ComfortRatio);
    }

    /// <summary>설치된 가구를 다시 주웠을 때 호출 — 안락도 감소</summary>
    public void OnFurniturePickedUp(ItemType type)
    {
        float value = GetComfortValue(type);
        CurrentComfort = Mathf.Max(0f, CurrentComfort - value);
        PlacedCount = Mathf.Max(0, PlacedCount - 1);

        Debug.Log($"[CabinComfort] {type} 수거 (-{value:F1}) → 안락도: {CurrentComfort:F1}/{maxComfortValue} ({ComfortRatio * 100f:F0}%)");
        OnComfortChanged?.Invoke(ComfortRatio);
    }

    /// <summary>ItemDatabase에서 안락도 가중치 조회, 없으면 기본값 1.0</summary>
    float GetComfortValue(ItemType type)
    {
        if (ItemDatabase.ComfortValue.ContainsKey(type))
            return ItemDatabase.ComfortValue[type];
        return 1.0f;
    }
}
