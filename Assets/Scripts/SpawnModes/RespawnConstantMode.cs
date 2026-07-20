using System.Collections.Generic;

public class RespawnConstantMode : ISpawnMode
{
    private SpawnerConfig config;
    private System.Func<string, float, float, Mob> spawnCallback;
    private List<Mob> aliveMobs = new();
    private float spawnTimer;

    public int AliveCount => aliveMobs.Count;

    public void Initialize(SpawnerConfig config, System.Func<string, float, float, Mob> spawnCallback)
    {
        this.config = config;
        this.spawnCallback = spawnCallback;

        for (int i = 0; i < config.maxAlive; i++)
        {
            SpawnMob();
        }
    }

    public void Update(float deltaTime)
    {
        spawnTimer += deltaTime;

        if (spawnTimer >= config.spawnInterval && aliveMobs.Count < config.maxAlive)
        {
            SpawnMob();
            spawnTimer = 0f;
        }
    }

    public void OnMobDied(Mob mob)
    {
        aliveMobs.Remove(mob);
    }

    private void SpawnMob()
    {
        string enemyId = WeightedRandomSelector.Select(config.enemyTypes);
        if (string.IsNullOrEmpty(enemyId)) return;

        Mob mob = spawnCallback(enemyId, config.healthMultiplier, config.damageMultiplier);
        if (mob != null)
        {
            aliveMobs.Add(mob);
        }
    }
}
