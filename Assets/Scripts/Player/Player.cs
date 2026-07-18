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

    public Dictionary<string,string> UnlockedAttackIds { get; private set; }

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
                PhysicalAttack = Stats.Strength * StatsConfig.strength.damagePerPoint,
                MagicAttack = Stats.Intelligence * StatsConfig.intelligence.spellDamagePerPoint,
                PhysicalDefense = Stats.Vitality * StatsConfig.vitality.healthPerPoint,
                MagicDefense = Stats.Intelligence * StatsConfig.intelligence.spellDamagePerPoint,
                CriticalChance = PlayerConfig.combat.criticalChance +
                                 (Stats.Dexterity * StatsConfig.Dexterity.criticalChancePerPoint),
                CriticalDamage = PlayerConfig.combat.criticalDamage
            };
        }
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

    public void Initialize(ConfigBoostrap config)
    {
        PlayerConfig = config.PlayerConfig;
        AttackConfig = config.AttackConfig;
        StatsConfig = config.StatsConfig;
        ProgressionConfig = config.ProgressionConfig;
        ProgressionConfig.BuildExperienceTable();


        // asd
        UnlockedAttackIds = new Dictionary<string, string>
        {
            { PlayerConfig.startingAttack, PlayerConfig.startingAttack }
        };
        // Dominio
        Stats = new PlayerStats();
        Stats.Initialize(PlayerConfig.baseStats);
        Progression = new PlayerProgression(Stats);
        Progression.Initialize(ProgressionConfig);
        Movement.Initialize(PlayerConfig.movement);
        Stats.Initialize(PlayerConfig.baseStats);
        Resources.Initialize(PlayerConfig.baseResources,this);
        Combat.Initilizate(this);
        Combat.OnAttackStateChanged += Movement.AttackStateChanged;
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

        Resources.TakeDamage(damage);

    }
}