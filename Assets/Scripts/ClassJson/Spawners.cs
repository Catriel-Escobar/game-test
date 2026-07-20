using System;
using System.Collections.Generic;

[Serializable]
public class SpawnersConfig
{
    public List<SpawnerConfig> spawners;
}

[Serializable]
public class SpawnerConfig
{
    public string id;
    public float[] position;
    public float radius;
    public int maxAlive;
    public float spawnInterval;
    public float respawnDelay;
    public float healthMultiplier;
    public float damageMultiplier;
    public bool useWaves;
    public List<EnemyTypeWeight> enemyTypes;
    public List<WaveConfig> waves;
}

[Serializable]
public class EnemyTypeWeight
{
    public string enemyId;
    public int weight;
}

[Serializable]
public class WaveConfig
{
    public int waveNumber;
    public List<EnemyTypeWeight> enemyTypes;
    public int totalSpawns;
    public float spawnInterval;
    public float healthMultiplier;
    public float damageMultiplier;
}
