using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, ICombatEntity,ITargetable
{
    public PlayerMovement Movement { get; private set; }
    public PlayerResources Resources { get; private set; }
    public PlayerInputs Inputs { get; private set; }
    public PlayerCombat Combat {get; private set;}

    public PlayerProgression Progression { get; private set; }
    public PlayerStats Stats { get; private set; }
    public PlayerAnimationController Animation { get; private set; }
    public PlayerSkills Skills { get; private set; }
    public SkillCaster Caster { get; private set; }

    public Dictionary<string,string> UnlockedAttackIds { get; private set; }

    private readonly Dictionary<string, float> _buffMultipliers = new Dictionary<string, float>();
    private float _damageReduction;
    private SkillUnlockService _skillUnlockService;

    public event Action<Vector3> OnDamageReduced;

    public float DamageReductionPercent => Mathf.Clamp01(_damageReduction);

    public float MovementSpeedMultiplier => GetBuffMultiplier("move_speed");

    public void AddDamageReduction(float percent)
    {
        _damageReduction += percent;
    }

    public void RemoveDamageReduction(float percent)
    {
        _damageReduction = Mathf.Max(0f, _damageReduction - percent);
    }

    //! CONFIGS
    public PlayerConfig PlayerConfig;
    public AttackConfig AttackConfig;
    public StatsConfig StatsConfig;
    public ProgressionConfig ProgressionConfig;
    public string Id => GetEntityId().ToString();
    public string Name => gameObject.name;
     public Transform Transform => transform;
    public bool IsAlive => !Resources.IsDead;
    public CombatStats CombatStats
    {
        get
        {
            if (Stats == null || StatsConfig == null || PlayerConfig == null)
                return new CombatStats();

            return new CombatStats
            {
                PhysicalAttack = Mathf.RoundToInt((Stats.Strength * StatsConfig.strength.damagePerPoint) * GetBuffMultiplier("physical_attack")),
                MagicAttack = Mathf.RoundToInt((Stats.Intelligence * StatsConfig.intelligence.spellDamagePerPoint) * GetBuffMultiplier("magical_attack")),
                PhysicalDefense = Mathf.RoundToInt((Stats.Vitality * StatsConfig.vitality.healthPerPoint) * GetBuffMultiplier("physical_defense")),
                MagicDefense = Mathf.RoundToInt((Stats.Intelligence * StatsConfig.intelligence.spellDamagePerPoint) * GetBuffMultiplier("magical_defense")),
                CriticalChance = (PlayerConfig.combat.criticalChance +
                                 (Stats.Dexterity * StatsConfig.Dexterity.criticalChancePerPoint)) * GetBuffMultiplier("critical_chance"),
                CriticalDamage = PlayerConfig.combat.criticalDamage * GetBuffMultiplier("critical_damage")
            };
        }
    }

    public void AddBuffMultiplier(string statId, float percent)
    {
        if (string.IsNullOrEmpty(statId)) return;

        _buffMultipliers.TryGetValue(statId, out float current);
        _buffMultipliers[statId] = current + percent;
    }

    public void RemoveBuffMultiplier(string statId, float percent)
    {
        if (string.IsNullOrEmpty(statId)) return;

        if (!_buffMultipliers.TryGetValue(statId, out float current))
            return;

        current -= percent;
        if (current <= 0.0001f)
            _buffMultipliers.Remove(statId);
        else
            _buffMultipliers[statId] = current;
    }

    private float GetBuffMultiplier(string statId)
    {
        return _buffMultipliers.TryGetValue(statId, out float value)
            ? 1f + value
            : 1f;
    }

    private void Awake()
    {
        // Componentes
        Inputs = GetComponent<PlayerInputs>();
        Movement = GetComponent<PlayerMovement>();
        Resources = GetComponent<PlayerResources>();
        Combat = GetComponent<PlayerCombat>();
        // Presentación
        Animation = GetComponent<PlayerAnimationController>();
    }

    public void Initialize(ConfigBoostrap config, PlayerSaveData saveData = null, string classId = null)
    {
        PlayerConfig = config.PlayerConfig;
        AttackConfig = config.AttackConfig;
        StatsConfig = config.StatsConfig;
        ProgressionConfig = config.ProgressionConfig;
        ProgressionConfig.BuildExperienceTable();

        Stats = new PlayerStats();
        Stats.Initialize(PlayerConfig.baseStats);
        Progression = new PlayerProgression(Stats);
        Progression.Initialize(ProgressionConfig);
        Movement.Initialize(PlayerConfig.movement);
        Combat.Initilizate(this);
        Combat.OnSwingStateChanged += Movement.AttackStateChanged;

        if (saveData != null)
        {
            Stats.SetStats(saveData.strength, saveData.vitality, saveData.intelligence, saveData.dexterity);
            Progression.SetState(saveData.level, saveData.currentExperience);

            UnlockedAttackIds = new Dictionary<string, string>();
            if (saveData.unlockedAttackIds != null)
            {
                for (int i = 0; i < saveData.unlockedAttackIds.Length; i++)
                {
                    string id = saveData.unlockedAttackIds[i];
                    UnlockedAttackIds[id] = id;
                }
            }

            Resources.Initialize(PlayerConfig.baseResources, this);
            Resources.SetCurrentValues(saveData.currentHp, saveData.currentMana);

            transform.position = saveData.Position;
            transform.rotation = saveData.Rotation;
        }
        else
        {
            UnlockedAttackIds = new Dictionary<string, string>
            {
                { PlayerConfig.startingAttack, PlayerConfig.startingAttack }
            };
            Resources.Initialize(PlayerConfig.baseResources, this);
        }

        Skills = new PlayerSkills();
        Skills.Initialize(classId, config.SkillsConfig, saveData?.unlockedSkillIds);

        if (!string.IsNullOrEmpty(classId))
        {
            _skillUnlockService = new SkillUnlockService(Skills, Progression);
            _skillUnlockService.Initialize();
        }

        Caster = GetComponent<SkillCaster>();
        if (Caster == null)
            Caster = gameObject.AddComponent<SkillCaster>();

        Caster.Initialize(this);
    }

    private void Start()
    {
        Resources.OnHit += Animation.OnHitAnimation;
    }
    private void Update()
    {
        Animation.Move(Movement.Speed);
    }

    public void TakeDamage(DamageData damageData)
    {
        if (Resources == null)
            return;

        int damage = damageData.FinalDamage > 0
            ? damageData.FinalDamage
            : damageData.BaseDamage;

        if (DamageReductionPercent > 0f)
        {
            damage = Mathf.Max(1, Mathf.RoundToInt(damage * (1f - DamageReductionPercent)));
            OnDamageReduced?.Invoke(GetHitPoint(damageData));
        }

        Resources.TakeDamage(damage);

    }

    private const float ShieldRadius = 1.2f;

    private Vector3 GetHitPoint(DamageData damageData)
    {
        if (damageData.Source is Component source)
        {
            Vector3 direction = transform.position - source.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
                direction = transform.forward;
            else
                direction.Normalize();

            return transform.position + direction * ShieldRadius;
        }

        return transform.position;
    }
}