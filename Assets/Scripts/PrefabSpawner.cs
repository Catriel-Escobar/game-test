using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{
    private SpawnerConfig config;
    private ISpawnMode spawnMode;
    private bool initialized;

    public int AliveCount => spawnMode != null ? spawnMode.AliveCount : 0;
    public ISpawnMode SpawnMode => spawnMode;
    public bool UseWaves => config != null && config.useWaves;

    public void Initialize(SpawnerConfig spawnerConfig)
    {
        config = spawnerConfig;
        ApplyMode(config.useWaves);
        initialized = true;
    }

    public void SetMode(bool useWaves)
    {
        if (config == null) return;
        config.useWaves = useWaves;
        ApplyMode(useWaves);
    }

    private void ApplyMode(bool useWaves)
    {
        if (useWaves && config.waves != null && config.waves.Count > 0)
        {
            spawnMode = new WaveMode();
        }
        else
        {
            spawnMode = new RespawnConstantMode();
        }

        spawnMode.Initialize(config, SpawnMob);
    }

    private void Update()
    {
        if (!initialized || spawnMode == null) return;
        spawnMode.Update(Time.deltaTime);
    }

    public void OnMobDied(Mob mob)
    {
        spawnMode?.OnMobDied(mob);
    }

    private Mob SpawnMob(string enemyId, float healthMultiplier, float damageMultiplier)
    {
        if (string.IsNullOrEmpty(enemyId)) return null;

        GameObject prefab = SpawnerManager.Instance.GetPrefab(enemyId);
        if (prefab == null) return null;

        Vector2 randomPoint = Random.insideUnitCircle * config.radius;
        Vector3 spawnPosition = transform.position + new Vector3(randomPoint.x, 0f, randomPoint.y);

        GameObject obj = Instantiate(prefab, spawnPosition, Quaternion.identity);

        MobSpawnData spawnData = new()
        {
            SpawnPosition = spawnPosition,
            PatrolRadius = config.radius,
            EnemyId = enemyId,
            HealthMultiplier = healthMultiplier,
            DamageMultiplier = damageMultiplier
        };

        if (obj.TryGetComponent<Mob>(out var mob))
        {
            mob.Initialize(spawnData);
            return mob;
        }

        return null;
    }

    private void OnDrawGizmosSelected()
    {
        if (config != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, config.radius);
        }
    }
}
