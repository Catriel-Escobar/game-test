using System.Collections.Generic;
using UnityEngine;

public static class AffixService
{
    public static int AffixCountForRarity(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return 0;
            case ItemRarity.Uncommon: return 1;
            case ItemRarity.Rare: return 2;
            case ItemRarity.Epic: return 3;
            case ItemRarity.Legendary: return 4;
            default: return 0;
        }
    }

    public static ItemRarity RarityForAffixCount(int affixCount)
    {
        if (affixCount >= 4) return ItemRarity.Legendary;
        if (affixCount == 3) return ItemRarity.Epic;
        if (affixCount == 2) return ItemRarity.Rare;
        if (affixCount == 1) return ItemRarity.Uncommon;
        return ItemRarity.Common;
    }

    public static ItemAffix[] RollAffixes(ItemRarity rarity, AffixesConfig config)
    {
        int count = AffixCountForRarity(rarity);
        if (count <= 0 || config?.affixes == null || config.affixes.Length == 0)
            return null;

        List<ItemAffixConfig> pool = new List<ItemAffixConfig>(config.affixes);
        List<ItemAffix> result = new List<ItemAffix>();

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            ItemAffixConfig def = pool[Random.Range(0, pool.Count)];
            int value = def.maxValue > def.minValue
                ? Random.Range(def.minValue, def.maxValue + 1)
                : def.minValue;

            result.Add(new ItemAffix(def.stat, def.isPercent ? 0 : value, def.isPercent ? value / 100f : 0f));
            pool.RemoveAll(a => a.stat == def.stat);
        }

        return result.Count > 0 ? result.ToArray() : null;
    }

    public static ItemStats ApplyAffixes(ItemStats baseStats, ItemAffix[] affixes)
    {
        if (baseStats == null) return null;
        if (affixes == null || affixes.Length == 0) return CloneStats(baseStats);

        ItemStats result = CloneStats(baseStats);
        for (int i = 0; i < affixes.Length; i++)
        {
            ItemAffix affix = affixes[i];
            if (affix == null || string.IsNullOrEmpty(affix.stat)) continue;

            if (affix.percent > 0f)
            {
                ApplyPercent(result, affix.stat, affix.percent);
            }
            else if (affix.value != 0)
            {
                ApplyFlat(result, affix.stat, affix.value);
            }
        }

        return result;
    }

    public static ItemStats CloneStats(ItemStats stats)
    {
        return new ItemStats
        {
            armor = stats.armor,
            health = stats.health,
            mana = stats.mana,
            damage = stats.damage,
            strength = stats.strength,
            vitality = stats.vitality,
            intelligence = stats.intelligence,
            dexterity = stats.dexterity
        };
    }

    private static void ApplyFlat(ItemStats stats, string stat, int value)
    {
        switch (stat)
        {
            case "armor": stats.armor += value; break;
            case "health": stats.health += value; break;
            case "mana": stats.mana += value; break;
            case "damage": stats.damage += value; break;
            case "strength": stats.strength += value; break;
            case "vitality": stats.vitality += value; break;
            case "intelligence": stats.intelligence += value; break;
            case "dexterity": stats.dexterity += value; break;
        }
    }

    private static void ApplyPercent(ItemStats stats, string stat, float percent)
    {
        switch (stat)
        {
            case "armor": stats.armor = RoundToInt(stats.armor * (1f + percent)); break;
            case "health": stats.health = RoundToInt(stats.health * (1f + percent)); break;
            case "mana": stats.mana = RoundToInt(stats.mana * (1f + percent)); break;
            case "damage": stats.damage = RoundToInt(stats.damage * (1f + percent)); break;
            case "strength": stats.strength = RoundToInt(stats.strength * (1f + percent)); break;
            case "vitality": stats.vitality = RoundToInt(stats.vitality * (1f + percent)); break;
            case "intelligence": stats.intelligence = RoundToInt(stats.intelligence * (1f + percent)); break;
            case "dexterity": stats.dexterity = RoundToInt(stats.dexterity * (1f + percent)); break;
        }
    }

    private static int RoundToInt(float value)
    {
        return Mathf.RoundToInt(value);
    }
}
