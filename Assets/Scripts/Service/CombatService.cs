using UnityEngine;

public class CombatService
{
 public void Attack(
        ICombatEntity attacker,
        ICombatEntity target,
        Attack attack)
    {
        int attackPower = attack.damageType == AttackDamageType.Physical
            ? attacker.CombatStats.PhysicalAttack
            : attacker.CombatStats.MagicAttack;

        int defense = attack.damageType == AttackDamageType.Physical
            ? target.CombatStats.PhysicalDefense
            : target.CombatStats.MagicDefense;

        int baseDamage = Mathf.Max(
            1,
            Mathf.RoundToInt(attackPower * attack.damageMultiplier) - defense);

        float critChance = attacker.CombatStats.CriticalChance;
        bool isCritical = Random.value < critChance;
        int finalDamage = isCritical
            ? Mathf.RoundToInt(baseDamage * attacker.CombatStats.CriticalDamage)
            : baseDamage;

        target.TakeDamage(new DamageData
        {
            BaseDamage = attackPower,
            FinalDamage = finalDamage,
            DamageType = attack.damageType,
            IsCritical = isCritical,
            Source = attacker
        });
    }

    private int CalculateDamage(Attack attack, PlayerStats attackerStats, StatsConfig statsConfig)
    {
        int strengthDamage = attackerStats.Strength * statsConfig.strength.damagePerPoint;
        float rawDamage = strengthDamage * attack.damageMultiplier;

        return Mathf.Max(1, Mathf.RoundToInt(rawDamage));
    }
}