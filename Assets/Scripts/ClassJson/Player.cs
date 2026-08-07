using System;

[Serializable]
public class PlayerConfig
{
    public PlayerStatsConfig baseStats;
    public PlayerResourcesConfig baseResources;
    public PlayerMovementConfig movement;
    public PlayerCombatConfig combat;
    public string startingAttack;
}

[Serializable]
public class PlayerStatsConfig
{
    public int strength;
    public int vitality;
    public int intelligence;
    public int dexterity;
}

[Serializable]
public class PlayerResourcesConfig
{
    public int health;
    public int mana;
    public float manaRegenPerSecond;
}

[Serializable]
public class PlayerMovementConfig
{
    public float gravity;
    public float speed;
    public float rotationSpeed;
}

[Serializable]
public class PlayerCombatConfig
{
    public float attackSpeed;
    public float criticalChance;
    public float criticalDamage;
}