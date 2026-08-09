using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[Serializable]
public class ItemsConfig
{
    public Item[] items;
}

[Serializable]
public class Item
{
    public string id;
    public string displayNameKey;
    public string icon;
    public string type;        // "Equipment" / "Consumable" (JsonUtility no parsea enums desde strings)
    public string slot;        // "Helmet" / "Chest" / "Weapon" ...
    public string rarity;      // "Common" / "Uncommon" / "Rare" ...
    public int levelRequirement;
    public int maxStackSize;   // 0/1 = no stackea (cada item es unico); >1 = stack maximo (ej. pociones)
    public string visualKey;
    public ItemStats stats;
    public ItemEffect effect;

    public ItemType Type
    {
        get
        {
            if (string.IsNullOrEmpty(type)) return default;
            return Enum.TryParse<ItemType>(type, true, out ItemType result) ? result : default;
        }
    }

    public EquipmentSlot Slot
    {
        get
        {
            if (string.IsNullOrEmpty(slot)) return default;
            return Enum.TryParse<EquipmentSlot>(slot, true, out EquipmentSlot result) ? result : default;
        }
    }

    public ItemRarity Rarity
    {
        get
        {
            if (string.IsNullOrEmpty(rarity)) return default;
            return Enum.TryParse<ItemRarity>(rarity, true, out ItemRarity result) ? result : default;
        }
    }
}

[Serializable]
public class ItemStats
{
    public int armor;
    public int health;
    public int mana;
    public int damage;
    public int strength;
    public int vitality;
    public int intelligence;
    public int dexterity;
}

[Serializable]
public class ItemEffect
{
    public int heal;
    public int restoreMana;
}

[Serializable]
public class ItemStack
{
    public string instanceId;  // unico por item (cada item es distinto por sus afijos)
    public string itemId;
    public int count;
    public ItemAffix[] affixes;

    public ItemStack() { }

    public ItemStack(string itemId, int count, ItemAffix[] affixes = null, string instanceId = null)
    {
        this.instanceId = string.IsNullOrEmpty(instanceId) ? Guid.NewGuid().ToString("N") : instanceId;
        this.itemId = itemId;
        this.count = count;
        this.affixes = affixes;
    }

    public bool HasAffixes()
    {
        return affixes != null && affixes.Length > 0;
    }

    public ItemRarity GetRarity()
    {
        return AffixService.RarityForAffixCount(affixes != null ? affixes.Length : 0);
    }

    public string GetAffixKey()
    {
        if (!HasAffixes()) return "";
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < affixes.Length; i++)
        {
            if (affixes[i] == null) continue;
            sb.Append(affixes[i].stat);
            sb.Append(':');
            sb.Append(affixes[i].value);
            sb.Append(':');
            sb.Append(affixes[i].percent.ToString("0.####"));
            sb.Append('|');
        }
        return sb.ToString();
    }
}

[Serializable]
public class ItemAffix
{
    public string stat;    // stat que modifica: strength, vitality, intelligence, dexterity, armor, health, mana, damage, critChance, critDamage
    public int value;      // bonus flat (ej: +5 strength)
    public float percent;  // bonus porcentual (ej: 0.1 = +10%); 0 si no aplica

    public ItemAffix() { }

    public ItemAffix(string stat, int value, float percent = 0f)
    {
        this.stat = stat;
        this.value = value;
        this.percent = percent;
    }
}

[Serializable]
public class AffixesConfig
{
    public ItemAffixConfig[] affixes;
}

[Serializable]
public class ItemAffixConfig
{
    public string id;
    public string displayNameKey;
    public string stat;
    public bool isPercent;
    public int minValue;
    public int maxValue;
}

[Serializable]
public class EquippedItemData
{
    public string itemId;
    public string instanceId;
    public ItemAffix[] affixes;
}

[JsonConverter(typeof(StringEnumConverter))]
public enum ItemType
{
    Equipment,
    Consumable
}

[JsonConverter(typeof(StringEnumConverter))]
public enum EquipmentSlot
{
    Helmet,
    Chest,
    Gloves,
    Boots,
    Cape,
    Weapon,
    OffHand
}

[JsonConverter(typeof(StringEnumConverter))]
public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}
