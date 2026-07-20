using System.Collections.Generic;
using UnityEngine;

public class WaveMode : ISpawnMode
{
    private SpawnerConfig config;
    private System.Func<string, float, float, Mob> spawnCallback;
    private List<Mob> aliveMobs = new();
    private int currentWaveIndex;
    private int spawnsRemaining;
    private float spawnTimer;

    public int AliveCount => aliveMobs.Count;
    public int CurrentWave => currentWaveIndex + 1;
    public int TotalWaves => config?.waves != null ? config.waves.Count : 0;
    public bool AllWavesComplete { get; private set; }

    public event System.Action<int> OnWaveStarted;
    public event System.Action<int> OnWaveCompleted;
    public event System.Action OnAllWavesCompleted;

    public void Initialize(SpawnerConfig config, System.Func<string, float, float, Mob> spawnCallback)
    {
        this.config = config;
        this.spawnCallback = spawnCallback;
        currentWaveIndex = 0;
        AllWavesComplete = false;

        if (config.waves != null && config.waves.Count > 0)
        {
            StartWave(0);
        }
        else
        {
            AllWavesComplete = true;
        }
    }

    public void Update(float deltaTime)
    {
        if (AllWavesComplete) return;
        if (config.waves == null || currentWaveIndex >= config.waves.Count) return;

        WaveConfig wave = config.waves[currentWaveIndex];

        spawnTimer += deltaTime;

        if (spawnTimer >= wave.spawnInterval && aliveMobs.Count < config.maxAlive && spawnsRemaining > 0)
        {
            string enemyId = WeightedRandomSelector.Select(wave.enemyTypes);
            if (!string.IsNullOrEmpty(enemyId))
            {
                Mob mob = spawnCallback(enemyId, wave.healthMultiplier, wave.damageMultiplier);
                if (mob != null)
                {
                    aliveMobs.Add(mob);
                    spawnsRemaining--;
                }
            }
            spawnTimer = 0f;
        }

        if (spawnsRemaining == 0 && aliveMobs.Count == 0)
        {
            OnWaveCompleted?.Invoke(currentWaveIndex);
            currentWaveIndex++;

            if (currentWaveIndex < config.waves.Count)
            {
                StartWave(currentWaveIndex);
            }
            else
            {
                AllWavesComplete = true;
                OnAllWavesCompleted?.Invoke();
            }
        }
    }

    public void OnMobDied(Mob mob)
    {
        aliveMobs.Remove(mob);
    }

    private void StartWave(int index)
    {
        WaveConfig wave = config.waves[index];
        spawnsRemaining = wave.totalSpawns;
        spawnTimer = wave.spawnInterval;
        OnWaveStarted?.Invoke(index);
    }
}
