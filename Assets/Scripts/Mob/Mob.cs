using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(MobResources))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(MobAnimationController))]
[RequireComponent(typeof(MobCombat))]
public class Mob : MonoBehaviour,ICombatEntity
{
    private MobAI _ai;
    private MobMovement _movement;
    private MobResources _resources;
    private MobAnimationController _animation;
    private MobCombat _combat;
    private Enemy _enemyConfig;
    private CombatStats _combatStats;
    private float _currentSpeed;
    private float _healthMultiplier;
    private float _damageMultiplier;

    private const float SpeedSmoothing = 8f;

    private float _baseSpeed = 3.5f;
    private float _slowFactor = 1f;

    public bool IsDead { get; private set; }
    public bool IsStunned { get; private set; }
    public bool IsSlowed => _slowFactor < 0.999f;

    public CombatStats CombatStats => _combatStats ?? new CombatStats();
    public string Id => _enemyConfig != null ? _enemyConfig.id : name;

    public string Name => _enemyConfig != null ? _enemyConfig.id : name;
    public Enemy EnemyConfig => _enemyConfig;
    public MobCombat Combat => _combat;
    private ICombatEntity _lastAttacker;
    public void Initialize(MobSpawnData spawnData)
    {
        NavMeshAgent agent =
            GetComponent<NavMeshAgent>();

        _resources = GetComponent<MobResources>();
        _animation = GetComponent<MobAnimationController>();
        _combat = GetComponent<MobCombat>();
        _healthMultiplier = spawnData.HealthMultiplier;
        _damageMultiplier = spawnData.DamageMultiplier;
        _baseSpeed = agent.speed;
        ResolveEnemyConfig(spawnData);
        _resources?.Initialize(_enemyConfig, _healthMultiplier);

        _movement = new MobMovement(agent);

        float rotSpeed = _enemyConfig != null ? _enemyConfig.rotationSpeed : 300f;
        agent.angularSpeed = rotSpeed;

        _ai = new MobAI(
            this,
            _movement,
            spawnData);

        _ai.AggroRange = _enemyConfig != null ? _enemyConfig.aggroRange : 10f;
        _ai.LoseTargetRange = _enemyConfig != null ? _enemyConfig.loseTargetRange : 18f;
        _ai.AttackRange = _enemyConfig != null ? _enemyConfig.attackRange : 2f;

        EnemyHealthBarManager
        .Instance
        .Create(this);
        _resources.OnDeath += HandleDeath;
    }

    private void ResolveEnemyConfig(MobSpawnData spawnData)
    {
        _enemyConfig = null;
        _combatStats = null;

        EnemiesConfig enemiesConfig = ConfigBoostrap.Current != null
            ? ConfigBoostrap.Current.EnemiesConfig
            : null;

        if (enemiesConfig?.enemies == null || enemiesConfig.enemies.Length == 0)
        {
            BuildFallbackCombatStats();
            return;
        }

        string enemyId = spawnData != null && !string.IsNullOrWhiteSpace(spawnData.EnemyId)
            ? spawnData.EnemyId
            : enemiesConfig.enemies[0].id;

        for (int i = 0; i < enemiesConfig.enemies.Length; i++)
        {
            if (enemiesConfig.enemies[i].id == enemyId)
            {
                _enemyConfig = enemiesConfig.enemies[i];
                break;
            }
        }

        if (_enemyConfig == null)
            _enemyConfig = enemiesConfig.enemies[0];

        BuildCombatStatsFromEnemy();
    }

    private void BuildCombatStatsFromEnemy()
    {
        if (_enemyConfig == null)
        {
            BuildFallbackCombatStats();
            return;
        }

        if (_enemyConfig.combat != null)
        {
            _combatStats = new CombatStats
            {
                PhysicalAttack = Mathf.RoundToInt(_enemyConfig.combat.physicalAttack * _damageMultiplier),
                MagicAttack = Mathf.RoundToInt(_enemyConfig.combat.magicAttack * _damageMultiplier),
                PhysicalDefense = _enemyConfig.combat.physicalDefense,
                MagicDefense = _enemyConfig.combat.magicDefense,
                CriticalChance = _enemyConfig.combat.criticalChance,
                CriticalDamage = _enemyConfig.combat.criticalDamage
            };
            return;
        }

        BuildFallbackCombatStats();
    }

    private void BuildFallbackCombatStats()
    {
        int physicalAttack = Mathf.RoundToInt(
            (_enemyConfig != null ? _enemyConfig.damage : 0f) * _damageMultiplier);

        _combatStats = new CombatStats
        {
            PhysicalAttack = physicalAttack,
            MagicAttack = 0,
            PhysicalDefense = 0,
            MagicDefense = 0,
            CriticalChance = 0f,
            CriticalDamage = 1f
        };
    }

    private void Update()
    {
        if (IsDead) return;

        _ai?.Tick();

        float targetSpeed = _ai != null && !IsStunned ? _ai.TargetSpeed : 0f;
        _currentSpeed = Mathf.MoveTowards(
            _currentSpeed,
            targetSpeed,
            SpeedSmoothing * Time.deltaTime);
        _animation?.Move(_currentSpeed);
    }

    public void AddStun()
    {
        if (IsStunned || IsDead) return;
        IsStunned = true;

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            agent.isStopped = true;

        Debug.Log($"[Skills] {name} stuneado");
    }

    public void RemoveStun()
    {
        if (!IsStunned) return;
        IsStunned = false;

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            agent.isStopped = false;

        Debug.Log($"[Skills] {name} deja de estar stuneado");
    }

    public void AddSlow(float slowPercent)
    {
        _slowFactor = Mathf.Clamp01(_slowFactor * (1f - slowPercent));
        ApplyMobSpeed();
    }

    public void RemoveSlow(float slowPercent)
    {
        float inverse = slowPercent >= 1f ? 1f : 1f / (1f - slowPercent);
        _slowFactor = Mathf.Clamp01(_slowFactor * inverse);
        ApplyMobSpeed();
    }

    private void ApplyMobSpeed()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.speed = _baseSpeed * _slowFactor;
    }

    public void SetTarget(Transform target)
    {
        _ai.Target = target;
    }

    public void ClearTarget()
    {
        _ai.Target = null;
    }

    public void TakeDamage(DamageData damageData)
    {
        if (IsDead) return;

        if (_resources == null)
        {
            Debug.LogWarning($"MobResources no encontrado en {name}");
            return;
        }
        _lastAttacker = damageData.Source;
        int damage = damageData.FinalDamage > 0
            ? damageData.FinalDamage
            : damageData.BaseDamage;

        _resources.TakeDamage(damage, damageData.IsCritical);
    }

    private void HandleDeath()
    {
        IsDead = true;

        if (SpawnerManager.Instance != null)
        {
            SpawnerManager.Instance.OnMobDied(this);
        }

        if (_lastAttacker is Player player)
        {
            player.Progression.AddExperience(_enemyConfig.experience);
            MobDropHandler.RollDrops(this, player);
        }

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        _animation.PlayDeath(OnDeathAnimationComplete);
    }

    private void OnDeathAnimationComplete()
    {
        Destroy(gameObject);
    }
}
