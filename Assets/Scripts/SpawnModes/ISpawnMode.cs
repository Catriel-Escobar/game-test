using System.Collections.Generic;

public interface ISpawnMode
{
    int AliveCount { get; }
    void Initialize(SpawnerConfig config, System.Func<string, float, float, Mob> spawnCallback);
    void Update(float deltaTime);
    void OnMobDied(Mob mob);
}

public static class WeightedRandomSelector
{
    public static string Select(List<EnemyTypeWeight> types)
    {
        if (types == null || types.Count == 0)
            return null;

        int totalWeight = 0;
        for (int i = 0; i < types.Count; i++)
        {
            totalWeight += types[i].weight;
        }

        if (totalWeight <= 0) return types[0].enemyId;

        int random = UnityEngine.Random.Range(0, totalWeight);
        int cumulative = 0;

        for (int i = 0; i < types.Count; i++)
        {
            cumulative += types[i].weight;
            if (random < cumulative)
                return types[i].enemyId;
        }

        return types[types.Count - 1].enemyId;
    }
}
