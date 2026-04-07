using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public float TotalWeight { get; private set; }

    private List<ItemType> items = new List<ItemType>();

    public void AddItem(ItemType type)
    {
        items.Add(type);

        if (ItemDatabase.Weight.TryGetValue(type, out float w))
            TotalWeight += w;
    }

    public void RemoveItem(ItemType type)
    {
        if (!items.Remove(type)) return;

        if (ItemDatabase.Weight.TryGetValue(type, out float w))
            TotalWeight = Mathf.Max(0f, TotalWeight - w);
    }

    public int GetItemCount() => items.Count;

    public IReadOnlyList<ItemType> GetItems() => items;
}
