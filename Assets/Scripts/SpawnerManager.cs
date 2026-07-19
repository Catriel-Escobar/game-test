using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    public static SpawnerManager Instance { get; private set; }

    private SpawnersConfig config;
    private EnemiesConfig enemiesConfig;
    private List<PrefabSpawner> activeSpawners = new();
    private Dictionary<string, GameObject> prefabCache = new();
    private Dictionary<string, Enemy> enemyConfigCache = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (ConfigBoostrap.Current == null)
        {
            Debug.LogError("SpawnerManager: ConfigBoostrap no inicializado");
            return;
        }

        config = ConfigBoostrap.Current.SpawnersConfig;
        enemiesConfig = ConfigBoostrap.Current.EnemiesConfig;

        BuildEnemyConfigCache();
        SpawnAllSpawners();
    }

    private void BuildEnemyConfigCache()
    {
        if (enemiesConfig?.enemies == null) return;

        for (int i = 0; i < enemiesConfig.enemies.Length; i++)
        {
            enemyConfigCache[enemiesConfig.enemies[i].id] = enemiesConfig.enemies[i];
        }
    }

    private void SpawnAllSpawners()
    {
        if (config?.spawners == null) return;

        for (int i = 0; i < config.spawners.Count; i++)
        {
            SpawnerConfig spawnerConfig = config.spawners[i];

            GameObject spawnerObj = new GameObject($"Spawner_{spawnerConfig.id}");
            spawnerObj.transform.SetParent(transform);

            Vector3 position = Vector3.zero;
            if (spawnerConfig.position != null && spawnerConfig.position.Length >= 3)
            {
                position = new Vector3(
                    spawnerConfig.position[0],
                    spawnerConfig.position[1],
                    spawnerConfig.position[2]);
            }
            spawnerObj.transform.position = position;

            PrefabSpawner spawner = spawnerObj.AddComponent<PrefabSpawner>();
            spawner.Initialize(spawnerConfig);
            activeSpawners.Add(spawner);
        }
    }

    public SpawnerConfig GetConfig(string spawnerId)
    {
        if (config?.spawners == null) return null;

        for (int i = 0; i < config.spawners.Count; i++)
        {
            if (config.spawners[i].id == spawnerId)
                return config.spawners[i];
        }
        return null;
    }

    public GameObject GetPrefab(string enemyId)
    {
        if (prefabCache.TryGetValue(enemyId, out GameObject cached))
            return cached;

        if (!enemyConfigCache.TryGetValue(enemyId, out Enemy enemyConfig))
        {
            Debug.LogWarning($"SpawnerManager: No se encontró config para enemyId={enemyId}");
            return null;
        }

        if (string.IsNullOrEmpty(enemyConfig.prefabPath))
        {
            Debug.LogWarning($"SpawnerManager: prefabPath vacío para enemyId={enemyId}");
            return null;
        }

        GameObject prefab = Resources.Load<GameObject>(enemyConfig.prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"SpawnerManager: No se encontró prefab en {enemyConfig.prefabPath} para enemyId={enemyId}");
            return null;
        }

        prefabCache[enemyId] = prefab;
        return prefab;
    }

    public Enemy GetEnemyConfig(string enemyId)
    {
        enemyConfigCache.TryGetValue(enemyId, out Enemy enemyConfig);
        return enemyConfig;
    }

    public void OnMobDied(Mob mob)
    {
        for (int i = 0; i < activeSpawners.Count; i++)
        {
            activeSpawners[i].OnMobDied(mob);
        }
    }

    public int GetTotalAliveMobs()
    {
        int total = 0;
        for (int i = 0; i < activeSpawners.Count; i++)
        {
            total += activeSpawners[i].AliveCount;
        }
        return total;
    }
}
