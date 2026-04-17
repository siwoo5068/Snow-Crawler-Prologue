using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Weight Settings")]
    public float maxWeight = 15f;

    public float TotalWeight { get; private set; }

    private List<ItemType> items = new List<ItemType>();

    public bool CanCarry(ItemType type)
    {
        if (!ItemDatabase.Weight.TryGetValue(type, out float w)) return true;
        return TotalWeight + w <= maxWeight;
    }

    public bool AddItem(ItemType type)
    {
        if (!CanCarry(type)) return false;

        items.Add(type);
        if (ItemDatabase.Weight.TryGetValue(type, out float w))
            TotalWeight += w;
        return true;
    }

    public void RemoveItem(ItemType type)
    {
        if (!items.Remove(type)) return;

        if (ItemDatabase.Weight.TryGetValue(type, out float w))
            TotalWeight = Mathf.Max(0f, TotalWeight - w);
    }

    public ItemType? DropLastItem()
    {
        if (items.Count == 0) return null;
        var last = items[items.Count - 1];
        RemoveItem(last);
        return last;
    }

    public int GetItemCount() => items.Count;

    public IReadOnlyList<ItemType> GetItems() => items;

    public void ResetState()
    {
        items.Clear();
        TotalWeight = 0f;
    }
}
