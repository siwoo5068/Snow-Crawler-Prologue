using UnityEngine;

public class CabinComfort : MonoBehaviour
{
    [Header("Comfort Settings")]
    public int maxFurnitureCount = 8;

    public int PlacedCount { get; private set; }
    public float ComfortRatio { get { return maxFurnitureCount > 0 ? (float)PlacedCount / maxFurnitureCount : 0f; } }

    public void OnFurniturePlaced(ItemType type)
    {
        PlacedCount = Mathf.Min(PlacedCount + 1, maxFurnitureCount);
    }

    /// <summary>
    /// 설치된 가구를 다시 주웠을 때 호출 – PlacedCount 감소
    /// </summary>
    public void OnFurniturePickedUp(ItemType type)
    {
        PlacedCount = Mathf.Max(0, PlacedCount - 1);
    }
}
