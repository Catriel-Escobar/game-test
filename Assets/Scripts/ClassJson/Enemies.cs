



using System;
[Serializable]
public class EnemiesConfig
{
    public Enemy[] enemies;
}
[Serializable]
public class Enemy
{
    public string id;
    public float health;
    public float damage;
    public float speed;
    public float experience;
    public float aggroRange;
    public float loseTargetRange;
    public float attackRange;
    public float rotationSpeed;
    public EnemyCombatConfig combat;
}

[Serializable]
public class EnemyCombatConfig
{
    public int physicalAttack;
    public int magicAttack;
    public int physicalDefense;
    public int magicDefense;
    public float criticalChance;
    public float criticalDamage;
}