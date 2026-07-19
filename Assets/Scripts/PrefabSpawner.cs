using System.Collections.Generic;
using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{
    private SpawnerConfig config;
    private List<Mob> aliveMobs = new();
    private float spawnTimer;
    private bool initialized;

    public int AliveCount => aliveMobs.Count;

    public void Initialize(SpawnerConfig spawnerConfig)
    {
        config = spawnerConfig;
        initialized = true;

        for (int i = 0; i < config.maxAlive; i++)
        {
            SpawnMob();
        }
    }

    private void Update()
    {
        if (!initialized || config == null) return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= config.spawnInterval && aliveMobs.Count < config.maxAlive)
        {
            SpawnMob();
            spawnTimer = 0f;
        }
    }

    private void SpawnMob()
    {
        string enemyId = SelectEnemyByWeight();
        if (string.IsNullOrEmpty(enemyId)) return;

        GameObject prefab = SpawnerManager.Instance.GetPrefab(enemyId);
        if (prefab == null) return;

        Vector2 randomPoint = Random.insideUnitCircle * config.radius;
        Vector3 spawnPosition = transform.position + new Vector3(randomPoint.x, 0f, randomPoint.y);

        GameObject obj = Instantiate(prefab, spawnPosition, Quaternion.identity);

        MobSpawnData spawnData = new()
        {
            SpawnPosition = spawnPosition,
            PatrolRadius = config.radius,
            EnemyId = enemyId,
            HealthMultiplier = config.healthMultiplier,
            DamageMultiplier = config.damageMultiplier
        };

        if (obj.TryGetComponent<Mob>(out var mob))
        {
            mob.Initialize(spawnData);
            aliveMobs.Add(mob);
        }
    }

    private string SelectEnemyByWeight()
    {
        if (config?.enemyTypes == null || config.enemyTypes.Count == 0)
            return null;

        int totalWeight = 0;
        for (int i = 0; i < config.enemyTypes.Count; i++)
        {
            totalWeight += config.enemyTypes[i].weight;
        }

        if (totalWeight <= 0) return config.enemyTypes[0].enemyId;

        int random = Random.Range(0, totalWeight);
        int cumulative = 0;

        for (int i = 0; i < config.enemyTypes.Count; i++)
        {
            cumulative += config.enemyTypes[i].weight;
            if (random < cumulative)
                return config.enemyTypes[i].enemyId;
        }

        return config.enemyTypes[config.enemyTypes.Count - 1].enemyId;
    }

    public void OnMobDied(Mob mob)
    {
        aliveMobs.Remove(mob);
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
