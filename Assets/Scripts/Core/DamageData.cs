public class DamageData
{
    public DamageData()
    {
    }

    public int BaseDamage { get; set; }
    public int FinalDamage { get; set; }
    public AttackDamageType DamageType { get; set; } = AttackDamageType.Physical;
    public bool IsCritical { get; set; }
    public ICombatEntity Source { get; set; }
    
}


public enum DamageType
{
    Physical,
    Magical,
    Fire,
    Ice,
    Lightning,
    Poison,
    Holy,
    Dark,
    True
}