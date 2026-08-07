using System.Collections;
using UnityEngine;

public class DamageAreaEffect : ISkillEffect
{
    private readonly SkillEffectDefinition _definition;

    public DamageAreaEffect(SkillEffectDefinition definition)
    {
        _definition = definition;
    }

    public IEnumerator Apply(SkillCastContext context)
    {
        float radius = Mathf.Max(0.1f, _definition.radius);
        Collider[] hits = Physics.OverlapSphere(context.Center, radius);
        CombatService combat = new CombatService();

        Attack attack = new Attack
        {
            damageMultiplier = _definition.damageMultiplier,
            damageType = _definition.damageType == 1
                ? AttackDamageType.Magical
                : AttackDamageType.Physical
        };

        for (int i = 0; i < hits.Length; i++)
        {
            if (!hits[i].TryGetComponent<Mob>(out Mob mob)) continue;
            if (mob.IsDead) continue;

            combat.Attack(context.Player, mob, attack);
        }

        yield break;
    }
}
