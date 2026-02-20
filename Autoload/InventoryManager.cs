using System.Collections.Generic;
using Godot;

namespace RotOfTime.Autoload;

/// <summary>
///     Manages all collectible items as a Dictionary&lt;string, int&gt;.
///     Plain C# class owned by GameManager (not a Godot Node).
///
///     Known item IDs:
///       "resonance" — collected resonance (fungible, activatable at bonfire)
///       "elevation" — elevation item dropped by any boss (genérico, no por elevación específica)
/// </summary>
public class InventoryManager
{
    private Dictionary<string, int> _items = new();

    public void AddItem(string id, int amount = 1)
    {
        _items.TryGetValue(id, out int current);
        _items[id] = current + amount;
    }

    public bool RemoveItem(string id, int amount = 1)
    {
        if (!HasItem(id, amount)) return false;
        _items[id] -= amount;
        if (_items[id] <= 0) _items.Remove(id);
        return true;
    }

    public bool HasItem(string id, int amount = 1)
    {
        return _items.TryGetValue(id, out int count) && count >= amount;
    }

    public int GetQuantity(string id)
    {
        return _items.TryGetValue(id, out int count) ? count : 0;
    }

    public Dictionary<string, int> GetAllItems() => new(_items);

    public void Load(Dictionary<string, int> items)
    {
        _items = items != null ? new Dictionary<string, int>(items) : new Dictionary<string, int>();
    }
}
