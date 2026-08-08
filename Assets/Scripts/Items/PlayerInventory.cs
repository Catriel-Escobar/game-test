using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private readonly List<ItemStack> _stacks = new List<ItemStack>();

    public event Action OnInventoryChanged;

    public int Count => _stacks.Count;

    public IReadOnlyList<ItemStack> Stacks => _stacks;

    public void AddItem(string itemId, int count = 1)
    {
        if (string.IsNullOrEmpty(itemId) || count <= 0) return;

        ItemStack stack = FindStack(itemId);
        if (stack != null)
        {
            stack.count += count;
        }
        else
        {
            _stacks.Add(new ItemStack(itemId, count));
        }

        OnInventoryChanged?.Invoke();
    }

    public bool RemoveItem(string itemId, int count = 1)
    {
        if (string.IsNullOrEmpty(itemId) || count <= 0) return false;

        ItemStack stack = FindStack(itemId);
        if (stack == null || stack.count < count) return false;

        stack.count -= count;
        if (stack.count <= 0)
            _stacks.Remove(stack);

        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool HasItem(string itemId)
    {
        ItemStack stack = FindStack(itemId);
        return stack != null && stack.count > 0;
    }

    public int GetCount(string itemId)
    {
        ItemStack stack = FindStack(itemId);
        return stack != null ? stack.count : 0;
    }

    private ItemStack FindStack(string itemId)
    {
        for (int i = 0; i < _stacks.Count; i++)
        {
            if (_stacks[i].itemId == itemId)
                return _stacks[i];
        }

        return null;
    }
}
