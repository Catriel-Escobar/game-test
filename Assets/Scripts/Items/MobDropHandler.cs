using UnityEngine;

public static class MobDropHandler
{
    public static void RollDrops(Mob mob, Player player)
    {
        if (mob == null || player == null) return;

        Enemy config = mob.EnemyConfig;
        if (config?.dropTable?.itemDrops == null || config.dropTable.itemDrops.Length == 0) return;

        AffixesConfig affixesConfig = ConfigBoostrap.Current?.AffixesConfig;

        for (int i = 0; i < config.dropTable.itemDrops.Length; i++)
        {
            ItemDrop drop = config.dropTable.itemDrops[i];
            if (drop == null || string.IsNullOrEmpty(drop.itemId)) continue;

            if (Random.value > drop.chance) continue;

            int count = drop.maxCount > drop.minCount
                ? Random.Range(drop.minCount, drop.maxCount + 1)
                : Mathf.Max(1, drop.minCount);

            Item item = player.Equipment?.FindItemById(drop.itemId);
            if (item == null)
            {
                Debug.LogWarning($"[Drops] Item '{drop.itemId}' no existe en items.json — drop ignorado.");
                continue;
            }

            ItemAffix[] affixes = item.Type == ItemType.Equipment
                ? AffixService.RollAffixes(RandomDropRarity(), affixesConfig)
                : null;

            Vector3 dropPosition = mob.transform.position +
                new Vector3(Random.Range(-0.6f, 0.6f), 0.1f, Random.Range(-0.6f, 0.6f));

            WorldDrop.Spawn(drop.itemId, count, affixes, dropPosition);
            Debug.Log($"[Drops] {config.id} dejo caer '{drop.itemId}' x{count} en el mundo" + (affixes != null ? $" con {affixes.Length} afijos" : ""));
        }
    }

    public static ItemRarity RandomDropRarity()
    {
        float roll = Random.value;
        if (roll < 0.45f) return ItemRarity.Uncommon;
        if (roll < 0.75f) return ItemRarity.Rare;
        if (roll < 0.93f) return ItemRarity.Epic;
        return ItemRarity.Legendary;
    }
}
