using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EquipmentDebugTester : MonoBehaviour
{
    private Player _player;
    private LocalizationConfig _loc;
    private readonly Dictionary<int, string> _lastEquippedInstanceBySlot = new Dictionary<int, string>();

    private void Start()
    {
        _player = FindObjectOfType<Player>();
        _loc = ConfigBoostrap.Current?.LocalizationConfig;
        Log("=== EQUIPMENT DEBUG TESTER ===");
        Log("3-9 Equipan el siguiente stack de ese slot DESDE EL INVENTARIO (3 Casco | 4 Pecho | 5 Guantes | 6 Botas | 7 Espada | 8 Capa | 9 Escudo)");
        Log("0 Posion vida | - Posion mana | = Stats | Enter Dump visual");
        Log("[ Dar 1 espada T1 | ] Dar 1 posion vida | \\ Dar 1 posion mana | ; Ver inventario");
        Log("F1 Dar 1 casco T2 con afijos | F2 Dar 1 espada T2 con afijos | F3 Dar espada T1 con afijos aleatorios");
        Log("F4 Generar bolsa de prueba en el mundo (health_potion x2) | Espacio recoge dentro del radio | Click recoge/acerca");
    }

    private void Update()
    {
        if (_player == null || _player.Equipment == null) return;
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.digit3Key.wasPressedThisFrame) CycleSlot(EquipmentSlot.Helmet);
        if (kb.digit4Key.wasPressedThisFrame) CycleSlot(EquipmentSlot.Chest);
        if (kb.digit5Key.wasPressedThisFrame) CycleSlot(EquipmentSlot.Gloves);
        if (kb.digit6Key.wasPressedThisFrame) CycleSlot(EquipmentSlot.Boots);
        if (kb.digit7Key.wasPressedThisFrame) CycleSlot(EquipmentSlot.Weapon);

        if (kb.digit8Key.wasPressedThisFrame) CycleSlot(EquipmentSlot.Cape);
        if (kb.digit9Key.wasPressedThisFrame) CycleSlot(EquipmentSlot.OffHand);

        if (kb.digit0Key.wasPressedThisFrame) _player.Equipment.UseConsumable("health_potion");
        if (kb.minusKey.wasPressedThisFrame) _player.Equipment.UseConsumable("mana_potion");

        if (kb.leftBracketKey.wasPressedThisFrame) GiveItem("warrior_sword_t1");
        if (kb.rightBracketKey.wasPressedThisFrame) GiveItem("health_potion");
        if (kb.backslashKey.wasPressedThisFrame) GiveItem("mana_potion");

        if (kb.semicolonKey.wasPressedThisFrame) PrintInventory();
        if (kb.equalsKey.wasPressedThisFrame) PrintStats();
        if (kb.enterKey.wasPressedThisFrame) DumpVisualHierarchy();

        if (kb.f1Key.wasPressedThisFrame) GiveItem("warrior_helmet_t2", new ItemAffix[] { new ItemAffix("vitality", 5), new ItemAffix("armor", 8) });
        if (kb.f2Key.wasPressedThisFrame) GiveItem("warrior_sword_t2", new ItemAffix[] { new ItemAffix("damage", 10, 0.15f) });
        if (kb.f3Key.wasPressedThisFrame)
        {
            ItemAffix[] affixes = AffixService.RollAffixes(ItemRarity.Rare, ConfigBoostrap.Current?.AffixesConfig);
            GiveItem("warrior_sword_t1", affixes);
        }

        if (kb.f4Key.wasPressedThisFrame) SpawnTestDrop();
    }

    private void SpawnTestDrop()
    {
        if (_player == null) return;
        Vector3 position = _player.transform.position + _player.transform.forward * 2f + Vector3.up * 0.1f;
        WorldDrop.Spawn("health_potion", 2, null, position);
        Log("Bolsa de prueba generada (health_potion x2)");
    }

    private void GiveItem(string itemId, ItemAffix[] affixes = null)
    {
        if (_player.Inventory == null)
        {
            Log("Inventory null");
            return;
        }

        _player.Inventory.AddItem(itemId, 1, affixes);
        string affixText = affixes != null && affixes.Length > 0 ? $" {DescribeAffixes(affixes)}" : "";
        Log($"+1 {ItemName(itemId)}{affixText} al inventario");
    }

    private void PrintInventory()
    {
        if (_player.Inventory == null)
        {
            Log("Inventory null");
            return;
        }

        Log($"=== INVENTARIO ({_player.Inventory.Count} stacks) ===");
        if (_player.Inventory.Stacks.Count == 0)
        {
            Log("  (vacio)");
            return;
        }

        for (int i = 0; i < _player.Inventory.Stacks.Count; i++)
        {
            var stack = _player.Inventory.Stacks[i];
            string rarityText = RarityLabel(stack.GetRarity());
            string affixText = stack.HasAffixes() ? $" {DescribeAffixes(stack.affixes)}" : "";
            Log($"  [{i}] {rarityText} {ItemName(stack.itemId)} x{stack.count}{affixText}");
        }
    }

    private string RarityLabel(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return "(Comun)";
            case ItemRarity.Uncommon: return "(Poco comun)";
            case ItemRarity.Rare: return "(Raro)";
            case ItemRarity.Epic: return "(Epico)";
            case ItemRarity.Legendary: return "(Legendario)";
            default: return "";
        }
    }

    private string DescribeAffixes(ItemAffix[] affixes)
    {
        if (affixes == null || affixes.Length == 0) return "";
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("[");
        for (int i = 0; i < affixes.Length; i++)
        {
            if (i > 0) sb.Append(", ");
            ItemAffix a = affixes[i];
            if (a.percent > 0f)
                sb.Append($"{(a.percent * 100f):0.#}% {a.stat}");
            else
                sb.Append($"+{a.value} {a.stat}");
        }
        sb.Append("]");
        return sb.ToString();
    }

    private void PrintStats()
    {
        Log("=== STATS ===");
        Log($"Nivel: {_player.Progression.Level} | XP: {_player.Progression.CurrentExperience}");
        Log($"HP: {_player.Resources.CurrentHp}/{_player.Resources.MaxHp} | MP: {_player.Resources.CurrentMana}/{_player.Resources.MaxMana}");

        ItemStats eq = _player.Equipment.TotalStats;
        Log($"Stats base -> FUE {_player.Stats.Strength} | VIT {_player.Stats.Vitality} | INT {_player.Stats.Intelligence} | DES {_player.Stats.Dexterity}");
        Log($"Stats equipo -> armor {eq.armor} | damage {eq.damage} | health {eq.health} | mana {eq.mana} | FUE {eq.strength} | VIT {eq.vitality} | INT {eq.intelligence} | DES {eq.dexterity}");

        CombatStats cs = _player.CombatStats;
        Log($"Combat -> ATK fisico {cs.PhysicalAttack} | ATK magico {cs.MagicAttack} | DEF fisica {cs.PhysicalDefense} | DEF magica {cs.MagicDefense} | CritChance {cs.CriticalChance:F2} | CritDmg {cs.CriticalDamage:F2}");

        Log("Items equipados:");
        EquipmentSlot[] slots = (EquipmentSlot[])System.Enum.GetValues(typeof(EquipmentSlot));
        for (int i = 0; i < slots.Length; i++)
        {
            Item item = _player.Equipment.GetItemInSlot(slots[i]);
            if (item == null) continue;
            ItemAffix[] affixes = _player.Equipment.GetEquippedAffixesInSlot(slots[i]);
            string rarityText = RarityLabel(AffixService.RarityForAffixCount(affixes != null ? affixes.Length : 0));
            string affixText = affixes != null && affixes.Length > 0 ? $" {DescribeAffixes(affixes)}" : "";
            Log($"  [{slots[i]}] {rarityText} {ItemName(item.id)}{affixText}");
        }
    }

    private void DumpVisualHierarchy()
    {
        Log("=== JERARQUIA VISUAL (nombre | activo) ===");
        Transform t = _player.transform;
        for (int i = 0; i < t.childCount; i++)
        {
            Transform child = t.GetChild(i);
            Log($"  [{i}] {child.name} | activo={child.gameObject.activeSelf} (raiz={child.gameObject.activeInHierarchy})");
            for (int j = 0; j < child.childCount; j++)
            {
                Transform grand = child.GetChild(j);
                if (grand.name.Contains("T1") || grand.name.Contains("T2") ||
                    grand.name.Contains("Casco") || grand.name.Contains("Remera") ||
                    grand.name.Contains("Armadura") || grand.name.Contains("Guante") ||
                    grand.name.Contains("Bota") || grand.name.Contains("Espada") ||
                    grand.name.Contains("Capa") || grand.name.Contains("Escudo"))
                {
                    Log($"    |- {grand.name} | activo={grand.gameObject.activeSelf} (raiz={grand.gameObject.activeInHierarchy})");
                }
            }
        }
    }

    private void CycleSlot(EquipmentSlot slot)
    {
        if (_player.Inventory == null)
        {
            Log("Inventory null");
            return;
        }

        Item current = _player.Equipment.GetItemInSlot(slot);
        if (current != null)
        {
            _player.Equipment.Unequip(slot);
            Log($"Desequipado {ItemName(current.id)} de {slot} -> vuelto al inventario");
            return;
        }

        List<ItemStack> candidates = new List<ItemStack>();
        for (int i = 0; i < _player.Inventory.Stacks.Count; i++)
        {
            ItemStack stack = _player.Inventory.Stacks[i];
            if (stack == null || stack.count <= 0) continue;
            Item item = _player.Equipment.FindItemById(stack.itemId);
            if (item != null && item.Type == ItemType.Equipment && item.Slot == slot)
                candidates.Add(stack);
        }

        if (candidates.Count == 0)
        {
            Log($"No hay items equipables para {slot} en el inventario (usa [ ] \\ o F1-F3 para obtener items)");
            return;
        }

        int startIndex = 0;
        if (_lastEquippedInstanceBySlot.TryGetValue((int)slot, out string lastInstance))
        {
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].instanceId == lastInstance)
                {
                    startIndex = (i + 1) % candidates.Count;
                    break;
                }
            }
        }

        ItemStack target = candidates[startIndex];
        if (!_player.Equipment.EquipFromInventory(target.instanceId))
        {
            Log($"No se pudo equipar {ItemName(target.itemId)} desde inventario");
            return;
        }

        _lastEquippedInstanceBySlot[(int)slot] = target.instanceId;
        string affixText = target.HasAffixes() ? $" {DescribeAffixes(target.affixes)}" : "";
        Log($"Equipado {RarityLabel(target.GetRarity())} {ItemName(target.itemId)} [{slot}] instancia={target.instanceId}{affixText}");
    }

    private string ItemName(string id)
    {
        Item item = _player.Equipment.FindItemById(id);
        if (item == null || string.IsNullOrEmpty(item.displayNameKey)) return id;
        return _loc != null ? _loc.Get(item.displayNameKey) : item.displayNameKey;
    }

    private void Log(string message)
    {
        Debug.Log($"[DebugTest] {message}");
    }
}
