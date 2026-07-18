using System;

[Serializable]
public class StatsConfig
{
    public StrengthConfig strength;
    public DexterityConfig Dexterity;
    public IntelligenceConfig intelligence;
    public VitalityConfig vitality;
}

[Serializable]
public class StrengthConfig
{
    public int damagePerPoint ;
}

[Serializable]
public class DexterityConfig
{
    public float attackSpeedPerPoint;
    public float criticalChancePerPoint;
}

[Serializable]
public class IntelligenceConfig
{
    public int manaPerPoint;
    public int spellDamagePerPoint;
}

[Serializable]
public class VitalityConfig
{
    public int healthPerPoint;
}