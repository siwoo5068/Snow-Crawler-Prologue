using UnityEngine;

public class FurniturePlacement : MonoBehaviour
{
    [Header("References")]
    public PlayerInventory inventory;
    public SurvivalTimer survivalTimer;
    public CabinComfort cabinComfort;

    [Header("Placement Settings")]
    public float placeDistance = 2f;
    public KeyCode placeKey = KeyCode.Q;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip placeSound;

    void Start()
    {
        if (inventory == null)
            inventory = GetComponent<PlayerInventory>();
        if (survivalTimer == null)
            survivalTimer = GetComponent<SurvivalTimer>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (survivalTimer == null || inventory == null) return;

        if (!survivalTimer.inSafeZone) return;

        if (Input.GetKeyDown(placeKey) && inventory.GetItemCount() > 0)
        {
            PlaceFurniture();
        }
    }

    void PlaceFurniture()
    {
        ItemType? dropped = inventory.DropLastItem();
        if (dropped == null) return;

        Vector3 spawnPos = transform.position + transform.forward * placeDistance;
        spawnPos.y = transform.position.y - 0.5f;

        GameObject placed = GameObject.CreatePrimitive(PrimitiveType.Cube);
        placed.name = "Placed_" + dropped.Value;
        placed.transform.position = spawnPos;
        placed.transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        placed.transform.localScale = GetFurnitureScale(dropped.Value);
        placed.tag = "Untagged";

        var renderer = placed.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = GetFurnitureColor(dropped.Value);
        }

        // SnowBurial이 붙지 않도록 Untagged (안전지대 내 영구 배치)
        var col = placed.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        if (cabinComfort != null)
            cabinComfort.OnFurniturePlaced(dropped.Value);

        if (audioSource != null && placeSound != null)
            audioSource.PlayOneShot(placeSound);
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
            case ItemType.WoodenTable: return new Color(0.6f, 0.4f, 0.2f);
            case ItemType.Bookshelf:   return new Color(0.45f, 0.3f, 0.15f);
            case ItemType.Lantern:     return new Color(1f, 0.85f, 0.4f);
            case ItemType.HeavyCrate:  return new Color(0.5f, 0.45f, 0.35f);
            case ItemType.Rug:         return new Color(0.7f, 0.2f, 0.15f);
            case ItemType.WallClock:   return new Color(0.3f, 0.25f, 0.2f);
            case ItemType.SmallDrawer: return new Color(0.5f, 0.35f, 0.2f);
            default:                   return Color.gray;
        }
    }
}
