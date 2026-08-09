using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    private Player _player;
    private ItemsConfig _itemsConfig;
    private PlayerVisualEquipment _visual;
    private readonly Dictionary<EquipmentSlot, EquippedItemData> _equipped = new Dictionary<EquipmentSlot, EquippedItemData>();

    public event Action OnEquipmentChanged;

    public ItemStats TotalStats
    {
        get
        {
            ItemStats total = new ItemStats();
            foreach (EquippedItemData equipped in _equipped.Values)
            {
                if (equipped == null) continue;
                Item item = FindItemById(equipped.itemId);
                if (item?.stats == null) continue;
                ItemStats stats = AffixService.ApplyAffixes(item.stats, equipped.affixes);
                total.armor += stats.armor;
                total.health += stats.health;
                total.mana += stats.mana;
                total.damage += stats.damage;
                total.strength += stats.strength;
                total.vitality += stats.vitality;
                total.intelligence += stats.intelligence;
                total.dexterity += stats.dexterity;
            }
            return total;
        }
    }

    public void Initialize(Player player, ItemsConfig itemsConfig, Dictionary<EquipmentSlot, EquippedItemData> equippedItems = null)
    {
        _player = player;
        _itemsConfig = itemsConfig;
        _visual = GetComponent<PlayerVisualEquipment>();
        if (_visual == null)
            _visual = gameObject.AddComponent<PlayerVisualEquipment>();
        _visual.Initialize(this);

        _equipped.Clear();
        if (equippedItems != null)
        {
            foreach (KeyValuePair<EquipmentSlot, EquippedItemData> pair in equippedItems)
            {
                EquippedItemData data = pair.Value;
                if (data == null || string.IsNullOrEmpty(data.itemId)) continue;
                if (FindItemById(data.itemId) != null)
                    _equipped[pair.Key] = data;
            }
        }

        _visual.RefreshAll();
        OnEquipmentChanged += HandleEquipmentChanged;
        OnEquipmentChanged?.Invoke();
    }

    private void HandleEquipmentChanged()
    {
        if (_player != null && _player.Resources != null)
            _player.Resources.RefreshMaxResources();
    }

    public Item GetItemInSlot(EquipmentSlot slot)
    {
        EquippedItemData data;
        return _equipped.TryGetValue(slot, out data) ? FindItemById(data.itemId) : null;
    }

    public Dictionary<EquipmentSlot, EquippedItemData> GetEquippedData()
    {
        Dictionary<EquipmentSlot, EquippedItemData> data = new Dictionary<EquipmentSlot, EquippedItemData>();
        foreach (KeyValuePair<EquipmentSlot, EquippedItemData> pair in _equipped)
        {
            EquippedItemData equipped = pair.Value;
            data[pair.Key] = new EquippedItemData
            {
                itemId = equipped.itemId,
                instanceId = equipped.instanceId,
                affixes = CloneAffixes(equipped.affixes)
            };
        }

        return data;
    }

    public bool EquipFromInventory(string instanceId)
    {
        if (_player?.Inventory == null) return false;

        ItemStack stack = _player.Inventory.FindStackByInstanceId(instanceId);
        if (stack == null)
        {
            Debug.LogWarning($"[Equipment] Stack con instanceId '{instanceId}' no encontrado en inventario.");
            return false;
        }

        Item item = FindItemById(stack.itemId);
        if (item == null)
        {
            Debug.LogWarning($"[Equipment] Item '{stack.itemId}' no encontrado.");
            return false;
        }

        if (item.Type != ItemType.Equipment)
        {
            Debug.LogWarning($"[Equipment] '{stack.itemId}' no es equipable.");
            return false;
        }

        if (_player.Progression != null &&
            _player.Progression.Level < item.levelRequirement)
        {
            Debug.LogWarning($"[Equipment] Requiere nivel {item.levelRequirement}.");
            return false;
        }

        _equipped[item.Slot] = new EquippedItemData
        {
            itemId = item.id,
            instanceId = stack.instanceId,
            affixes = CloneAffixes(stack.affixes)
        };
        _player.Inventory.RemoveByInstanceId(instanceId);
        _visual.ApplySlot(item.Slot);
        OnEquipmentChanged?.Invoke();
        return true;
    }

    public void Unequip(EquipmentSlot slot)
    {
        EquippedItemData equipped;
        if (!_equipped.TryGetValue(slot, out equipped)) return;

        _equipped.Remove(slot);
        _visual.ClearSlot(slot);
        OnEquipmentChanged?.Invoke();

        if (_player?.Inventory != null)
            _player.Inventory.AddItem(equipped.itemId, 1, equipped.affixes, equipped.instanceId);
    }

    public Item FindItemById(string itemId)
    {
        if (_itemsConfig?.items == null || string.IsNullOrEmpty(itemId)) return null;

        for (int i = 0; i < _itemsConfig.items.Length; i++)
        {
            if (_itemsConfig.items[i].id == itemId)
                return _itemsConfig.items[i];
        }

        return null;
    }

    public bool UseConsumable(string itemId)
    {
        Item item = FindItemById(itemId);
        if (item == null)
        {
            Debug.LogWarning($"[Equipment] Item '{itemId}' no encontrado.");
            return false;
        }

        if (item.Type != ItemType.Consumable || item.effect == null)
        {
            Debug.LogWarning($"[Equipment] '{itemId}' no es consumible.");
            return false;
        }

        if (_player?.Resources == null) return false;

        if (_player.Inventory != null && !_player.Inventory.HasItem(itemId))
        {
            Debug.LogWarning($"[Equipment] No hay '{itemId}' en el inventario.");
            return false;
        }

        if (item.effect.heal > 0)
            _player.Resources.Heal(item.effect.heal);

        if (item.effect.restoreMana > 0)
            _player.Resources.RestoreMana(item.effect.restoreMana);

        if (_player.Inventory != null)
            _player.Inventory.RemoveItem(itemId, 1);

        Debug.Log($"[Equipment] Consumido '{item.id}' (heal {item.effect.heal}, mana {item.effect.restoreMana}). HP {_player.Resources.CurrentHp}/{_player.Resources.MaxHp} | MP {_player.Resources.CurrentMana}/{_player.Resources.MaxMana}");
        return true;
    }

    public ItemAffix[] GetEquippedAffixesInSlot(EquipmentSlot slot)
    {
        EquippedItemData equipped;
        return _equipped.TryGetValue(slot, out equipped) ? equipped.affixes : null;
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
