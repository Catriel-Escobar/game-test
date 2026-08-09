using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private readonly List<ItemStack> _stacks = new List<ItemStack>();

    public event Action OnInventoryChanged;

    public int Count => _stacks.Count;

    public IReadOnlyList<ItemStack> Stacks => _stacks;

    public void AddItem(string itemId, int count = 1, ItemAffix[] affixes = null, string instanceId = null)
    {
        if (string.IsNullOrEmpty(itemId) || count <= 0) return;

        if (!string.IsNullOrEmpty(instanceId))
        {
            _stacks.Add(new ItemStack(itemId, count, CloneAffixes(affixes), instanceId));
            OnInventoryChanged?.Invoke();
            return;
        }

        int maxStack = GetMaxStackSize(itemId);
        if (maxStack > 1)
        {
            AddStacked(itemId, count, affixes, maxStack);
        }
        else
        {
            for (int i = 0; i < count; i++)
                _stacks.Add(new ItemStack(itemId, 1, CloneAffixes(affixes)));
        }

        OnInventoryChanged?.Invoke();
    }

    private void AddStacked(string itemId, int count, ItemAffix[] affixes, int maxStack)
    {
        string affixKey = AffixKeyOf(affixes);

        while (count > 0)
        {
            ItemStack target = FindPartialStack(itemId, affixKey, maxStack);
            if (target == null)
            {
                int add = Mathf.Min(maxStack, count);
                _stacks.Add(new ItemStack(itemId, add, CloneAffixes(affixes)));
                count -= add;
            }
            else
            {
                int space = maxStack - target.count;
                int add = Mathf.Min(space, count);
                target.count += add;
                count -= add;
            }
        }
    }

    private ItemStack FindPartialStack(string itemId, string affixKey, int maxStack)
    {
        for (int i = 0; i < _stacks.Count; i++)
        {
            ItemStack stack = _stacks[i];
            if (stack.itemId == itemId && stack.count < maxStack && stack.GetAffixKey() == affixKey)
                return stack;
        }

        return null;
    }

    private static int GetMaxStackSize(string itemId)
    {
        Item item = ConfigBoostrap.Current?.ItemsConfig != null
            ? FindItem(itemId, ConfigBoostrap.Current.ItemsConfig.items)
            : null;
        return item?.maxStackSize ?? 0;
    }

    private static Item FindItem(string itemId, Item[] items)
    {
        if (items == null) return null;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null && items[i].id == itemId)
                return items[i];
        }

        return null;
    }

    public bool RemoveItem(string itemId, int count = 1, ItemAffix[] affixes = null)
    {
        if (string.IsNullOrEmpty(itemId) || count <= 0) return false;

        string affixKey = AffixKeyOf(affixes);
        int remaining = count;

        for (int i = _stacks.Count - 1; i >= 0 && remaining > 0; i--)
        {
            ItemStack stack = _stacks[i];
            if (stack.itemId != itemId || stack.GetAffixKey() != affixKey) continue;

            if (stack.count <= remaining)
            {
                remaining -= stack.count;
                _stacks.RemoveAt(i);
            }
            else
            {
                stack.count -= remaining;
                remaining = 0;
            }
        }

        if (remaining > 0) return false;

        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool HasItem(string itemId)
    {
        for (int i = 0; i < _stacks.Count; i++)
        {
            if (_stacks[i].itemId == itemId && _stacks[i].count > 0)
                return true;
        }

        return false;
    }

    public ItemStack FindStackByInstanceId(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId)) return null;
        for (int i = 0; i < _stacks.Count; i++)
        {
            if (_stacks[i].instanceId == instanceId)
                return _stacks[i];
        }

        return null;
    }

    public bool RemoveByInstanceId(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId)) return false;
        for (int i = 0; i < _stacks.Count; i++)
        {
            if (_stacks[i].instanceId == instanceId)
            {
                _stacks.RemoveAt(i);
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        return false;
    }

    public int GetCount(string itemId)
    {
        int total = 0;
        for (int i = 0; i < _stacks.Count; i++)
        {
            if (_stacks[i].itemId == itemId)
                total += _stacks[i].count;
        }

        return total;
    }

    public List<ItemStack> GetStacksByItem(string itemId)
    {
        List<ItemStack> result = new List<ItemStack>();
        for (int i = 0; i < _stacks.Count; i++)
        {
            if (_stacks[i].itemId == itemId)
                result.Add(_stacks[i]);
        }

        return result;
    }

    private static string AffixKeyOf(ItemAffix[] affixes)
    {
        if (affixes == null || affixes.Length == 0) return "";
        return new ItemStack("", 0, affixes).GetAffixKey();
    }

    private static ItemAffix[] CloneAffixes(ItemAffix[] affixes)
    {
        if (affixes == null || affixes.Length == 0) return null;
        ItemAffix[] clone = new ItemAffix[affixes.Length];
        for (int i = 0; i < affixes.Length; i++)
        {
            ItemAffix a = affixes[i];
            clone[i] = a != null ? new ItemAffix(a.stat, a.value, a.percent) : null;
        }

        return clone;
    }
}
