using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    private Player _player;
    private ItemsConfig _itemsConfig;
    private PlayerVisualEquipment _visual;
    private readonly Item[] _equipped = new Item[7];

    public event Action OnEquipmentChanged;

    public ItemStats TotalStats
    {
        get
        {
            ItemStats total = new ItemStats();
            for (int i = 0; i < _equipped.Length; i++)
            {
                if (_equipped[i]?.stats == null) continue;
                total.armor += _equipped[i].stats.armor;
                total.health += _equipped[i].stats.health;
                total.mana += _equipped[i].stats.mana;
                total.damage += _equipped[i].stats.damage;
                total.strength += _equipped[i].stats.strength;
                total.vitality += _equipped[i].stats.vitality;
                total.intelligence += _equipped[i].stats.intelligence;
                total.dexterity += _equipped[i].stats.dexterity;
            }
            return total;
        }
    }

    public void Initialize(Player player, ItemsConfig itemsConfig, string[] equippedItemIds = null)
    {
        _player = player;
        _itemsConfig = itemsConfig;
        _visual = GetComponent<PlayerVisualEquipment>();
        if (_visual == null)
            _visual = gameObject.AddComponent<PlayerVisualEquipment>();
        _visual.Initialize(this);

        if (equippedItemIds != null)
        {
            for (int i = 0; i < equippedItemIds.Length && i < _equipped.Length; i++)
            {
                if (string.IsNullOrEmpty(equippedItemIds[i])) continue;
                Item item = FindItemById(equippedItemIds[i]);
                if (item != null)
                    _equipped[i] = item;
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
        return _equipped[(int)slot];
    }

    public string[] GetEquippedIds()
    {
        string[] ids = new string[_equipped.Length];
        for (int i = 0; i < _equipped.Length; i++)
            ids[i] = _equipped[i]?.id ?? "";
        return ids;
    }

    public bool Equip(string itemId)
    {
        Item item = FindItemById(itemId);
        if (item == null)
        {
            Debug.LogWarning($"[Equipment] Item '{itemId}' no encontrado.");
            return false;
        }

        if (item.type != ItemType.Equipment)
        {
            Debug.LogWarning($"[Equipment] '{itemId}' no es equipable.");
            return false;
        }

        int slotIndex = (int)item.slot;
        if (slotIndex < 0 || slotIndex >= _equipped.Length)
        {
            Debug.LogWarning($"[Equipment] Slot invalido para '{itemId}'.");
            return false;
        }

        if (_player.Progression != null &&
            _player.Progression.Level < item.levelRequirement)
        {
            Debug.LogWarning($"[Equipment] Requiere nivel {item.levelRequirement}.");
            return false;
        }

        _equipped[slotIndex] = item;
        _visual.ApplySlot(item.slot);
        OnEquipmentChanged?.Invoke();
        return true;
    }

    public void Unequip(EquipmentSlot slot)
    {
        int slotIndex = (int)slot;
        if (slotIndex < 0 || slotIndex >= _equipped.Length) return;
        if (_equipped[slotIndex] == null) return;

        _equipped[slotIndex] = null;
        _visual.ClearSlot(slot);
        OnEquipmentChanged?.Invoke();
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

        if (item.type != ItemType.Consumable || item.effect == null)
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
}
