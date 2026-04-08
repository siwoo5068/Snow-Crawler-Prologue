using UnityEngine;

public class CabinComfort : MonoBehaviour
{
    [Header("Comfort Settings")]
    public int maxFurnitureCount = 8;

    public int PlacedCount { get; private set; }
    public float ComfortRatio { get { return maxFurnitureCount > 0 ? (float)PlacedCount / maxFurnitureCount : 0f; } }

    public void OnFurniturePlaced(ItemType type)
    {
        PlacedCount++;
    }
}
