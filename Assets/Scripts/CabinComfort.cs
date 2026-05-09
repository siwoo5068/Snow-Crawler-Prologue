using UnityEngine;

public class CabinComfort : MonoBehaviour
{
    [Header("Comfort Settings")]
    public int maxFurnitureCount = 8;

    public int PlacedCount { get; private set; }
    public float ComfortRatio { get { return maxFurnitureCount > 0 ? (float)PlacedCount / maxFurnitureCount : 0f; } }

    /// <summary>
    /// 안락도 변화 시 호출되는 이벤트. EndingManager가 구독하여 2막 트리거 판정에 사용.
    /// </summary>
    public event System.Action<float> OnComfortChanged;

    public void OnFurniturePlaced(ItemType type)
    {
        PlacedCount = Mathf.Min(PlacedCount + 1, maxFurnitureCount);
        OnComfortChanged?.Invoke(ComfortRatio);
    }

    /// <summary>
    /// 설치된 가구를 다시 주웠을 때 호출 – PlacedCount 감소
    /// </summary>
    public void OnFurniturePickedUp(ItemType type)
    {
        PlacedCount = Mathf.Max(0, PlacedCount - 1);
        OnComfortChanged?.Invoke(ComfortRatio);
    }
}
