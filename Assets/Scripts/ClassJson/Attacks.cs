using System;

[Serializable]
public class AttackConfig
{
    public Attack[] attacks;
}

[Serializable]
public class Attack
{
    public string id;
    public string animation_id;
    public float duration;
    public float damageMultiplier;
    public AttackDamageType damageType = AttackDamageType.Physical;
    public float range;
    public int manaCost;
    public float cooldown;
}

public enum     AttackDamageType
{
    Physical,
    Magical
}