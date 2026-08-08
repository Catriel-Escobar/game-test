using UnityEngine;
using UnityEngine.InputSystem;

public class EquipmentDebugTester : MonoBehaviour
{
    private Player _player;
    private LocalizationConfig _loc;

    private void Start()
    {
        _player = FindObjectOfType<Player>();
        _loc = ConfigBoostrap.Current?.LocalizationConfig;
        Log("=== EQUIPMENT DEBUG TESTER ===");
        Log("3 Casco | 4 Pecho | 5 Guantes | 6 Botas | 7 Espada | 8 Capa | 9 Escudo");
        Log("0 Posion vida | - Posion mana | = Stats | Enter Dump visual");
        Log("[ Dar 1 espada T1 | ] Dar 1 posion vida | \\ Dar 1 posion mana | ; Ver inventario");
    }

    private void Update()
    {
        if (_player == null || _player.Equipment == null) return;
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.digit3Key.wasPressedThisFrame) CycleSlot(EquipmentSlot.Helmet, "warrior_helmet_t1", "warrior_helmet_t2");
        if (kb.digit4Key.wasPressedThisFrame) CycleSlot(EquipmentSlot.Chest, "warrior_chest_t1", "warrior_chest_t2");
        if (kb.digit5Key.wasPressedThisFrame) CycleSlot(EquipmentSlot.Gloves, "warrior_gloves_t1", "warrior_gloves_t2");
        if (kb.digit6Key.wasPressedThisFrame) CycleSlot(EquipmentSlot.Boots, "warrior_boots_t1", "warrior_boots_t2");
        if (kb.digit7Key.wasPressedThisFrame) CycleSlot(EquipmentSlot.Weapon, "warrior_sword_t1", "warrior_sword_t2");

        if (kb.digit8Key.wasPressedThisFrame) CycleSlot(EquipmentSlot.Cape, "warrior_cape_t2");
        if (kb.digit9Key.wasPressedThisFrame) CycleSlot(EquipmentSlot.OffHand, "warrior_shield_t2");

        if (kb.digit0Key.wasPressedThisFrame) _player.Equipment.UseConsumable("health_potion");
        if (kb.minusKey.wasPressedThisFrame) _player.Equipment.UseConsumable("mana_potion");

        if (kb.leftBracketKey.wasPressedThisFrame) GiveItem("warrior_sword_t1");
        if (kb.rightBracketKey.wasPressedThisFrame) GiveItem("health_potion");
        if (kb.backslashKey.wasPressedThisFrame) GiveItem("mana_potion");

        if (kb.semicolonKey.wasPressedThisFrame) PrintInventory();
        if (kb.equalsKey.wasPressedThisFrame) PrintStats();
        if (kb.enterKey.wasPressedThisFrame) DumpVisualHierarchy();
    }

    private void GiveItem(string itemId)
    {
        if (_player.Inventory == null)
        {
            Log("Inventory null");
            return;
        }

        _player.Inventory.AddItem(itemId, 1);
        Log($"+1 {ItemName(itemId)} al inventario (total: {_player.Inventory.GetCount(itemId)})");
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
            Log($"  [{i}] {ItemName(stack.itemId)} x{stack.count}");
        }
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
        for (int i = 0; i < 7; i++)
        {
            Item item = _player.Equipment.GetItemInSlot((EquipmentSlot)i);
            if (item != null)
                Log($"  [{((EquipmentSlot)i)}] {ItemName(item.id)}");
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

    private void CycleSlot(EquipmentSlot slot, params string[] variants)
    {
        if (variants == null || variants.Length == 0) return;

        Item current = _player.Equipment.GetItemInSlot(slot);
        int nextIndex = 0;
        if (current != null)
        {
            int currentIndex = -1;
            for (int i = 0; i < variants.Length; i++)
            {
                if (variants[i] == current.id)
                {
                    currentIndex = i;
                    break;
                }
            }

            if (currentIndex == variants.Length - 1)
            {
                _player.Equipment.Unequip(slot);
                Log($"Desequipado {ItemName(current.id)} de {slot}");
                return;
            }

            nextIndex = currentIndex + 1;
        }

        if (nextIndex < variants.Length)
        {
            _player.Equipment.Equip(variants[nextIndex]);
            Item equipped = _player.Equipment.GetItemInSlot(slot);
            Log($"Equipado {ItemName(equipped.id)} [{slot}]");
        }
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
