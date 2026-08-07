


using System;
using UnityEngine;

public class PlayerResources : MonoBehaviour
{
    public int CurrentHp { get; private set; }
    public int MaxHp { get; private set; }

    public int CurrentMana { get; private set; }
    public int MaxMana { get; private set; }

    public bool IsDead => CurrentHp <= 0;
    private Player _player;
    public event Action OnDeath;
    public event Action<int,int>OnHealthChanged;
    public event Action<int,int>OnManaChanged;
    public event Action OnHit;
    // GAMEPLAY
    private PlayerStats _stats;
    // CONFIGS
    private PlayerResourcesConfig _resourcesConfigs;
    private StatsConfig _statsConfig;
    internal void Initialize(PlayerResourcesConfig baseResources,Player player)
    {
        _resourcesConfigs = baseResources;
        _statsConfig = player.StatsConfig;
        _stats = player.Stats;
        UpdateResources(_resourcesConfigs, _stats,_statsConfig);

        player.Progression.OnLevelChanged += UpdateResourcesByLevelUp;
    }
    private float _manaRegenAccumulator;

    private void Update()
    {
        if (CurrentHp <= 0)
        {
            Die();
            return;
        }

        RegenMana();
    }

    private void RegenMana()
    {
        float regenPerSecond = _resourcesConfigs != null ? _resourcesConfigs.manaRegenPerSecond : 0f;
        if (regenPerSecond <= 0f || CurrentMana >= MaxMana) return;

        _manaRegenAccumulator += regenPerSecond * Time.deltaTime;
        int regen = Mathf.FloorToInt(_manaRegenAccumulator);
        if (regen <= 0) return;

        _manaRegenAccumulator -= regen;
        CurrentMana = Mathf.Min(CurrentMana + regen, MaxMana);
        OnManaChanged.Invoke(CurrentMana, MaxMana);
    }
    public void TakeDamage(int damage)
    {
        CurrentHp -= damage;
        OnHealthChanged.Invoke(CurrentHp,MaxHp);
        if (CurrentHp <= 0)
        {
            CurrentHp = 0;
            Die();
        }else
        {
            OnHit.Invoke();
        }
    }

 

    public void Heal(int amount)
    {
        CurrentHp = Mathf.Min(
            CurrentHp + amount,
            MaxHp
        );
        OnHealthChanged.Invoke(CurrentHp,MaxHp);
    }

    public void UpdateMaxHP(int amount)
    {
        MaxHp +=amount;
        OnHealthChanged(CurrentHp,MaxHp);
    }

       public void UpdateMaxMP(int amount)
    {
        MaxMana +=amount;
        OnManaChanged(CurrentMana,MaxMana);
    }

    public void ConsumeMana(int amount)
    {
        CurrentMana = Mathf.Max(CurrentMana- amount, 0);
        OnManaChanged.Invoke(CurrentMana,MaxMana);
    }


    private void Die()
    {
        OnDeath?.Invoke();
    }

    //? ESCUCHA UN EVENTO (EN ESTE CASO SUBIR DE NIVEL PARA ACTUALIAR LA VIDA Y MANA MAXIMOS Y CURRENTS)
    private void UpdateResourcesByLevelUp(int arg1, double arg2, long arg3)
    {
       UpdateResources(_resourcesConfigs, _stats,_statsConfig);
       CurrentHp = MaxHp;
       CurrentMana = MaxMana;
       OnHealthChanged.Invoke(CurrentHp,MaxHp);
       OnManaChanged.Invoke(CurrentMana,MaxMana);
    }

    // ! HELPER
    private void UpdateResources(PlayerResourcesConfig baseResources, PlayerStats stats,StatsConfig statsConfig)
    {
        var initHP = baseResources.health + stats.Vitality * statsConfig.vitality.healthPerPoint;
        var initMP = baseResources.mana + stats.Intelligence * statsConfig.intelligence.manaPerPoint;
        MaxHp = initHP;
        MaxMana = initMP;
        CurrentHp = Mathf.Min(CurrentHp, MaxHp);
        CurrentMana = Mathf.Min(CurrentMana, MaxMana);
        if (CurrentHp <= 0) CurrentHp = MaxHp;
        if (CurrentMana <= 0) CurrentMana = MaxMana;
    }

    public void SetCurrentValues(int hp, int mana)
    {
        CurrentHp = Mathf.Clamp(hp, 0, MaxHp);
        CurrentMana = Mathf.Clamp(mana, 0, MaxMana);
        OnHealthChanged?.Invoke(CurrentHp, MaxHp);
        OnManaChanged?.Invoke(CurrentMana, MaxMana);
    }


}