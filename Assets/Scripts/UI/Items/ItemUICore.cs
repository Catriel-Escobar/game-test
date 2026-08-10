using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class ItemUICore
{
    public static Color RarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common:    return new Color(0.85f, 0.85f, 0.85f);
            case ItemRarity.Uncommon:  return new Color(0.30f, 0.90f, 0.30f);
            case ItemRarity.Rare:      return new Color(0.25f, 0.55f, 1.00f);
            case ItemRarity.Epic:      return new Color(0.70f, 0.35f, 1.00f);
            case ItemRarity.Legendary: return new Color(1.00f, 0.60f, 0.15f);
            default:                   return Color.white;
        }
    }

    public static string Name(Item item, LocalizationConfig localization)
    {
        if (item == null) return "";
        if (!string.IsNullOrEmpty(item.displayNameKey) && localization != null)
            return localization.Get(item.displayNameKey);
        return item.id;
    }

    public static string SlotName(EquipmentSlot slot, LocalizationConfig localization)
    {
        if (localization == null) return slot.ToString();
        string localized = localization.Get($"slot.{slot.ToString().ToLowerInvariant()}");
        return string.IsNullOrEmpty(localized) || localized == $"slot.{slot.ToString().ToLowerInvariant()}"
            ? slot.ToString()
            : localized;
    }

    public static Sprite Icon(Item item)
    {
        if (item == null || string.IsNullOrEmpty(item.icon)) return null;
        return Resources.Load<Sprite>($"Items/Icons/{item.icon}");
    }

    public static void SetIcon(Image image, Item item, ItemRarity rarity = default)
    {
        if (image == null) return;
        Sprite sprite = Icon(item);
        if (sprite != null)
        {
            image.sprite = sprite;
            image.color = Color.white;
        }
        else
        {
            image.sprite = null;
            image.color = RarityColor(rarity);
        }
    }

    public static ItemRarity EffectiveRarity(Item item, ItemAffix[] affixes)
    {
        if (affixes != null && affixes.Length > 0)
            return AffixService.RarityForAffixCount(affixes.Length);
        return item != null ? item.Rarity : default;
    }

    public static string BuildTooltip(Item item, ItemAffix[] affixes, LocalizationConfig localization)
    {
        if (item == null) return "";

        StringBuilder sb = new StringBuilder();

        string name = Name(item, localization);
        ItemRarity rarity = EffectiveRarity(item, affixes);
        string rarityKey = $"item.rarity.{rarity.ToString().ToLowerInvariant()}";
        string rarityText = localization != null ? localization.Get(rarityKey) : rarity.ToString();
        sb.Append($"<color=#{ColorUtility.ToHtmlStringRGB(RarityColor(rarity))}>{name}</color>");
        sb.Append($"\n<size=70%>{rarityText}{(item.Type == ItemType.Equipment ? $" · {SlotName(item.Slot, localization)}" : "")}</size>");

        if (item.levelRequirement > 1)
            sb.Append($"\n<size=80%>Requires Lv. {item.levelRequirement}</size>");

        string stats = FormatStats(item.stats);
        if (!string.IsNullOrEmpty(stats))
            sb.Append($"\n{stats}");

        string affixText = FormatAffixes(affixes);
        if (!string.IsNullOrEmpty(affixText))
            sb.Append($"\n{affixText}");

        if (item.effect != null && (item.effect.heal > 0 || item.effect.restoreMana > 0))
        {
            if (item.effect.heal > 0)
                sb.Append($"\n<color=#{ColorUtility.ToHtmlStringRGB(new Color(0.4f, 1f, 0.4f))}>+{item.effect.heal} HP</color>");
            if (item.effect.restoreMana > 0)
                sb.Append($"\n<color=#55B4FF>+{item.effect.restoreMana} MP</color>");
        }

        return sb.ToString();
    }

    public static string FormatStats(ItemStats stats)
    {
        if (stats == null) return "";
        StringBuilder sb = new StringBuilder();
        AppendStat(sb, "Armor", stats.armor);
        AppendStat(sb, "Health", stats.health);
        AppendStat(sb, "Mana", stats.mana);
        AppendStat(sb, "Damage", stats.damage);
        AppendStat(sb, "Strength", stats.strength);
        AppendStat(sb, "Vitality", stats.vitality);
        AppendStat(sb, "Intelligence", stats.intelligence);
        AppendStat(sb, "Dexterity", stats.dexterity);
        return sb.ToString();
    }

    private static void AppendStat(StringBuilder sb, string label, int value)
    {
        if (value == 0) return;
        string color = value > 0 ? "#4AD14A" : "#FF6B6B";
        string sign = value > 0 ? "+" : "";
        sb.Append($"\n<color={color}>{sign}{value} {label}</color>");
    }

    public static string FormatAffixes(ItemAffix[] affixes)
    {
        if (affixes == null || affixes.Length == 0) return "";
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < affixes.Length; i++)
        {
            ItemAffix a = affixes[i];
            if (a == null) continue;
            if (a.percent > 0f)
                sb.Append($"\n<color=#55B4FF>+{(a.percent * 100f):0.#}% {a.stat}</color>");
            else
                sb.Append($"\n<color=#55B4FF>+{a.value} {a.stat}</color>");
        }
        return sb.ToString();
    }

    public static string FormatTotalStats(ItemStats stats)
    {
        if (stats == null) return "";
        StringBuilder sb = new StringBuilder();
        if (stats.armor > 0) sb.Append($"Armor {stats.armor}\n");
        if (stats.health > 0) sb.Append($"Health {stats.health}\n");
        if (stats.mana > 0) sb.Append($"Mana {stats.mana}\n");
        if (stats.damage > 0) sb.Append($"Damage {stats.damage}\n");
        if (stats.strength > 0) sb.Append($"Strength {stats.strength}\n");
        if (stats.vitality > 0) sb.Append($"Vitality {stats.vitality}\n");
        if (stats.intelligence > 0) sb.Append($"Intelligence {stats.intelligence}\n");
        if (stats.dexterity > 0) sb.Append($"Dexterity {stats.dexterity}\n");
        return sb.ToString();
    }
}
