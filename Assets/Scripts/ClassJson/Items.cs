using System;

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
    public ItemType type;
    public EquipmentSlot slot;
    public ItemRarity rarity;
    public int levelRequirement;
    public string visualKey;
    public ItemStats stats;
    public ItemEffect effect;
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
    public string itemId;
    public int count;

    public ItemStack() { }

    public ItemStack(string itemId, int count)
    {
        this.itemId = itemId;
        this.count = count;
    }
}

public enum ItemType
{
    Equipment,
    Consumable
}

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

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}
